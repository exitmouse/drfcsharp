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
		public const double CONVERGENCE_CONSTANT = 0.000000001;
		public const double START_STEP_LENGTH = 0.0000001d;//TODO all these small thingies are hacks
		public const double LIKELIHOOD_CONVERGENCE = 0.1d;
		public const double EPSILON = 0.000000001d;
		public readonly int time_to_converge;
		
		public int Ons_seen = 0;
		public int Sites_seen = 0;
		
		public ModifiedModel (DenseVector w, DenseVector v, int iter_count, int Ons_seen, int Sites_seen)
		{
			this.w = w;
			this.v = v;
			this.time_to_converge = iter_count;
			this.Ons_seen = Ons_seen;
			this.Sites_seen = Sites_seen;
		}
		public Classification LogisticInfer(ImageData test_input)
		{
			Label[,] curr_classification = new Label[ImageData.x_sites, ImageData.y_sites];
			for(int x = 0; x < ImageData.x_sites; x++) for(int y = 0; y < ImageData.y_sites; y++)
			{
				var f = test_input[x,y];
				if(w.DotProduct(SiteFeatureSet.TransformedFeatureVector(f)) > 0)
					curr_classification[x,y] = Label.ON;
			}
			return new Classification(curr_classification);
		}
		public Classification ICMInfer(ImageData test_input)
		{
			Label[,] curr_classification = new Label[ImageData.x_sites,ImageData.y_sites];
			for(int x = 0; x < ImageData.x_sites; x++) for(int y = 0; y < ImageData.y_sites; y++) curr_classification[x,y] = Label.OFF;
			bool converged = false;
			while(!converged)
			{
				int changecount = 0;
				converged = true;
				for(int x = 0; x < ImageData.x_sites; x++) for(int y = 0; y < ImageData.y_sites; y++)
				{
					Label old = curr_classification[x,y];
					//We have prob 1 vs prob 0. Prob n \propto exp(A + sum over neighbors of I calculated at n)
					//So we can just calculate A + sum over neighbors of I for each labeling of the site, and
					//assign to the site whichever is higher.
					var sitefeatures = SiteFeatureSet.TransformedFeatureVector(test_input[x,y]);
					double on_association = Sigma(w.DotProduct(sitefeatures));
					double off_association = Sigma(-1 * w.DotProduct(sitefeatures));
					double on_interaction = 0d;
					double off_interaction = 0d;
					
					foreach(Tuple<int,int> t in ImageData.GetNeighbors(x,y))
					{
						var mu = SiteFeatureSet.CrossFeatures(test_input[x,y],test_input[t.Item1,t.Item2]);
						if(curr_classification[t.Item1,t.Item2] == Label.ON)
						{
							on_interaction += v.DotProduct(mu);
							off_interaction -= v.DotProduct(mu);
						}
						else
						{
							on_interaction -= v.DotProduct(mu);
							off_interaction += v.DotProduct(mu);
						}
					}
					
					if(on_association + on_interaction > off_association + off_interaction)
					{
						curr_classification[x,y] = Label.ON;
					}
					else
					{
						curr_classification[x,y] = Label.OFF;
					}
					if(curr_classification[x,y] != old)
					{
						converged = false;
						changecount += 1;
					}
				}
				Console.WriteLine("Number of changes in this round of ICM: {0}",changecount);
			}
			return new Classification(curr_classification);
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
			
			for(int j = 0; j < ImageData.y_sites; j++)
			{
				for(int i = 0; i < ImageData.x_sites; i++) 
				{
					Vertex t = site_nodes[i,j];
					//Add the edge with capacity lambda_t from the source, or the edge with capacity -lambda_t to the target.
					//Lambda_t is the log-likelihood ratio: log( p(y | x = 1) / p(y | x = 0) ).
					//Using Bayes' law, we have
					//Posterior Odds = P(x = 1 | y)/P(x = 0 | y) = Likelihood Ratio * Prior Odds = (P(y | x = 1) / P(y | x = 0))*(P(x=1)/P(x=0)) = e^(lambda_t)*1
					//So lambda_t should be log(Posterior Odds) + log(Prior Odds) = log(P(x=1|y))-log(P(x=0|y)) + possibly 0?
					
					//Now, P(x=1|y) is modeled as sigma(w^T * h(y)), so this should be
					//log(sigma(w^T * h(y))) - log(1-sigma(w^T * h(y))).
					//However, all these calculations were done at roughly 5:50 AM and I hadn't slept yet, so...
					//I could totally be wrong.
					//-Jesse Selover
					double modeled_prob_of_one = Sigma(w.DotProduct(SiteFeatureSet.TransformedFeatureVector(test_input[i,j])));
					double prob_one = ((double)Ons_seen)/((double) Sites_seen);
					double prob_zero = 1d - prob_one;
					double lambda = Log(modeled_prob_of_one) - Log (1 - modeled_prob_of_one) + Log (prob_one/prob_zero);
					if(lambda > 0)
					{
						Console.WriteLine ("@source with strength {0}",lambda);
						Edge.AddEdge(source,t,lambda,0);
					}
					else
					{
						Console.WriteLine ("@target with strength {0}",-lambda);
						Edge.AddEdge(t,target,-lambda,0);
					}
					
					foreach(Tuple<int,int> other in ImageData.GetNewConnections(i,j))
					{
						Vertex u = site_nodes[other.Item1,other.Item2];
						//Add the edge with capacity Beta_{t,u} in both directions between t and u.
						//DRFS (2006) says that the data dependent smoothing term is max(0,v^T * mu_{i,j}y)
						double capacity = Math.Max(0,v.DotProduct(SiteFeatureSet.CrossFeatures(test_input[i,j],test_input[other.Item1,other.Item2])));
						Console.WriteLine ("\tInternode edge with strength {0}",capacity);
						Edge.AddEdge(t,u,capacity,capacity);
					}
				}
			}
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
		
		public static ModifiedModel Deserialize(string params_in)
		{
			string parameter_storage_in_path = string.Format("{0}../../.../../Dataset/{1}",AppDomain.CurrentDomain.BaseDirectory,params_in);
			Stream parameter_storage_in = new FileStream(parameter_storage_in_path,FileMode.Open);
			SoapFormatter serializer = new SoapFormatter();
			DenseVector w = (DenseVector)serializer.Deserialize(parameter_storage_in);
			DenseVector v = (DenseVector)serializer.Deserialize(parameter_storage_in);
			int Ons_seen = (int)serializer.Deserialize(parameter_storage_in);
			int Sites_seen = (int)serializer.Deserialize(parameter_storage_in);
			return new ModifiedModel(w, v, 0, Ons_seen, Sites_seen);
		}
		
		public static ModifiedModel PseudoLikelihoodTrain(string params_in, string params_out, ImageData[] training_inputs, Classification[] training_outputs, double tau)
		{
			if(tau <= 0) throw new ArgumentException("Tau must be positive");
			if(training_inputs.Length != training_outputs.Length) throw new ArgumentException("Different number of training inputs and outputs");
			
			DenseVector w = new DenseVector(SiteFeatureSet.NUM_FEATURES + 1, 0d);
			DenseVector v = new DenseVector(SiteFeatureSet.NUM_FEATURES + 1, 0d);
			
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
			while(iter_count < MAX_ITERS)
			{
				wgrad.Clear();
				vgrad.Clear();
				
				//Compute gradients
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
								
								if(double.IsNaN(wgrad[k])) throw new NotFiniteNumberException();
								
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
								
								if(z <= 0d) throw new NotFiniteNumberException();
								
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
								//h_i(y):
								DenseVector h = SiteFeatureSet.TransformedFeatureVector(training_inputs[m][horz,vert]);
								
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
									if(double.IsNaN(dzdv)||double.IsNaN(z)||double.IsInfinity(dzdv)||double.IsInfinity(z)) throw new NotFiniteNumberException();
								}
								
								if(z <= 0d) throw new NotFiniteNumberException();
								
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
					if(a*sumofnorms < CONVERGENCE_CONSTANT)
					{
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
					serializer.Serialize(parameter_storage_out,ImageData.Ons_seen);
					serializer.Serialize(parameter_storage_out,ImageData.Sites_seen);
					parameter_storage_out.Close();
				}
				if(newlikelihood - oldlikelihood < LIKELIHOOD_CONVERGENCE)//Hack, remove the abs
				{
					Console.WriteLine("New likelihood - Old likelihood is {0}; Converged.",newlikelihood-oldlikelihood);
					break;
				}
				
				iter_count++;
			}
			return new ModifiedModel(w,v,iter_count, ImageData.Ons_seen, ImageData.Sites_seen);
			
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

