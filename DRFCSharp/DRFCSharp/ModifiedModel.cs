using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ModifiedModel
	{
		DenseVector w;
		DenseVector v;
		public const int MAX_ITERS = 3000;
		public const double CONVERGENCE_CONSTANT = 1;
		public const double START_STEP_LENGTH = 0.0000000001d;
		public const double MIN_STEP_LENGTH = 0.0000000000001d; //TODO all these small thingies are hacks
		public const double LIKELIHOOD_CONVERGENCE = 3d;
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
			Vertex[,] site_nodes = new Vertex[ImageData.x_sites,ImageData.y_sites];
			for(int i = 0; i < ImageData.x_sites; i++) for(int j = 0; j < ImageData.y_sites; j++)
			{
				site_nodes[i,j] = new Vertex();
			}
			Vertex source = new Vertex();
			Vertex target = new Vertex();
			StreamWriter sw = new StreamWriter("C:/Users/Jesse/Documents/DiscriminativeRandomFields/Discriminative-Random-Fields/Dataset/231.txt");
			for(int j = 0; j < ImageData.y_sites; j++)
			{
				for(int i = 0; i < ImageData.x_sites; i++) 
				{
					Vertex t = site_nodes[i,j];
					//Add the edge with capacity lambda_t from the source, or the edge with capacity -lambda_t to the target.
					//Lambda_t is the log-likelihood ratio: log( p(y | x = 1) / p(y | x = 0) ).
					//Using Bayes' law, we have
					//Posterior Odds = P(x = 1 | y)/P(x = 0 | y) = Likelihood Ratio * Prior Odds = (P(y | x = 1) / P(y | x = 0))*(P(x=1)/P(x=0)) = e^(lambda_t)*1
					//So lambda_t should be log(Posterior Odds) = log(P(x=1|y))-log(P(x=0|y))
					
					//Now, P(x=1|y) is modeled as sigma(w^T * h(y)), so this should be
					//log(sigma(w^T * h(y))) - log(1-sigma(w^T * h(y))).
					//However, all these calculations were done at roughly 5:50 AM and I hadn't slept yet, so...
					//I could totally be wrong.
					//-Jesse Selover
					double modeled_prob_of_one = Sigma(w.DotProduct(SiteFeatureSet.TransformedFeatureVector(test_input[i,j])));
					double lambda = Log(modeled_prob_of_one) - Log (1 - modeled_prob_of_one);
					if(lambda > 0)
					{
						Console.WriteLine ("Edge to source with strength {0}",lambda);
						Edge.AddEdge(source,t,lambda,0);
						sw.Write('1');
					}
					else
					{
						Console.WriteLine ("Edge to target with strength {0}",-lambda);
						Edge.AddEdge(t,target,-lambda,0);
						sw.Write('0');
					}
					sw.Write(',');
					
					foreach(Tuple<int,int> other in ImageData.GetNewConnections(i,j))
					{
						Vertex u = site_nodes[other.Item1,other.Item2];
						//Add the edge with capacity Beta_{t,u} in both directions between t and u.
						//DRFS (2006) says that the data dependent smoothing term is max(0,v^T * mu_{i,j}y
						double capacity = Math.Max(0,v.DotProduct(SiteFeatureSet.CrossFeatures(test_input[i,j],test_input[other.Item1,other.Item2])));
						Edge.AddEdge(t,u,capacity,capacity);
					}
				}
				sw.Write('\n');
			}
			sw.Close();
			double flow_added = 0;
			while(true)
			{
				flow_added = source.AddFlowTo(new List<Vertex>(), target, 400000000d);
				if(flow_added <= 0.0000001d) break;
			}; //Find the maximum flow
			source.ResidualCapacityConnectedNodes(); //Find the source end of the minimum cut
			
			Label[,] toReturn = new Label[ImageData.x_sites,ImageData.y_sites];
			for(int i = 0; i < ImageData.x_sites; i++) for(int j = 0; j < ImageData.y_sites; j++)
			{
				if(site_nodes[i,j].tagged_as_one) toReturn[i,j] = Label.ON;
			}
			return new Classification(toReturn);
		}
		
		public static ModifiedModel PseudoLikelihoodTrain(ImageData[] training_inputs, Classification[] training_outputs, double tau)
		{
			if(tau <= 0) throw new ArgumentException("Tau must be positive");
			if(training_inputs.Length != training_outputs.Length) throw new ArgumentException("Different number of training inputs and outputs");
			
			DenseVector w = new DenseVector(SiteFeatureSet.NUM_FEATURES + 1, 0d);
			DenseVector v = new DenseVector(SiteFeatureSet.NUM_FEATURES*2 + 1, 0d);
			
			DenseVector wgrad = new DenseVector(w.Count);
			DenseVector vgrad = new DenseVector(v.Count);
			
			Stream thetaverboselog = new FileStream("C:/Users/Jesse/Documents/DiscriminativeRandomFields/Discriminative-Random-Fields/thetalog80.xml",FileMode.OpenOrCreate);
			SoapFormatter serializer = new SoapFormatter();
			
			w = (DenseVector)serializer.Deserialize(thetaverboselog);
			v = (DenseVector)serializer.Deserialize(thetaverboselog);
			
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
								double hk = h[k];
								if(double.IsNaN(hk)) throw new NotFiniteNumberException();
								//sigma term that keeps reappearing
								double sig = Sigma(x * w.DotProduct(h));
								//x_i * h_i(y)_k * (1 - sigma(x_i * w^T h_i(y)))
								double old = wgrad[k];
								wgrad[k] += x*h[k]*(1 - sig);
								if(double.IsNaN(wgrad[k])) 
								{
									throw new NotFiniteNumberException();
								}
								
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
									double multfactor = (1 - Sigma (tempx * w.DotProduct(h)));
									dzdw += coeff * tempx*h[k]*multfactor;
									if(double.IsNaN(dzdw)||double.IsNaN(z)||double.IsInfinity(dzdw)||double.IsInfinity(z)) throw new NotFiniteNumberException();
								}
								if(z <= 0d)
								{
									throw new NotFiniteNumberException();
								}
								wgrad[k] -= dzdw/z;
								if(double.IsNaN(wgrad[k])) throw new NotFiniteNumberException();
								if(Math.Abs(wgrad[k]) > 100000000000d) throw new NotFiniteNumberException();
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
				double normwgrad = wgrad.Norm(2d);
				double normvgrad = vgrad.Norm(2d);
				double sumofnorms = normwgrad + normvgrad;
				Console.WriteLine("\t\t\t\t\tL2 Norms Summed: {0}",sumofnorms);
				//Check for convergence
				if(sumofnorms < CONVERGENCE_CONSTANT)
				{
					break;
				}
				
				//Compute best step length
				double a = START_STEP_LENGTH;
				double oldlikelihood = PseudoLikelihood(w,v, training_inputs, training_outputs, tau);
				double newlikelihood = 0;
				while(true)
				{
					newlikelihood = PseudoLikelihood(w + (DenseVector)wgrad.Multiply(a), v + (DenseVector)vgrad.Multiply(a), training_inputs, training_outputs, tau);
					if(newlikelihood > oldlikelihood)
					{
						Console.WriteLine ("Likelihood after this step: {0}",newlikelihood);
						break;
					}
					a = a/2;
					if(a < MIN_STEP_LENGTH)
					{
						break;
					}
				}
				Console.WriteLine ("Step length: {0}",a);
				if( a < MIN_STEP_LENGTH)
				{
					break;
				}
				//Step
				w += (DenseVector)wgrad.Multiply(a);
				v += (DenseVector)vgrad.Multiply(a);
				
				serializer.Serialize(thetaverboselog,w);
				serializer.Serialize(thetaverboselog,v);
				thetaverboselog.Flush();
				if(newlikelihood - oldlikelihood < LIKELIHOOD_CONVERGENCE)
				{
					break;
				}
				
				iter_count++;
			}
			
			thetaverboselog.Close();
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
			double result = 1/(1+Exp(-x));
			if(result < 0 || result > 1)
			{
				throw new NotFiniteNumberException("Sigma should be between 0 and 1");
			}
			if(result < EPSILON) return EPSILON;
			return result; 
		}
	}
}

