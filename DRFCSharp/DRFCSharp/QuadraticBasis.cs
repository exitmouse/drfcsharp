using System;
using MathNet.Numerics.LinearAlgebra.Double;
namespace DRFCSharp
{
	/// <summary>
	/// Represents a set of quadratic basis functions to apply to features. In this implementation,
	/// we add a 1 to the front to accomodate a constant bias term, and we also include all linear
	/// terms, making the components of QuadraticBasis a superset of the LinearBasis.
	/// 
	/// The class is sealed to prevent derivation; this ensures it remains singleton.
	/// </summary>
	public sealed class QuadraticBasis : TransformFeatureStrategy
	{
		/// <summary>
		/// The private single instance of this class; since it is immutable and stateless we want it to be singleton.
		/// We initialize it here for thread-safety later.
		/// </summary>
		private static QuadraticBasis _instance = new QuadraticBasis();
		
		/// <summary>
		/// Gets the single instance. There is no setter.
		/// </summary>
		public static QuadraticBasis INSTANCE {
			get{
				return _instance;
			}
		}
		
		/* Constructor is private so it won't be instantiated */
		private QuadraticBasis()
		{
		}
		
		/// <summary>
		/// Returns a DenseVector, the components of which are each pairwise product of the components of a.
		/// Note that we first add a 1 to the front of a, so that these pairwise products include
		/// 1*1, 1*a[0], 1*a[1], etc.
		/// This ensures that the QuadraticBasis is a superset of the LinearBasis.
		/// </summary>
		/// <param name='a'>
		///  A SiteFeatureSet to be transformed. 
		/// </param>
		public DenseVector Transform(SiteFeatureSet a){
			//DenseVector af = a.features;
			double[] aa = LinearBasis.INSTANCE.Transform(a).ToArray();
			double[] z = new double[(aa.Length*(aa.Length+1))/2];
			int count = 0;
			for(int i = 0; i < aa.Length; i++)
			{
				for(int j = i; j < aa.Length; j++)
				{
					z[count] = aa[i]*aa[j];
					count++;
				}
			}
			return new DenseVector(z);
		}
		
		/// <summary>
		/// Returns the size of the DenseVector Transform would return if called on an argument of size num_features.
		/// This is (num_features + 1) choose 2.
		/// </summary>
		/// <param name='num_features'>
		///  The number of features per site in the current context. 
		/// </param>
		public int Components(int num_features){
			/* We multiply each pair of components together exactly once, but first we add a 1 to the front.
			 * Therefore, this is (num_features + 1) choose 2, or the following expression:*/
			return ((num_features+1) * (num_features+2))/2;
		}
		
		public override string ToString()
		{
			return string.Format("[Transform Feature Strategy: Quadratic Basis Functions]");
		}
	}
}