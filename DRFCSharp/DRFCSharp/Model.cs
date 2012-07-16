using System;
using System.IO;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class Model
	{
		public DenseVector W { get; private set; }	
		public DenseVector V { get; private set; }
		public int TimeToConverge { get; private set; }
		public int OnsSeen { get; private set; }
		public int SitesSeen { get; private set; }
		public CrossFeatureStrategy Crosser { get; private set; }
		public TransformFeatureStrategy Transformer { get; private set; }
		
		public Model (DenseVector w, DenseVector v, int iter_count, int ons_seen, int sites_seen, CrossFeatureStrategy crosser, TransformFeatureStrategy transformer)
		{
			this.W = w;
			this.V = v;
			this.TimeToConverge = iter_count;
			this.OnsSeen = ons_seen;
			this.SitesSeen = sites_seen;
			this.Crosser = crosser;
			this.Transformer = transformer;
		}
		public Classification LogisticInfer(ImageData test_input)
		{
			Classification curr_classification = new Classification(new Label[test_input.XSites, test_input.YSites]);
			for(int x = 0; x < test_input.XSites; x++) for(int y = 0; y < test_input.YSites; y++)
			{
				double modeled_prob_of_one = MathWrapper.Sigma(W.DotProduct(Transformer.Transform(test_input[x,y])));
				double prob_one = ((double)OnsSeen)/((double) SitesSeen);
				//double prob_zero = 1d - prob_one;
				double lambda = MathWrapper.Log(modeled_prob_of_one) - MathWrapper.Log (1 - modeled_prob_of_one)/* + MathWrapper.Log (prob_one/prob_zero)*/;
				if(lambda > 0)
					curr_classification[x,y] = Label.ON;
				else
					curr_classification[x,y] = Label.OFF;
			}
			return curr_classification;
		}
		public Classification ICMInfer(ImageData test_input)
		{
			Classification curr_classification = LogisticInfer(test_input); //Muahaha better initialization. I don't think this is hacky.
			//for(int x = 0; x < ImageData.x_sites; x++) for(int y = 0; y < ImageData.y_sites; y++) curr_classification[x,y] = Label.OFF;
			bool converged = false;
			int loopcount = 0;
			while(!converged && loopcount < 300)
			{
				loopcount++;
				int changecount = 0;
				converged = true;
				for(int x = 0; x < test_input.XSites; x++) for(int y = 0; y < test_input.YSites; y++)
				{
					Label old = curr_classification[x,y];
					//We have prob 1 vs prob 0. Prob n \propto exp(A + sum over neighbors of I calculated at n)
					//So we can just calculate A + sum over neighbors of I for each labeling of the site, and
					//assign to the site whichever is higher.
					var sitefeatures = Transformer.Transform(test_input[x,y]);
					if(x == 6 && y == 10)
					{
						Console.WriteLine("Components of the dot product w*sitefeatures:");
						for(int i = 0; i < sitefeatures.Count; i++)
						{
							Console.WriteLine("{0}th component: {1}", i, W[i]*sitefeatures[i]);
						}
							                  
					}
					double on_association = MathWrapper.Log(MathWrapper.Sigma(W.DotProduct(sitefeatures)));
					double off_association = MathWrapper.Log(MathWrapper.Sigma(-1 * W.DotProduct(sitefeatures)));
					double on_interaction = 0d;
					double off_interaction = 0d;
					

					
					foreach(Tuple<int,int> t in test_input.GetNeighbors(x,y))
					{
						DenseVector mu;
						if(ImageData.IsEarlier(x,y,t.Item1,t.Item2))mu = Crosser.Cross(test_input[x,y],test_input[t.Item1,t.Item2]);
						else mu = Crosser.Cross(test_input[t.Item1,t.Item2], test_input[x,y]);
						//Console.WriteLine("Magnitude of Interaction: {0}",v.DotProduct(mu));
						if(curr_classification[t.Item1,t.Item2] == Label.ON)
						{
							on_interaction += V.DotProduct(mu);
							off_interaction -= V.DotProduct(mu);
						}
						else
						{
							on_interaction -= V.DotProduct(mu);
							off_interaction += V.DotProduct(mu);
						}
					}
					
					if(on_association + on_interaction > off_association + off_interaction)
					{
						/*Console.WriteLine("On Association: {0}",on_association);
						Console.WriteLine("Off Association: {0}",off_association);*/
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
			return curr_classification;
		}
		public Classification MaximumAPosterioriInfer(ImageData test_input)
		{
			Vertex[,] site_nodes = new Vertex[test_input.XSites, test_input.YSites];
			for(int i = 0; i < test_input.XSites; i++) for(int j = 0; j < test_input.YSites; j++)
			{
				site_nodes[i,j] = new Vertex();
			}
			Vertex source = new Vertex();
			Vertex target = new Vertex();
			
			for(int j = 0; j < test_input.YSites; j++)
			{
				for(int i = 0; i < test_input.XSites; i++) 
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
					double modeled_prob_of_one = MathWrapper.Sigma(W.DotProduct(Transformer.Transform(test_input[i,j])));
					/*double prob_one = ((double)Ons_seen)/((double) Sites_seen);
					double prob_zero = 1d - prob_one;
					double lambda = MathWrapper.Log(modeled_prob_of_one) - MathWrapper.Log (1 - modeled_prob_of_one) + MathWrapper.Log (prob_one/prob_zero);*/
					Edge.AddEdge(source,t,-MathWrapper.Log(modeled_prob_of_one),0);
					Edge.AddEdge(t,target,-MathWrapper.Log(1-modeled_prob_of_one),0);
					Console.WriteLine("Edge to target with strength {0}",-MathWrapper.Log(1-modeled_prob_of_one));
					//Add an edge from the source with the modeled probability of 1, and an edge to the target with the modeled probability of 0.
					//Console.WriteLine(ImageData.GetNewConnections(i,j).Count);
					foreach(Tuple<int,int> other in test_input.GetNewConnections(i,j))
					{
						Vertex u = site_nodes[other.Item1,other.Item2];
						//Add the edge with capacity Beta_{t,u} in both directions between t and u.
						//DRFS (2006) says that the data dependent smoothing term is max(0,v^T * mu_{i,j}y)
						DenseVector mu;
						if(ImageData.IsEarlier(i,j,other.Item1,other.Item2))mu = Crosser.Cross(test_input[i,j],test_input[other.Item1,other.Item2]);
						else mu = Crosser.Cross(test_input[other.Item1,other.Item2], test_input[i,j]);
						double capacity = Math.Max(0,V.DotProduct(mu));
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
			
			Label[,] toReturn = new Label[test_input.XSites, test_input.YSites];
			for(int i = 0; i < test_input.XSites; i++) for(int j = 0; j < test_input.YSites; j++)
			{
				if(site_nodes[i,j].tagged_as_one) toReturn[i,j] = Label.ON;
			}
			return new Classification(toReturn);
		}
	}
}

