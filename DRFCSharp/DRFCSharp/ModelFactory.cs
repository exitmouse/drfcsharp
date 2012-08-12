using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
    /// <summary>
    /// Stores all necessary "hyperparameters" needed to train and thereby build models.
    /// </summary>
    public class ModelFactory
    {
        public CrossFeatureStrategy Crosser { get; private set; }
        public TransformFeatureStrategy Transformer { get; private set; }
        public double Tau { get; private set; }
        public int MaxIters { get; private set; }
        public int Iters { get; private set; }
        public double StartStepLength { get; private set; }
        public double LikelihoodConvergence { get; private set; }
        public DenseVector W { get; private set; }
        public DenseVector V { get; private set; }
        public List<ImageData> TrainingInputs { get; private set; }
        public List<Classification> TrainingOutputs { get; private set; }
        public ModelFactory(Builder builder)/*{{{*/
        {
            this.Crosser = builder.Crosser;
            this.Transformer = builder.Transformer;
            this.Tau = builder.Tau;
            this.MaxIters = builder.MaxIters;
            this.Iters = 0;
            this.StartStepLength = builder.StartStepLength;
            this.LikelihoodConvergence = builder.LikelihoodConvergence;
            this.W = builder.W;
            this.V = builder.V;
        }/*}}}*/

        public class Builder/*{{{*/
        {
            public CrossFeatureStrategy Crosser { get; private set; }
            public TransformFeatureStrategy Transformer { get; private set; }
            public double Tau { get; private set; }
            public int MaxIters { get; private set; }
            public double StartStepLength { get; private set; }
            public double LikelihoodConvergence { get; private set; }
            public DenseVector W { get; private set; }
            public DenseVector V { get; private set; }
            public List<ImageData> TrainingInputs { get; private set; }
            public List<Classification> TrainingOutputs { get; private set; }

            public Builder(List<ImageData> inputs, List<Classification> outputs){
                Crosser = ConcatenateFeatures.INSTANCE;
                Transformer = LinearBasis.INSTANCE;
                Tau = 0.0001d;
                MaxIters = 3000;
                StartStepLength = 1d;
                LikelihoodConvergence = 1d;

                if(inputs.Count != outputs.Count) throw new ArgumentException("Different number of training inputs and outputs");
                if(inputs.Count == 0) throw new ArgumentException("There must be at least one training image.");
                TrainingInputs = inputs;
                TrainingOutputs = outputs;

                RecalculateWV();
            }

            private void RecalculateWV(){
                //TODO: Assert that these are all the same across the entire ImageData array--do it by having a class that's a wrapper around ImageData[]s and
                //one that's a wrapper around Classification[]s.
                W = new DenseVector(Transformer.Components(TrainingInputs[0].FeatureCount), 0d);
                V = new DenseVector(Crosser.Components(TrainingInputs[0].FeatureCount), 0d);
            }

            public Builder SetCrosser(CrossFeatureStrategy val) { Crosser = val; RecalculateWV(); return this; }

            public Builder SetTransformer(TransformFeatureStrategy val) { Transformer = val; RecalculateWV(); return this; }

            public Builder SetTau(double val)
            { 
                if(val <= 0) throw new ArgumentException("Tau must be positive");
                Tau = val; 
                return this; 
            }

            public Builder SetMaxIters(int val) { MaxIters = val; return this; }

            public Builder SetStartStepLength(double val) { StartStepLength = val; return this; }

            public Builder SetLikelihoodConvergence(double val) { LikelihoodConvergence = val; return this; }

            public Builder SetW(DenseVector val) { W = val; return this; }

            public Builder SetV(DenseVector val) { V = val; return this; }

            public ModelFactory Build() { return new ModelFactory(this); }
        }/*}}}*/

        //Does this really belong here? It seems unlikely now that we've refactored
        public static ModelFactory Deserialize(string path)
        {
            SoapFormatter serializer = new SoapFormatter();
            if(!string.IsNullOrEmpty(path))
            {
                Stream parameter_storage_in = new FileStream(path, FileMode.Open);
                return (ModelFactory)serializer.Deserialize(parameter_storage_in);
            } else
            {
                throw new ArgumentException("Path must not be null or empty");
            }
        }

        public void Serialize(string path)
        {
            SoapFormatter serializer = new SoapFormatter();
            if(!string.IsNullOrEmpty(path))
            {
                Stream parameter_storage_out = new FileStream(path, FileMode.Create);
                serializer.Serialize(parameter_storage_out, this);
                parameter_storage_out.Close();
            } else
            {
                throw new ArgumentException("Path must not be null or empty");
            }
        }

        public Model PseudoLikelihoodTrain()
        {
            while(Iterate());
            return new Model(this);
        }

        public Model PseudoLikelihoodTrainAndSave(string path)
        {
            while(Iterate()) Serialize(path);
            return new Model(this);
        }

        private DenseVector CalculateWGradient()
        {
            DenseVector wgrad = new DenseVector(W.Count);
            wgrad.Clear(); //TODO test if this is necessary

            for(int k = 0; k < wgrad.Count; k++)
            {
                for(int m = 0; m < TrainingInputs.Count; m++)
                {
                    for(int horz = 0; horz < TrainingInputs[m].XSites; horz++)
                    {
                        for(int vert = 0; vert < TrainingInputs[m].YSites; vert++)
                        {

                            //h_i(y):
                            DenseVector h = Transformer.Transform(TrainingInputs[m][horz,vert]);
                            if(double.IsNaN(h[k])) throw new NotFiniteNumberException();

                            //x_i
                            int x = (int)(TrainingOutputs[m][horz,vert])*2 - 1;


                            //sigma term that keeps reappearing
                            double sig = MathWrapper.Sigma(x * W.DotProduct(h));
                            //x_i * h_i(y)_k * (1 - sigma(x_i * w^T h_i(y)))
                            //double old = wgrad[k];
                            wgrad[k] += x*h[k]*(1 - sig);

                            if(double.IsNaN(wgrad[k])) throw new NotFiniteNumberException();

                            //- ((d/(dw_k)) z_i) / z_i
                            double z = 0;
                            double dzdw = 0;


                            //Sum over possible x_i
                            for(int tempx = -1; tempx <= 1; tempx += 2)
                            {
                                double logofcoeff = MathWrapper.Log(MathWrapper.Sigma(tempx * W.DotProduct(h)));
                                //sum over the neighbors
                                foreach(Tuple<int,int> j in TrainingInputs[m].GetNeighbors(horz,vert))
                                {
                                    int jx = (int)(TrainingOutputs[m][j.Item1,j.Item2])*2 - 1;
                                    DenseVector mu;
                                    if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(TrainingInputs[m][horz,vert],TrainingInputs[m][j.Item1,j.Item2]);
                                    else mu = Crosser.Cross(TrainingInputs[m][j.Item1,j.Item2], TrainingInputs[m][horz,vert]);
                                    logofcoeff += tempx * jx * V.DotProduct(mu);
                                }
                                double coeff = MathWrapper.Exp(logofcoeff);
                                z += coeff;
                                double multfactor = (1 - MathWrapper.Sigma (tempx * W.DotProduct(h)));
                                dzdw += coeff * tempx*h[k]*multfactor;

                                if(double.IsNaN(dzdw)||double.IsNaN(z)||double.IsInfinity(dzdw)||double.IsInfinity(z)) throw new NotFiniteNumberException();
                            }

                            if(z <= 0d) throw new NotFiniteNumberException();

                            wgrad[k] -= dzdw/z;

                            if(double.IsNaN(wgrad[k])) throw new NotFiniteNumberException();
                            if(double.IsInfinity(wgrad[k])) throw new NotFiniteNumberException();
                        }
                    }
                }
            }
            return wgrad;
        }

        private DenseVector CalculateVGradient()
        {
            DenseVector vgrad = new DenseVector(V.Count);
            vgrad.Clear(); //TODO test if this is necessary
            for(int k = 0; k < vgrad.Count; k++)
            {
                for(int m = 0; m < TrainingInputs.Count; m++)
                {
                    for(int horz = 0; horz < TrainingInputs[m].XSites; horz++)
                    {
                        for(int vert = 0; vert < TrainingInputs[m].YSites; vert++)
                        {

                            //vgrad[k] = sum over image sites in all images of
                            //[ sum over image sites of x_i x_j (mu_ij (y))_k] <- vterm
                            //-
                            //[dzdv]/[z]
                            //
                            //all minus v_k/tau^2
                            double z = 0;
                            //x_i
                            int x = (int)(TrainingOutputs[m][horz,vert])*2 - 1;

                            //Sum over neighbors of x_i x_j mu_{ij}(y)_k
                            double vterm = 0;
                            foreach(Tuple<int,int> j in TrainingInputs[m].GetNeighbors(horz, vert))
                            {
                                int jx = (int)(TrainingOutputs[m][j.Item1,j.Item2])*2 - 1;
                                DenseVector mu;
                                if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(TrainingInputs[m][horz,vert],TrainingInputs[m][j.Item1,j.Item2]);
                                else mu = Crosser.Cross(TrainingInputs[m][j.Item1,j.Item2], TrainingInputs[m][horz,vert]);
                                vterm += x * jx * mu[k];
                            }
                            vgrad[k] += vterm;
                            double dzdv = 0;


                            //h_i(y):
                            DenseVector h = Transformer.Transform(TrainingInputs[m][horz,vert]);

                            //Sum over possible x_i	
                            for(int tempx = -1; tempx <= 1; tempx += 2)
                            {
                                double logofcoeff = MathWrapper.Log(MathWrapper.Sigma(tempx * W.DotProduct(h)));
                                double dzdvterm = 0;
                                //sum over the neighbors
                                foreach(Tuple<int,int> j in TrainingInputs[m].GetNeighbors(horz,vert))
                                {
                                    int jx = (int)(TrainingOutputs[m][j.Item1,j.Item2])*2 - 1;
                                    DenseVector mu;
                                    if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(TrainingInputs[m][horz,vert],TrainingInputs[m][j.Item1,j.Item2]);
                                    else mu = Crosser.Cross(TrainingInputs[m][j.Item1,j.Item2], TrainingInputs[m][horz,vert]);
                                    logofcoeff += tempx * jx * V.DotProduct(mu);
                                    dzdvterm += tempx * jx * mu[k];
                                }
                                double coeff = MathWrapper.Exp(logofcoeff);
                                z += coeff;
                                dzdv += coeff * dzdvterm;
                                if(double.IsNaN(dzdv)||double.IsNaN(z)||double.IsInfinity(dzdv)||double.IsInfinity(z)) throw new NotFiniteNumberException();
                            }

                            if(z <= 0d) throw new NotFiniteNumberException();

                            vgrad[k] -= dzdv/z;
                        }
                    }
                }
                vgrad[k] -= V[k]/(Math.Pow (Tau,2));
            }
            return vgrad;
        }
        private double PseudoLikelihood(DenseVector wtest, DenseVector vtest)
        {
            double first_term = 0;
            for(int m = 0; m < TrainingInputs.Count; m++)
            {
                for(int horz = 0; horz < TrainingInputs[m].XSites; horz++)
                {
                    for(int vert = 0; vert < TrainingInputs[m].YSites; vert++)
                    {
                        int x = (int)(TrainingOutputs[m][horz,vert])*2 - 1;

                        //h_i(y):
                        DenseVector h = Transformer.Transform(TrainingInputs[m][horz,vert]);
                        first_term += MathWrapper.Log (MathWrapper.Sigma( x * wtest.DotProduct(h) ) );
                        foreach(Tuple<int,int> j in TrainingInputs[m].GetNeighbors(horz,vert))
                        {
                            int jx = (int)(TrainingOutputs[m][j.Item1,j.Item2])*2 - 1;
                            DenseVector mu;
                            if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(TrainingInputs[m][horz,vert],TrainingInputs[m][j.Item1,j.Item2]);
                            else mu = Crosser.Cross(TrainingInputs[m][j.Item1,j.Item2], TrainingInputs[m][horz,vert]);
                            first_term += x * jx * vtest.DotProduct(mu);
                        }
                        double z = 0;
                        //Sum over possible x_i
                        for(int tempx = -1; tempx <= 1; tempx += 2)
                        {
                            double logofcoeff = MathWrapper.Log(MathWrapper.Sigma(tempx * wtest.DotProduct(h)));
                            //sum over the neighbors
                            foreach(Tuple<int,int> j in TrainingInputs[m].GetNeighbors(horz,vert))
                            {
                                int jx = (int)(TrainingOutputs[m][j.Item1,j.Item2])*2 - 1;
                                DenseVector mu;
                                if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(TrainingInputs[m][horz,vert],TrainingInputs[m][j.Item1,j.Item2]);
                                else mu = Crosser.Cross(TrainingInputs[m][j.Item1,j.Item2], TrainingInputs[m][horz,vert]);
                                logofcoeff += tempx * jx * vtest.DotProduct(mu);
                            }
                            z += MathWrapper.Exp (logofcoeff);
                        }
                        first_term -= MathWrapper.Log(z);
                    }
                }
            }
            return first_term - vtest.DotProduct(vtest)/(2*Math.Pow(Tau,2d));
        }
        //Iterates the model, and returns false if the iteration has converged.
        private bool Iterate()
        {
            if(Iters >= MaxIters) return false;
            Iters++;
            
            DenseVector wgrad = CalculateWGradient();
            DenseVector vgrad = CalculateVGradient();

            double normwgrad = wgrad.Norm(2d);
            double normvgrad = vgrad.Norm(2d);
            double sumofnorms = normwgrad + normvgrad;
            Console.WriteLine("\t\t\t\t\tL2 Norms Summed: {0}",sumofnorms);

            //Compute best step length
            double a = StartStepLength;
            double oldlikelihood = PseudoLikelihood(W,V);
            double newlikelihood = 0;
            while(true)
            {
                newlikelihood = PseudoLikelihood(W + (DenseVector)wgrad.Multiply(a), V + (DenseVector)vgrad.Multiply(a));
                if(newlikelihood > oldlikelihood)
                {
                    Console.WriteLine ("Likelihood after this step: {0}", newlikelihood);
                    break;
                }
                a = a/2;
                if(a*sumofnorms < double.Epsilon)
                {
                    //Well, we can't go any lower. Numerical error; we should break and quit training.
                    break;
                }
            }

            W += (DenseVector)wgrad.Multiply(a);
            V += (DenseVector)vgrad.Multiply(a);

            if(newlikelihood - oldlikelihood < LikelihoodConvergence)
            {
                Console.WriteLine("New likelihood - Old likelihood is {0}; Converged.",newlikelihood-oldlikelihood);
                return false;
            }

            return true;

        }
    }
}

