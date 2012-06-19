using System;
using MathNet.Numerics.LinearAlgebra.Double;
namespace DRFCSharp
{
	/// <summary>
	/// Represents a set of linear basis functions to apply to features. Those are fancy words
	/// that mean that this class takes a vector and puts a 1 on the front.
	/// 
	/// The class is sealed to prevent derivation; this ensures it remains singleton.
	/// </summary>
	public sealed class LinearBasis : TransformFeatureStrategy
	{
		/// <summary>
		/// The private single instance of this class; since it is immutable and stateless we want it to be singleton.
		/// We initialize it here for thread-safety later.
		/// </summary>
		private static LinearBasis _instance = new LinearBasis();
		
		/// <summary>
		/// Gets the single instance. There is no setter.
		/// </summary>
		public static LinearBasis INSTANCE {
			get{
				return _instance;
			}
		}
		
		/* Constructor is private so it won't be instantiated */
		private LinearBasis()
		{
		}
		
		/// <summary>
		/// Adds 1 to the front of a's features.
		/// </summary>
		/// <param name='a'>
		///  A SiteFeatureSet to be transformed. 
		/// </param>
		public DenseVector Transform(SiteFeatureSet a){
			double[] aa = a.ToArray();
			double[] z = new double[aa.Length + 1];
			z[0] = 1;
			aa.CopyTo(z,1);
			return new DenseVector(z);
		}
		
		/// <summary>
		/// Returns the size of the DenseVector Transform would return if called on an argument of size num_features.
		/// Clearly, this is num_features + 1.
		/// </summary>
		/// <param name='num_features'>
		///  The number of features per site in the current context. 
		/// </param>
		public int Components(int num_features){
			return num_features+1;
		}
		
		public override string ToString()
		{
			return string.Format("[Transform Feature Strategy: Linear Basis Functions]");
		}
	}
}

