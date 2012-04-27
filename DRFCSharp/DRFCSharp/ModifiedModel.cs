using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ModifiedModel
	{
		DenseVector w;
		DenseVector v;
		
		public ModifiedModel (DenseVector w, DenseVector v)
		{
			this.w = w;
			this.v = v;
		}
		
		public Classification MaximumAPosterioriInfer(ImageData test_input)
		{
			throw new NotImplementedException();
		}
		
		public static Model PseudoLikelihoodTrain(ImageData[] training_inputs, Classification[] training_outputs)
		{
			throw new NotImplementedException();
		}
		
	}
}

