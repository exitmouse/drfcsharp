using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ModifiedModel
	{
		DenseVector w;
		DenseVector v;
		public const int MAX_ITERS = 30000;
		public const double CONVERGENCE_CONSTANT = 0.0001;
		
		public ModifiedModel (DenseVector w, DenseVector v)
		{
			this.w = w;
			this.v = v;
		}
		
		public Classification MaximumAPosterioriInfer(ImageData test_input)
		{
			throw new NotImplementedException();
		}
		
		public static ModifiedModel PseudoLikelihoodTrain(ImageData[] training_inputs, Classification[] training_outputs, double tau)
		{
			if(training_inputs.Length != training_outputs.Length) return null;
			
			DenseVector w = new DenseVector(5); //TODO replace
			DenseVector v = new DenseVector(5); //TODO NOT FIVE.
			
			DenseVector wgrad = new DenseVector(5);
			DenseVector vgrad = new DenseVector(5);
			
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
									dzdw += coeff * tempx*h[k]/Sigma(tempx * w.DotProduct(h));
									dzdv += coeff * dzdvterm;
								}
								wgrad[k] -= dzdw/z;
								vgrad[k] -= dzdv/z;
							}
						}
					}
					vgrad[k] -= v[k]/(Math.Pow (tau,2));
				}
				//Check for convergence
				if(wgrad.Norm(1d)+vgrad.Norm(1d) < CONVERGENCE_CONSTANT)
				{
					break;
				}
				
				//Compute best step length
				double a = 0.0001;
				
				//Step
				w += (DenseVector)wgrad.Multiply(a);
				v += (DenseVector)vgrad.Multiply(a);
				iter_count++;
			}
			return new ModifiedModel(w,v);
			
		}
		public static double Exp(double x){ return Math.Exp(x); }
		public static double Log(double x){ return Math.Log(x); }
		public static double Sigma(double x){ return 1/(1+ Exp (x)); }
		
	}
}

