using System;
using System.Collections.Generic;
using System.Drawing;

namespace DRFCSharp
{
	public class AverageRGBFeature : Feature
	{
		private FeatureApplicationScheme Windower { get; set; }
		public int Length {
			get {
				return Windower.NumScales * 3;
			}
		}
        public string Name(int idx)
        {
            return string.Format("AverageRGBFeature [Color: {0}, Scale: {1}]", GetColorName(idx % 3), idx/3);
        }
        private string GetColorName(int color_idx)
        {
            switch(color_idx)
            {
                case 0:
                    return "Red";
                case 1:
                    return "Green";
                case 2:
                    return "Blue";
                default:
                    return "That is not a color.";
            }
        }

		public AverageRGBFeature(FeatureApplicationScheme windower)
		{
			Windower = windower;
		}
		public List<double> Calculate(Bitmap bmp, int x, int y)
		{
			List<double> result = new List<double>();
			for(int scale = 0; scale < Windower.NumScales; scale++)
			{
				result.AddRange(AverageRGB(bmp, Windower[x, y, scale]));
			}
			return result;
		}
		public static double[] AverageRGB(Bitmap img, Window w)
		{
			double[] colors = new double[3];
			for(int x = w.StartX; x < w.EndX; x++) for(int y = w.StartY; y < w.EndY; y++)
			{
				Color pixval = img.GetPixel(x,y);
				colors[0] += pixval.R;
				colors[1] += pixval.G;
				colors[2] += pixval.B;
			}
			double multfactor = 1/((double)w.Area);
			for(int i = 0; i < 3; i++) colors[i] *= multfactor;
			return colors;
		}
	}
}

