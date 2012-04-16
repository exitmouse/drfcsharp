using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class Model
	{
		DenseVector w;
		DenseVector v;
		double beta;
		double kappa;
		
		public Model (DenseVector w, DenseVector v, double beta, double kappa)
		{
			this.w = w;
			this.v = v;
			this.beta = beta;
			this.kappa = kappa;
		}
		
		public Classification MaximumLikelihoodInfer(ImageData test_input)
		{
			throw new NotImplementedException();
		}
		
		public static Model PseudoLikelihoodTrain(ImageData[] training_inputs, Classification[] training_outputs)
		{
			throw new NotImplementedException();
		}
		
	}
}

