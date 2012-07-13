using System;
using System.Collections.Generic;
using System.Drawing;

namespace DRFCSharp
{
	public class PeaksFeature : Feature
	{
		private HogsMaker SharedHogs { get; set; }
		private FeatureApplicationScheme Windower { get; set; }
		public PeaksFeature(HogsMaker sh, FeatureApplicationScheme windower)
		{
			SharedHogs = sh;
			Windower = windower;
		}
		public int Length {
			get {
				return Windower.NumScales - 1;
			}
		}
		public List<double> Calculate(Bitmap bmp, int x, int y)
		{
			int[] intra_scale_peaks = new int[Windower.NumScales];
			double[] intra_scale_angles = new double[Windower.NumScales];
			for(int scale = 0; scale < Windower.NumScales; scale++)
			{
				intra_scale_peaks[scale] = 0;
				double[] shogs = SharedHogs.GetSmoothedHogs(bmp, Windower[x, y, scale]);
				for(int i = 0; i < shogs.Length; i++) if(shogs[i] > shogs[intra_scale_peaks[scale]]) intra_scale_peaks[scale] = i;
				intra_scale_angles[scale] = (2*Math.PI/((double)shogs.Length))*((double)intra_scale_peaks[scale]);
			}
			List<double> result = new List<double>();
			for(int u = 0; u < intra_scale_angles.Length-1; u++){
				result.Add(Math.Abs(Math.Cos(2*(intra_scale_angles[u]-intra_scale_angles[u+1]))));
			}
			return result;
		}
	}
}

