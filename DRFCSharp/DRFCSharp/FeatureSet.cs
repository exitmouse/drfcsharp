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
		private List<Feature> Features{ get; set; }
		public FeatureSet()
		{
			Features = new List<Feature>();
		}

		public void AddFeature(Feature f)
		{
			Features.Add(f);
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
			return dv;
		}
	}
}

