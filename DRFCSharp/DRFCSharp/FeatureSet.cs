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
		public FeatureSet()
		{
			Features = new List<Feature>();
			Length = 0;
		}

		public void AddFeature(Feature f)
		{
			Features.Add(f);
			Length += f.Length;
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

