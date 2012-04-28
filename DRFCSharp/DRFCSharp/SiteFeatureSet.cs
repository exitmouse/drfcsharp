using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class SiteFeatureSet
	{
		public DenseVector features;
		
		public SiteFeatureSet (DenseVector features)
		{
			this.features = features;
		}
		/// <summary>
		/// We have features for two sites; we want a feature vector that describes their
		/// cross-term for the interaction potential. This is the mu in Kumar & Hebert
		/// 2006 (p. 14).
		/// </summary>
		/// <returns>
		/// The new feature vector mu.
		/// </returns>
		/// <param name='a'>
		/// One of the sites.
		/// </param>
		/// <param name='b'>
		/// The other site.
		/// </param>
		/// <exception cref='NotImplementedException'>
		/// Is thrown when a requested operation is not implemented for a given type.
		/// </exception>
		public static DenseVector CrossFeatures(SiteFeatureSet a, SiteFeatureSet b)
		{
			throw new NotImplementedException();
			return new DenseVector(5);
		}
	}
}

