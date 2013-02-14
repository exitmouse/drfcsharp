using System;
using MathNet.Numerics.LinearAlgebra.Double;
namespace DRFCSharp
{
	/// <summary>
	/// Represents a strategy for building a cross-feature vector from two SiteFeatureSets
	/// </summary>
	public interface CrossFeatureStrategy
	{
		/// <summary>
		/// Somehow combine the specified SiteFeatureSets, returning a DenseVector with parameters that measure
		/// the similarity between the two sites represented by a and b.
		/// </summary>
		/// <param name='a'>
		/// A.
		/// </param>
		/// <param name='b'>
		/// B.
		/// </param>/
		DenseVector Cross(SiteFeatureSet a, SiteFeatureSet b);
		
		/// <summary>
		/// Returns the size of the DenseVector Cross would return if called on arguments of size num_features.
		/// </summary>
		/// <param name='num_features'>
		/// The number of features per site in the current context.
		/// </param>
		int Components(int num_features);
	}
}

