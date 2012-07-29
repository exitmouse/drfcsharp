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
	public class ModelBuilder
	{
		public CrossFeatureStrategy Crosser { get; private set; }
		public TransformFeatureStrategy Transformer { get; private set; }
		public double Tau { get; private set; }
		public int MaxIters { get; private set; }
		public double StartStepLength { get; private set; }
		public double LikelihoodConvergence { get; private set; }
		public ModelBuilder(CrossFeatureStrategy crosser, TransformFeatureStrategy transformer, double tau, int max_iters, double start_len, double likelihood_converge)
		{
			if(tau <= 0) throw new ArgumentException("Tau must be positive");
			this.Crosser = crosser;
			this.Transformer = transformer;
			this.Tau = tau;
			this.MaxIters = max_iters;
			this.StartStepLength = start_len;
			this.LikelihoodConvergence = likelihood_converge;
		}
		public Model PseudoLikelihoodTrain(string params_in, string params_out, ImageData[] training_inputs, Classification[] training_outputs)
		{
			if(training_inputs.Length != training_outputs.Length) throw new ArgumentException("Different number of training inputs and outputs");

			//TODO: Assert that these are all the same across the entire ImageData array--do it by having a class that's a wrapper around ImageData[]s and
			//one that's a wrapper around Classification[]s.
			DenseVector w = new DenseVector(Transformer.Components(training_inputs[0].FeatureCount), 0d);
			DenseVector v = new DenseVector(Crosser.Components(training_inputs[0].FeatureCount), 0d);
			
			DenseVector wgrad = new DenseVector(w.Count);
			DenseVector vgrad = new DenseVector(v.Count);
			
			SoapFormatter serializer = new SoapFormatter();
			
			if(!string.IsNullOrEmpty(params_in))
			{
				string parameter_storage_in_path = string.Format("{0}../../../../Dataset/{1}",AppDomain.CurrentDomain.BaseDirectory,params_in);
				Stream parameter_storage_in = new FileStream(parameter_storage_in_path,FileMode.Open);
				w = (DenseVector)serializer.Deserialize(parameter_storage_in);
				v = (DenseVector)serializer.Deserialize(parameter_storage_in);
			}
			Stream parameter_storage_out = null;
			string parameter_storage_out_path = "";
			if(!string.IsNullOrEmpty(params_out))
			{
				parameter_storage_out_path = string.Format("{0}../../../../Dataset/{1}",AppDomain.CurrentDomain.BaseDirectory,params_out);
			}
			
			int iter_count = 0;
			while(iter_count < MaxIters)
			{
				wgrad.Clear();
				vgrad.Clear();
				
				//Compute gradients
				for(int k = 0; k < wgrad.Count; k++)
				{
					for(int m = 0; m < training_inputs.Length; m++)
					{
						for(int horz = 0; horz < training_inputs[m].XSites; horz++)
						{
							for(int vert = 0; vert < training_inputs[m].YSites; vert++)
							{

								//h_i(y):
								DenseVector h = Transformer.Transform(training_inputs[m][horz,vert]);
	
								//x_i
								int x = (int)(training_outputs[m][horz,vert])*2 - 1;
								double hk = h[k];
								
								if(double.IsNaN(hk)) throw new NotFiniteNumberException();
									
								//sigma term that keeps reappearing
								double sig = MathWrapper.Sigma(x * w.DotProduct(h));
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
									double logofcoeff = MathWrapper.Log(MathWrapper.Sigma(tempx * w.DotProduct(h)));
									//sum over the neighbors
									foreach(Tuple<int,int> j in training_inputs[m].GetNeighbors(horz,vert))
									{
										int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
										DenseVector mu;
										if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
										else mu = Crosser.Cross(training_inputs[m][j.Item1,j.Item2], training_inputs[m][horz,vert]);
										logofcoeff += tempx * jx * v.DotProduct(mu);
									}
									double coeff = MathWrapper.Exp(logofcoeff);
									z += coeff;
									double multfactor = (1 - MathWrapper.Sigma (tempx * w.DotProduct(h)));
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
				for(int k = 0; k < vgrad.Count; k++)
				{
					for(int m = 0; m < training_inputs.Length; m++)
					{
						for(int horz = 0; horz < training_inputs[m].XSites; horz++)
						{
							for(int vert = 0; vert < training_inputs[m].YSites; vert++)
							{
								
								//vgrad[k] = sum over image sites in all images of
								//[ sum over image sites of x_i x_j (mu_ij (y))_k] <- vterm
								//-
								//[dzdv]/[z]
								//
								//all minus v_k/tau^2
								double z = 0;
								//x_i
								int x = (int)(training_outputs[m][horz,vert])*2 - 1;
								
								//Sum over neighbors of x_i x_j mu_{ij}(y)_k
								double vterm = 0;
								foreach(Tuple<int,int> j in training_inputs[m].GetNeighbors(horz, vert))
								{
									int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
									DenseVector mu;
									if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
									else mu = Crosser.Cross(training_inputs[m][j.Item1,j.Item2], training_inputs[m][horz,vert]);
									vterm += x * jx * mu[k];
								}
								vgrad[k] += vterm;
								double dzdv = 0;
								
							
								//h_i(y):
								DenseVector h = Transformer.Transform(training_inputs[m][horz,vert]);
								
								//Sum over possible x_i	
								for(int tempx = -1; tempx <= 1; tempx += 2)
								{
									double logofcoeff = MathWrapper.Log(MathWrapper.Sigma(tempx * w.DotProduct(h)));
									double dzdvterm = 0;
									//sum over the neighbors
									foreach(Tuple<int,int> j in training_inputs[m].GetNeighbors(horz,vert))
									{
										int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
										DenseVector mu;
										if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
										else mu = Crosser.Cross(training_inputs[m][j.Item1,j.Item2], training_inputs[m][horz,vert]);
										logofcoeff += tempx * jx * v.DotProduct(mu);
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
					vgrad[k] -= v[k]/(Math.Pow (Tau,2));
				}
				double normwgrad = wgrad.Norm(2d);
				double normvgrad = vgrad.Norm(2d);
				double sumofnorms = normwgrad + normvgrad;
				Console.WriteLine("\t\t\t\t\tL2 Norms Summed: {0}",sumofnorms);
				
				//Compute best step length
				double a = StartStepLength;
				double oldlikelihood = PseudoLikelihood(w,v, training_inputs, training_outputs);
				double newlikelihood = 0;
				while(true)
				{
					newlikelihood = PseudoLikelihood(w + (DenseVector)wgrad.Multiply(a), v + (DenseVector)vgrad.Multiply(a), training_inputs, training_outputs);
					if(newlikelihood > oldlikelihood)
					{
						Console.WriteLine ("Likelihood after this step: {0}",newlikelihood);
						break;
					}
					a = a/2;
					if(a*sumofnorms < double.Epsilon)
					{
						//Well, we can't go any lower. Numerical error; we should break and quit training.
						break;
					}
				}
				//Console.WriteLine ("Step length: {0}",a*sumofnorms);
				/*if( a*sumofnorms < CONVERGENCE_CONSTANT) //Not quite correct; it should be the sqrt(squared sum of norms), but should be a fine approx.
				{
					break;
				}*/
				//Step
				w += (DenseVector)wgrad.Multiply(a);
				v += (DenseVector)vgrad.Multiply(a);
				
				if(!string.IsNullOrEmpty(parameter_storage_out_path))
				{
					parameter_storage_out = new FileStream(parameter_storage_out_path,FileMode.Create);
					serializer.Serialize(parameter_storage_out,w);
					serializer.Serialize(parameter_storage_out,v);
					/* serializer.Serialize(parameter_storage_out,ImageData.Ons_seen); */
					/* serializer.Serialize(parameter_storage_out,ImageData.Sites_seen); */
					parameter_storage_out.Close();
				}
				if(newlikelihood - oldlikelihood < LikelihoodConvergence)
				{
					Console.WriteLine("New likelihood - Old likelihood is {0}; Converged.",newlikelihood-oldlikelihood);
					break;
				}
				
				iter_count++;
			}
			return new Model(w,v,iter_count, 0, 10, Crosser, Transformer);
			
		}
		public double PseudoLikelihood(DenseVector wtest, DenseVector vtest, ImageData[] training_inputs, Classification[] training_outputs)
		{
			double first_term = 0;
			for(int m = 0; m < training_inputs.Length; m++)
			{
				for(int horz = 0; horz < training_inputs[m].XSites; horz++)
				{
					for(int vert = 0; vert < training_inputs[m].YSites; vert++)
					{
						int x = (int)(training_outputs[m][horz,vert])*2 - 1;
						
						//h_i(y):
						DenseVector h = Transformer.Transform(training_inputs[m][horz,vert]);
						first_term += MathWrapper.Log (MathWrapper.Sigma( x * wtest.DotProduct(h) ) );
						foreach(Tuple<int,int> j in training_inputs[m].GetNeighbors(horz,vert))
						{
							int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
							DenseVector mu;
							if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
							else mu = Crosser.Cross(training_inputs[m][j.Item1,j.Item2], training_inputs[m][horz,vert]);
							first_term += x * jx * vtest.DotProduct(mu);
						}
						double z = 0;
						//Sum over possible x_i
						for(int tempx = -1; tempx <= 1; tempx += 2)
						{
							double logofcoeff = MathWrapper.Log(MathWrapper.Sigma(tempx * wtest.DotProduct(h)));
							//sum over the neighbors
							foreach(Tuple<int,int> j in training_inputs[m].GetNeighbors(horz,vert))
							{
								int jx = (int)(training_outputs[m][j.Item1,j.Item2])*2 - 1;
								DenseVector mu;
								if(ImageData.IsEarlier(horz,vert,j.Item1,j.Item2))mu = Crosser.Cross(training_inputs[m][horz,vert],training_inputs[m][j.Item1,j.Item2]);
								else mu = Crosser.Cross(training_inputs[m][j.Item1,j.Item2], training_inputs[m][horz,vert]);
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
		public static Model Deserialize(string params_in)
		{ 
			string parameter_storage_in_path = string.Format("{0}../../../../Dataset/{1}",AppDomain.CurrentDomain.BaseDirectory,params_in);
			Stream parameter_storage_in = new FileStream(parameter_storage_in_path,FileMode.Open);
			SoapFormatter serializer = new SoapFormatter();
			DenseVector w = (DenseVector)serializer.Deserialize(parameter_storage_in);
			DenseVector v = (DenseVector)serializer.Deserialize(parameter_storage_in);
			/* int Ons_seen = (int)serializer.Deserialize(parameter_storage_in); */
			/* int Sites_seen = (int)serializer.Deserialize(parameter_storage_in); */
			/*CrossFeatureStrategy crosser = (CrossFeatureStrategy)serializer.Deserialize(parameter_storage_in);
			TransformFeatureStrategy transformer = (TransformFeatureStrategy)serializer.Deserialize(parameter_storage_in);*/
			
			Console.WriteLine("Successfully loaded model from {0}\n", parameter_storage_in_path); 
			return new Model(w, v, 0, 0, 10, ConcatenateFeatures.INSTANCE, LinearBasis.INSTANCE); //HACK TODO: This sucks.
		}
	}
}

