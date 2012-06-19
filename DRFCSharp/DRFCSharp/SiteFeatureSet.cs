using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class SiteFeatureSet
	{
		public DenseVector features;
		public const int NUM_FEATURES = 11;
		public SiteFeatureSet ()
		{
			this.features = new DenseVector(NUM_FEATURES,0d);
		}

		public SiteFeatureSet (DenseVector features)
		{
			if(features.Count != NUM_FEATURES) throw new ArgumentException("SiteFeatureSets have "+NUM_FEATURES.ToString()+" features, not "+features.Count.ToString()+".");
			this.features = features;
		}
		
		public double[] ToArray(){
			return features.ToArray();
		}

		public override bool Equals (object obj)
		{
			if(obj is SiteFeatureSet)
			{
				return features.Equals((obj as SiteFeatureSet).features);
			}
			else return false;
		}
		public static void Init(SiteFeatureSet[,] sitesarray)
		{
			for(int i = 0; i < sitesarray.GetLength(0); i++)
			{
				for(int j = 0; j < sitesarray.GetLength(1); j++)
				{
					sitesarray[i,j] = new SiteFeatureSet();
				}
			}
		}
	}
}

