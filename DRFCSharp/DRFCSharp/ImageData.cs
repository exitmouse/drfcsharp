using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ImageData
	{
		public static int x_sites = 24; //Make sure these divide the image dimensions. The size of the sites is deduced from them.
		public static int y_sites = 16;
		public const double variation = 0.5d; //Make sure 6*variation is odd.
		public const int NUM_ORIENTATIONS = 50;
		public SiteFeatureSet[,] site_features;
		public static int Ons_seen = 0;
		public static int Sites_seen = 0;
		public static int Images_seen = 0;
		private static GradientArrayMaker gm = new GradientArrayMaker(0.5d);
		private static HogsMaker hm = new HogsMaker(gm, 50);
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
		public static List<Tuple<int,int>> GetNeighbors(int x, int y)
		{
			List<Tuple<int,int>> toReturn = new List<Tuple<int, int>>();
			for(int horz = -1; horz <=1; horz++)
				for(int vert = -1; vert <=1; vert++)
					if(Math.Abs(horz+vert)==1) //Hacky way of ensuring precisely one of the components is nonzero.
						if(InBounds(x+horz,y+vert))
							toReturn.Add(Tuple.Create<int,int>(x+horz,y+vert));
			return toReturn;
		}
		public static bool InBounds(int x, int y)
		{
			if(x >= 0 && x < x_sites && y >= 0 && y < y_sites)
				return true;
			return false;
		}
		public static bool IsEarlier(int x1, int y1, int x2, int y2)
		{
			if(y1 == y2) return x1 < x2;
			else return y1 < y2;
		}
		public static List<Tuple<int,int>> GetNewConnections(int x, int y)
		{
			List<Tuple<int,int>> toReturn = GetNeighbors(x,y);
			toReturn.RemoveAll((t) => IsEarlier(t.Item1,t.Item2,x,y));
			return toReturn;
		}
		public static ImageData FromImage(Bitmap img)
		{
			ImageWindowScheme iws = new ImageWindowScheme(16, 16, img.Width, img.Height, 3);
			SiteFeatureSet[,] sitefeatures = new SiteFeatureSet[iws.NumXs, iws.NumYs];

			MomentFeature moment0 = new MomentFeature(hm, iws, 0);
			MomentFeature moment2 = new MomentFeature(hm, iws, 2);
			RightAngleFeature right_angles = new RightAngleFeature(hm, iws, 3);
			AverageRGBFeature avg_rgb = new AverageRGBFeature(iws);
			PeaksFeature peaks = new PeaksFeature(hm, iws);

			FeatureSet feature_set = new FeatureSet();
			feature_set.AddFeature(moment0);
			feature_set.AddFeature(moment2);
			feature_set.AddFeature(right_angles);
			feature_set.AddFeature(avg_rgb);
			feature_set.AddFeature(peaks);
			for(int x = 0; x < iws.NumXs; x++) for(int y = 0; y < iws.NumYs; y++)
			{
				sitefeatures[x,y] = new SiteFeatureSet(feature_set.ApplyToBitmap(img, x, y));
			}
			return new ImageData(sitefeatures);
		}
		public static Classification ImportLabeling(string filename)
		{
			Label[,] labels = new Label[x_sites,y_sites];
			using(StreamReader csvfile = new StreamReader(filename))
			{
				for(int col = 0; col < y_sites; col++)
				{
					string line = csvfile.ReadLine();
					string[] vals = line.Split(',');
					for(int row = 0; row < x_sites; row++)
					{
						int val = Int32.Parse(vals[row]);
						if(val > 0)
							labels[row,col] = Label.ON;
						else
							labels[row,col] = Label.OFF;
						Sites_seen += 1;
						if(labels[row,col]==Label.ON) Ons_seen += 1;
					}
				}
			}
			return new Classification(labels);
		}
	}
}

