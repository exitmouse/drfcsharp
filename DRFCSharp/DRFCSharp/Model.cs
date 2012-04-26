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
			ICMClassifier icm = new ICMClassifier(w,v,beta,kappa,test_input);
			return icm.Classify();
		}
		
		public static Model PseudoLikelihoodTrain(ImageData[] training_inputs, Classification[] training_outputs)
		{
			throw new NotImplementedException();
			/* We have \vec{\theta}^{ML} approximated as
			* \argmax_\theta \prod_{m=1}^M \prod_{i \in S} P(x^m_i | \vec{x}^m_{\mathcal{N}_i}, \vec{y}^m, \vec{\theta}).
			* To maximize this, we'll take the log:
			* \argmax_\theta \ell(\theta) = \sum_{m=1}^M \sum_{i \in S} -log(z_i) + A(x_i, \vec{y}) + \sum_{j \in \mathcal{N}_i} I(x_i,x_j,\vec{y})
			* And then we take the gradient, which is basically four different derivatives:
			* 
			* \frac{\partial \ell}{\partial \vec{w}_a} for a bunch of a,
			* \frac{\partial \ell}{\partial \vec{v}_a} for a bunch of a,
			* \frac{d \ell}{d \beta},
			* \frac{d \ell}{d \kappa}.
			* 
			* 
		}
		
	}
}

