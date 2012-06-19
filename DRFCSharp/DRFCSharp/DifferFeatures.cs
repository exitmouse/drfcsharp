using System;
using MathNet.Numerics.LinearAlgebra.Double;
namespace DRFCSharp
{
	/// <summary>
	/// Represents the absolute difference strategy for combining site features. In this implementation,
	/// we add a 1 to the front to accomodate a constant bias term.
	/// This strategy takes two vectors, subtracts them component-wise, and returns the component-wise
	/// absolute value, and aspires to be a better measure of distance between sites than concatenation.
	/// 
	/// The class is sealed to prevent derivation; this ensures it remains singleton.
	/// </summary>
	public sealed class DifferFeatures : CrossFeatureStrategy
	{
		/// <summary>
		/// The private single instance of this class; since it is immutable and stateless we want it to be singleton.
		/// We initialize it here for thread-safety later.
		/// </summary>
		private static DifferFeatures _instance = new DifferFeatures();
		
		/// <summary>
		/// Gets the single instance. There is no setter.
		/// </summary>
		public static DifferFeatures INSTANCE {
			get{
				return _instance;
			}
		}
		
		/* Constructor is private so it won't be instantiated */
		private DifferFeatures()
		{
		}
		
		/// <summary>
		/// Combine the specified SiteFeatureSets, returning a DenseVector containing the component-wise absolute
		/// difference of a and b. It also has a constant bias term.
		/// 
		/// Note: Commutative.
		/// </summary>
		public DenseVector Cross(SiteFeatureSet a, SiteFeatureSet b){
			double[] aa = a.ToArray();
			double[] ba = b.ToArray();
			double[] z = new double[aa.Length + 1];
			z[0] = 1;
			for(int i = 0; i < aa.Length; i++)
			{
				z[i+1] = Math.Abs(aa[i] - ba[i]);
			}
			return new DenseVector(z);
		}
		
		/// <summary>
		/// Returns the size of the DenseVector Cross would return if called on arguments of size num_features.
		/// In this case, the only change to the size of the vector is adding a 1 to the front, so this returns
		/// num_features + 1.
		/// </summary>
		/// <param name='num_features'>
		///  The number of features per site in the current context. 
		/// </param>
		public int Components(int num_features){
			return num_features + 1;
		}
		
		public override string ToString()
		{
			return string.Format("[Cross Feature Strategy: Differ Features]");
		}
	}
}

