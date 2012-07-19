using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
    //Immutable
	public class SiteFeatureSet
	{
		public DenseVector Features{ get; private set; }
        private FeatureSet _feature_set;
        public SiteFeatureSet (double[] features, FeatureSet feature_set)
        {
            Features = features;
            _feature_set = feature_set;
            //TODO: Assert _feature_set.Length == Features.Count
        }
		public SiteFeatureSet (List<double> features, FeatureSet feature_set)
		{
            Features = GetDenseVector(features);
            _feature_set = feature_set;
		}

        private static DenseVector GetDenseVector(List<double> features)
        {
            double[] features_arr = features.ToArray();
            return new DenseVector(features_arr);
        }
		
		public double[] ToArray(){
			return Features.ToArray();
		}

		public override bool Equals (object obj)
		{
			if(obj is SiteFeatureSet)
			{
				return Features.Equals((obj as SiteFeatureSet).Features);
			}
			else return false;
		}

        public override int GetHashCode()
        {
            return Features.GetHashCode();
        }

		public override string ToString()
		{
			return string.Format("[SiteFeatureSet: {0}]", _feature_set.StringFeatures(Features));
		}
	}
}

