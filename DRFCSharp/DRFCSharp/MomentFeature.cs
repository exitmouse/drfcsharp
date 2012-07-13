using System;
using System.Collections.Generic;
using System.Drawing;

namespace DRFCSharp
{
	public class MomentFeature : Feature
	{
		private HogsMaker SharedHogs { get; set; }
		private FeatureApplicationScheme Windower { get; set; }
		private int Moment { get; set; }
		public int Length {
			get {
				return Windower.NumScales;
			}
		}
		public MomentFeature(HogsMaker sh, FeatureApplicationScheme windower, int moment)
		{
			SharedHogs = sh;
			Windower = windower;
			Moment = moment;
		}
		public List<double> Calculate(Bitmap bmp, int x, int y)
		{
			List<double> result = new List<double>();
			for(int scale = 0; scale < Windower.NumScales; scale++)
			{
				double[] shogs = SharedHogs.GetSmoothedHogs(bmp, Windower[x, y, scale]);
				result.Add(CalculateMoment(shogs, Moment));
			}
			return result;
		}
		private static double CalculateMoment(double[] histogram, int p)
		{
			if(p == 0)
			{
				double sum = 0.0d;         
				for(int i = 0; i < histogram.Length; i++)
				{
					sum += histogram[i];
				}
				return sum/((double)histogram.Length);
			}
			else
			{
				double v_0 = CalculateMoment(histogram, 0);
				double numerator = 0.0d;
				double denom = 0.0d;
				for(int i = 0; i < histogram.Length; i++)
				{
					if(histogram[i] <= v_0) continue;
					else
					{
						numerator += Math.Pow((histogram[i]-v_0),p+1);
						denom += histogram[i]-v_0;
					}
				}
				return numerator/(denom+double.Epsilon);
			}
		}
	}
}

