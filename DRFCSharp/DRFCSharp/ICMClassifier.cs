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
			
			current_classification = new Label[ImageData.x_sites, ImageData.y_sites]();
			current_classification.Initialize();
		}
		public Classification Classify()
		{
			//Divide into coding sets
			//Let the threads run
		}
	}
}

