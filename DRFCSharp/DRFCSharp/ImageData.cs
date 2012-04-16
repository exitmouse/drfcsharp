using System;

namespace DRFCSharp
{
	public class ImageData
	{
		public const int x_sites = 16;
		public const int y_sites = 16;
		public SiteFeatureSet[,] site_features;
		public ImageData (SiteFeatureSet[,] site_features)
		{
			if(site_features.GetLength(0) != x_sites || site_features.GetLength(1) != y_sites)
				throw new ArgumentException("Wrong size of test input for this model");
			this.site_features = site_features;
		}
		
		public SiteFeatureSet this[int value1,int value2]{
			get{
				return this.site_features[value1,value2];
			}
		}
	}
}

