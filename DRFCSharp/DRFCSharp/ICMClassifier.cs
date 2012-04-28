using System;
using System.Threading;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ICMClassifier
	{
		Label[,] current_classification;
		DenseVector w;
		DenseVector v;
		double beta;
		double kappa;
		ImageData img;
		
		public ICMClassifier (DenseVector w, DenseVector v, double beta, double kappa, ImageData img)
		{
			this.w = w;
			this.v = v;
			this.beta = beta;
			this.kappa = kappa;
			this.img = img;
			
			//current_classification = new Label[ImageData.x_sites, ImageData.y_sites];
			//current_classification.Initialize();
		}
		public Classification Classify()
		{
			//Eventually:
			//Divide into coding sets (speedup is nice at runtime)
			//Let the threads run
			//Currently:
			for(int n = 0; n < 3; n++)
			{
				for(int i = 0; i < ImageData.x_sites; i++)
				{
					for(int j = 0; j < ImageData.y_sites; j++)
					{
						//Set current_classification[i,j] to the maximally probable choice given the current state of its neighbors.
						if(ConditionalProbability(img, i, j, Label.ON) > ConditionalProbability(img, i, j, Label.OFF))
							current_classification[i,j] = Label.ON;
						else
							current_classification[i,j] = Label.OFF;
					}
				}
			}
			return new Classification(current_classification);
		}
		public double ConditionalProbability(ImageData img, int i, int j, Label label)
		{
			double a_potential = AssociationPotential(img, i, j, label);
			double i_potential = SumOfInteractionPotentials(img, i, j, label);
			return a_potential + beta*i_potential;
		}
		public double AssociationPotential(ImageData img, int i, int j, Label label)
		{
			// See equation 6 of the DRF paper.
			throw new NotImplementedException();
		}
		public double SumOfInteractionPotentials(ImageData img, int i, int j, Label label)
		{
			// See equation 8 of the DRF paper.
			throw new NotImplementedException();
		}
	}
}

