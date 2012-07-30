using System;
using System.Collections.Generic;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	/// <summary>
	/// A feature set is (too hard to document right now evidently, because I need to sleep)
	/// </summary>
	public sealed class FeatureSet
	{
		private List<Feature> Features { get; set; }
		public int Length { get; private set; }
		private FeatureSet(Builder builder)
		{
            Features = builder.Features;
            Length = builder.Length;
		}

        public class Builder
        {
            public List<Feature> Features { get; private set; }
            public int Length { get; private set; }
            public Builder()
            {
                Features = new List<Feature>();
            }
            public Builder AddFeature(Feature f)
            {
                Features.Add(f);
                Length += f.Length;
                return this;
            }
            public FeatureSet Build()
            {
                return new FeatureSet(this);
                //TODO assert the length was calculated right?
            }
        }

        public string StringFeatures(DenseVector dv)
        {
            if(dv.Count != Length) throw new ArgumentException();
            string to_return = "";
            for(int i = 0; i < Length; i++)
            {
                to_return += string.Format("{0}: {1}", GetFeatureName(i), dv[i]);
                if(i < Length - 1) to_return += ", ";
            }
            return to_return;
        }

        private string GetFeatureName(int component_idx)
        {
            int feature_idx = 0;
            while(component_idx >= Features[feature_idx].Length)
            {
                component_idx -= Features[feature_idx].Length;
                feature_idx++;
            }
            return Features[feature_idx].Name(component_idx);
        }

		public DenseVector ApplyToBitmap(Bitmap bmp, int x, int y)
		{
			List<double> feature_results = new List<double>();
			foreach(Feature f in Features)
			{
				//Evaluate the feature on the bitmap over the window dictated by the scheme.
				feature_results.AddRange(f.Calculate(bmp, x, y));
			}
			DenseVector dv = new DenseVector(feature_results.ToArray());
			//TODO figure out the right pattern for validating this.
			if(dv.Count != Length) throw new InvalidOperationException(string.Format("Length out of sync: Length: {0}, Actual: {1}", Length, dv.Count));
			return dv;
		}
	}
}

