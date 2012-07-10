using System;
using System.Collections.Generic;
using System.Drawing;

namespace DRFCSharp
{
	public class RightAngleFeature : Feature
	{
		private HogsMaker SharedHogs { get; set; }
		private FeatureApplicationScheme Windower { get; set; }
		private int NumPeaks { get; set; }
		public RightAngleFeature(HogsMaker sh, FeatureApplicationScheme windower, int num_peaks)
		{
			SharedHogs = sh;
			Windower = windower;
			NumPeaks = num_peaks;
		}
		public List<double> Calculate(Bitmap bmp, int x, int y)
		{
			List<double> result = new List<double>();
			for(int scale = 0; scale < Windower.NumScales; scale++)
			{
				double[] shogs = SharedHogs.GetSmoothedHogs(bmp, Windower[x, y, scale]); //TODO consider GetHogs instead
				result.Add(RightAngleFinder(shogs, NumPeaks));
			}
			return result;
		}
		public static double RightAngleFinder(double[] histogram, int num_peaks_to_consider)
		{
			int[] peak_indices = GetPeakIndices(histogram, num_peaks_to_consider);
			if(peak_indices[0] == -1)
			{
				return 0.0d;
			}
			double toReturn = 0.0d;
			for (int j = num_peaks_to_consider -1; j > 0; j--)
			{
				for (int i = j-1; i >= 0 && peak_indices[j] != -1; i--)
				{
					double ang1 = (2*Math.PI/((double)histogram.Length))*((double)peak_indices[i]);
					double ang2 = (2*Math.PI/((double)histogram.Length))*((double)peak_indices[j]);
					double ang = ang1-ang2;
					double interim = Math.Sin(ang);
					toReturn = Math.Max(toReturn, Math.Abs(interim));
				
					/*Console.WriteLine("RightAngleFinder");
					Console.WriteLine(histogram.ToString());
					Console.WriteLine(string.Format("{0}th peak: {1} \t {2}th peak: {3}\nright-angle feature: {4}",
					                                i, peak_indices[i],
					                                j, peak_indices[j],
					                                toReturn));*/
				}
			}
			return toReturn;
		}
		public static int[] GetPeakIndices(double[] histogram, int num_peaks_to_consider)
		{
			int[] peak_indices = new int[num_peaks_to_consider];
			int[] indices = new int[histogram.Length];
			for(int i = 0; i < indices.Length; i++)
				indices[i] = i;
			Array.Sort(histogram, indices);
			/*Get the top num_peaks_to_consider entries of indices, which is sorted in the reverse of the order we want.
			 *If we don't have enough entries, fill the rest with -1s*/
			for(int i = 0; i < peak_indices.Length; i++)
				peak_indices[i] = (i < indices.Length)? indices[indices.Length - i - 1] : -1;
			return peak_indices;
		}
	}
}

