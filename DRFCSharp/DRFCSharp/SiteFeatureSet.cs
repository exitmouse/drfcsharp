using System;
using System.Collections.Generic;
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
		public override bool Equals (object obj)
		{
			if(obj is SiteFeatureSet)
			{
				return features.Equals((obj as SiteFeatureSet).features);
			}
			else return false;
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
			DenseVector af = a.features;
			DenseVector bf = b.features;
			double[] aa = af.ToArray();
			double[] ba = bf.ToArray();
			double[] z = new double[aa.Length + ba.Length + 1];
			z[0] = 1;
			aa.CopyTo(z,1);
			ba.CopyTo(z,aa.Length+1);
			return new DenseVector(z);
		}
	}
}

