using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ModifiedModel
	{
		DenseVector w;
		DenseVector v;
		public const int MAX_ITERS = 30000;
		public const double CONVERGENCE_CONSTANT = 1;
		public const double START_STEP_LENGTH = 200d;
		public const double MIN_STEP_LENGTH = 0.000000000001d; //TODO all these small thingies are hacks
		public const double EPSILON = 0.000000001d;
		public readonly int time_to_converge;
		
		public ModifiedModel (DenseVector w, DenseVector v, int iter_count)
		{
			this.w = w;
			this.v = v;
			this.time_to_converge = iter_count;
		}
		
		public Classification MaximumAPosterioriInfer(ImageData test_input)
		{
			throw new NotImplementedException();
		}
		
		public static ModifiedModel PseudoLikelihoodTrain(ImageData[] training_inputs, Classification[] training_outputs, double tau)
		{
			if(tau <= 0) throw new ArgumentException("Tau must be positive");
			if(training_inputs.Length != training_outputs.Length) throw new ArgumentException("Different number of training inputs and outputs");
			
			DenseVector w = new DenseVector(SiteFeatureSet.NUM_FEATURES + 1, 1d);
			DenseVector v = new DenseVector(SiteFeatureSet.NUM_FEATURES*2 + 1, 1d);
			
			DenseVector wgrad = new DenseVector(w.Count);
			DenseVector vgrad = new DenseVector(v.Count);
			
			int iter_count = 0;
			while(iter_count < MAX_ITERS)
			{
				wgrad.Clear();
				vgrad.Clear();
				
				//Compute gradients
				
				//TODO go over this code with a fine tooth comb
				for(int k = 0; k < wgrad.Count; k++)
				{
					for(int m = 0; m < training_inputs.Length; m++)
					{
						for(int horz = 0; horz < ImageData.x_sites; horz++)
						{
							for(int vert = 0; vert < ImageData.y_sites; vert++)
							{

								//h_i(y):
								DenseVector h = SiteFeatureSet.TransformedFeatureVector(training_inputs[m][horz,vert]);
	
								//x_i
								int x = (int)(training_outputs[m][horz,vert])*2 - 1;
								
								//sigma term that keeps reappearing
								double sig = Sigma(x * w.DotProduct(h));
								//x_i * h_i(y)_k * (1 - sigma(x_i * w^T h_i(y)))
								wgrad[k] += x*h[k]*(1 - sig);
								
								//- ((d/(dw_k)) z_i) / z_i
								double z = 0;
								double dzdw = 0;

								
								//Sum over possible x_i
								for(int tempx = -1; tempx <= 1; tempx += 2)
								{
									double logofcoeff = Log(Sigma(tempx * w.DotProduct(h)));
									//sum over the neighbors
									foreach(Tuple<int,int> j in ImageData.GetNeighbors(horz,vert))
									{
										int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
										DenseVector mu = SiteFeatureSet.CrossFeatures(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
										logofcoeff += tempx * jx * v.DotProduct(mu);
									}
									double coeff = Exp(logofcoeff);
									z += coeff;
									dzdw += coeff * tempx*h[k]/Sigma(tempx * w.DotProduct(h));
								}
								if(z <= 0d)
								{
									throw new NotFiniteNumberException();
								}
								wgrad[k] -= dzdw/z;
							}
						}
					}
				}
				for(int k = 0; k < vgrad.Count; k++)
				{
					for(int m = 0; m < training_inputs.Length; m++)
					{
						for(int horz = 0; horz < ImageData.x_sites; horz++)
						{
							for(int vert = 0; vert < ImageData.y_sites; vert++)
							{
								//h_i(y):
								DenseVector h = SiteFeatureSet.TransformedFeatureVector(training_inputs[m][horz,vert]);
								double z = 0;
								//x_i
								int x = (int)(training_outputs[m][horz,vert])*2 - 1;
								
								//Sum over neighbors of x_i x_j mu_{ij}(y)_k
								double vterm = 0;
								foreach(Tuple<int,int> j in ImageData.GetNeighbors(horz, vert))
								{
									int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
									DenseVector mu = SiteFeatureSet.CrossFeatures(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
									vterm += x * jx * mu[k];
								}
								vgrad[k] += vterm;
								double dzdv = 0;
								
								//Sum over possible x_i
								for(int tempx = -1; tempx <= 1; tempx += 2)
								{
									double logofcoeff = Log(Sigma(tempx * w.DotProduct(h)));
									double dzdvterm = 0;
									//sum over the neighbors
									foreach(Tuple<int,int> j in ImageData.GetNeighbors(horz,vert))
									{
										int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
										DenseVector mu = SiteFeatureSet.CrossFeatures(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
										logofcoeff += tempx * jx * v.DotProduct(mu);
										dzdvterm += tempx * jx * mu[k];
									}
									double coeff = Exp(logofcoeff);
									z += coeff;
									dzdv += coeff * dzdvterm;
								}
								if(z <= 0d)
								{
									throw new NotFiniteNumberException();
								}
								vgrad[k] -= dzdv/z;
							}
						}
					}
					vgrad[k] -= v[k]/(Math.Pow (tau,2));
				}
				double normw = wgrad.Norm(1d);
				double normv = vgrad.Norm(1d);
				//Check for convergence
				if(normw + normv < CONVERGENCE_CONSTANT)
				{
					break;
				}
				
				//Compute best step length
				double a = START_STEP_LENGTH;
				while(PseudoLikelihood(w + (DenseVector)wgrad.Multiply(a), v + (DenseVector)vgrad.Multiply(a), training_inputs, training_outputs, tau) < PseudoLikelihood(w,v, training_inputs, training_outputs, tau))
				{
					a = a/2;
				}
				if( a < MIN_STEP_LENGTH)
				{
					break;
				}
				//Step
				w += (DenseVector)wgrad.Multiply(a);
				v += (DenseVector)vgrad.Multiply(a);
				iter_count++;
			}
			return new ModifiedModel(w,v,iter_count);
			
		}
		public static double PseudoLikelihood(DenseVector wtest, DenseVector vtest, ImageData[] training_inputs, Classification[] training_outputs, double tau)
		{
			double first_term = 0;
			for(int m = 0; m < training_inputs.Length; m++)
			{
				for(int horz = 0; horz < ImageData.x_sites; horz++)
				{
					for(int vert = 0; vert < ImageData.y_sites; vert++)
					{
						int x = (int)(training_outputs[m][horz,vert])*2 - 1;
						
						//h_i(y):
						DenseVector h = SiteFeatureSet.TransformedFeatureVector(training_inputs[m][horz,vert]);
						first_term += Log (Sigma( x * wtest.DotProduct(h) ) );
						foreach(Tuple<int,int> j in ImageData.GetNeighbors(horz,vert))
						{
							int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
							DenseVector mu = SiteFeatureSet.CrossFeatures(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
							first_term += x * jx * vtest.DotProduct(mu);
						}
						double z = 0;
						//Sum over possible x_i
						for(int tempx = -1; tempx <= 1; tempx += 2)
						{
							double logofcoeff = Log(Sigma(tempx * wtest.DotProduct(h)));
							//sum over the neighbors
							foreach(Tuple<int,int> j in ImageData.GetNeighbors(horz,vert))
							{
								int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
								DenseVector mu = SiteFeatureSet.CrossFeatures(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
								logofcoeff += tempx * jx * vtest.DotProduct(mu);
							}
							z += Exp (logofcoeff);
						}
						first_term -= Log(z);
					}
				}
			}
			return first_term - vtest.DotProduct(vtest)/(2*Math.Pow(tau,2d));
		}
		public static double Exp(double x){ return Math.Exp(x); }
		public static double Log(double x){ 
			if(x <= EPSILON)
			{
				return Math.Log (EPSILON);
			}
			return Math.Log(x);
		}
		public static double Sigma(double x){
			double result = 1/(1+ Exp(x));
			if(result < 0 || result > 1)
			{
				throw new NotFiniteNumberException("Sigma should be between 0 and 1");
			}
			if(result < EPSILON) return EPSILON;
			return result; 
		}
	}
}

