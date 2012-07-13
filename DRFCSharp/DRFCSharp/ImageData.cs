using System;
using System.Drawing;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ImageData
	{
		private SiteFeatureSet[,] site_features;
		public int FeatureCount { get; private set; }
		public int XSites { get; private set; }
		public int YSites { get; private set; }
		public static int Ons_seen = 0;
		public static int Sites_seen = 0;
		public static int Images_seen = 0;
		private static GradientArrayMaker gm = new GradientArrayMaker(0.5d);
		private static HogsMaker hm = new HogsMaker(gm, 50);
		private ImageData (SiteFeatureSet[,] site_features, int feature_count)
		{
			if(site_features == null) throw new ArgumentNullException("site_features");
			XSites = site_features.GetLength(0);
			YSites = site_features.GetLength(1);
			if(XSites == 0 || YSites == 0) throw new ArgumentException("site_features must not be size zero");
			if(site_features[0,0] == null) throw new ArgumentException("site_features must be initialized");
			if(site_features[0,0].Features.Count != feature_count) throw new ArgumentException("wtf"); //TODO remove this or remove the parameter.
			FeatureCount = feature_count;
			this.site_features = site_features;
		}
		
		public SiteFeatureSet this[int value1,int value2]{
			get{
				return this.site_features[value1,value2];
			}
		}
		public List<Tuple<int,int>> GetNeighbors(int x, int y)
		{
			List<Tuple<int,int>> toReturn = new List<Tuple<int, int>>();
			for(int horz = -1; horz <=1; horz++)
				for(int vert = -1; vert <=1; vert++)
					if(Math.Abs(horz+vert)==1) //Hacky way of ensuring precisely one of the components is nonzero.
						if(InBounds(x+horz,y+vert))
							toReturn.Add(Tuple.Create<int,int>(x+horz,y+vert));
			return toReturn;
		}
		public bool InBounds(int x, int y)
		{
			if(x >= 0 && x < XSites && y >= 0 && y < YSites)
				return true;
			return false;
		}
		public static bool IsEarlier(int x1, int y1, int x2, int y2)
		{
			if(y1 == y2) return x1 < x2;
			else return y1 < y2;
		}
		public List<Tuple<int,int>> GetNewConnections(int x, int y)
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
			return new ImageData(sitefeatures, feature_set.Length);
		}
	}
}

