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
			int[] peak_indices = new int[num_peaks_to_consider]; 
			for (int i = 0; i < num_peaks_to_consider; i++)
			{
				peak_indices[i] = -1;
			}
			double last_height_level = -1.0d;
			// Traverse the histogram in backward order to find the first index having
			// different height than histogram[0].  This will be the initial value of 
			// last_height_level.
			for (int i = histogram.Length - 1; i >= 0; i--)
			{
				if (i == 0)
				{
					// the entire histogram has uniform distribution
					return 0.0d;
				}
				else if (histogram[i] != histogram[0])
				{
					last_height_level = histogram[i];
					break;
				}
			}
			// Now traverse the histogram in forwards order finding the peaks and right
			// plateau corners.
			for (int i = 0; i < histogram.Length; i++)
			{
				if (histogram[i] > last_height_level && histogram[i] > histogram[(i + 1) % histogram.Length])
				{
					// this is a peak (we accept as a peak the right corner of a plateau).
					for (int j = 0; j < num_peaks_to_consider; j++)
					{
						if (peak_indices[j] == -1 || histogram[i] > histogram[peak_indices[j]])
						{
							// shift all the lesser peaks
							for (int k = num_peaks_to_consider -1; k > j; k--)
							{
								peak_indices[k] = peak_indices[k-1];
							}
							// and save this one
							peak_indices[j] = i;
							break;
						}
					}
				}
				if (histogram[(i + 1) % histogram.Length] != histogram[i]) 
				{
					last_height_level = histogram[i];
				}
			}
			if(peak_indices[1] == -1)
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
	}
}

