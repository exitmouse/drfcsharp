using System;
using MathNet.Numerics.LinearAlgebra.Double;
namespace DRFCSharp
{
	/// <summary>
	/// Represents the concatenation strategy for combining site features. In this implementation,
	/// we add a 1 to the front to accomodate a constant bias term.
	/// 
	/// The class is sealed to prevent derivation; this ensures it remains singleton.
	/// </summary>
	public sealed class ConcatenateFeatures : CrossFeatureStrategy
	{
		/// <summary>
		/// The private single instance of this class; since it is immutable and stateless we want it to be singleton.
		/// We initialize it here for thread-safety later.
		/// </summary>
		private static ConcatenateFeatures _instance = new ConcatenateFeatures();
		
		/// <summary>
		/// Gets the single instance. There is no setter.
		/// </summary>
		public static ConcatenateFeatures INSTANCE {
			get{
				return _instance;
			}
		}
		
		/* Constructor is private so it won't be instantiated */
		private ConcatenateFeatures()
		{
		}
		
		/// <summary>
		/// Combine the specified SiteFeatureSets, returning a DenseVector containing a's features
		/// and then b's in order. It also has a constant bias term.
		/// 
		/// Note: Not commutative.
		/// </summary>
		public DenseVector Cross(SiteFeatureSet a, SiteFeatureSet b){
			double[] aa = a.ToArray();
			double[] ba = b.ToArray();
			double[] z = new double[aa.Length + ba.Length + 1];
			z[0] = 1;
			aa.CopyTo(z,1);
			ba.CopyTo(z,aa.Length+1);
			return new DenseVector(z);
		}
		
		/// <summary>
		/// Returns the size of the DenseVector Cross would return if called on arguments of size num_features.
		/// In this case, since we concatenate and then add the constant term, the function is 2x+1.
		/// </summary>
		/// <param name='num_features'>
		///  The number of features per site in the current context. 
		/// </param>
		public int Components(int num_features){
			return 2*num_features + 1;
		}
		
		public override string ToString()
		{
			return string.Format("[Cross Feature Strategy: Concatenate Features]");
		}
	}
}

