using System;
using MathNet.Numerics.LinearAlgebra.Double;
namespace DRFCSharp
{
	/// <summary>
	/// Represents a set of basis functions that map a SiteFeatureSet into a vector suitable for learning with.
	/// Instead of representing each function separately, we just have one function, Transform, that maps
	/// SiteFeatureSets into DenseVectors.
	/// </summary>
	public interface TransformFeatureStrategy
	{
		/// <summary>
		/// Somehow transform the specified SiteFeatureSet, returning a DenseVector with parameters more suitable
		/// for learning on.
		/// </summary>
		/// <param name='a'>
		/// A SiteFeatureSet to be transformed.
		/// </param>
		DenseVector Transform(SiteFeatureSet a);
		
		/// <summary>
		/// Returns the size of the DenseVector Transform would return if called on an argument of size num_features.
		/// </summary>
		/// <param name='num_features'>
		/// The number of features per site in the current context.
		/// </param>
		int Components(int num_features);
	}
}

