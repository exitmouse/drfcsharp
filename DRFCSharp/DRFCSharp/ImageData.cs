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
		private ImageData (SiteFeatureSet[,] site_features, int feature_count)
		{
			if(site_features == null) throw new ArgumentNullException("site_features");
			XSites = site_features.GetLength(0);
			YSites = site_features.GetLength(1);
			if(XSites == 0 || YSites == 0) throw new ArgumentException("site_features must not be size zero");
			FeatureCount = feature_count;
			this.site_features = site_features;
		}
		
		public SiteFeatureSet this[int value1,int value2]{
			get{
				return this.site_features[value1,value2];
			}
		}

        public class Factory
        {
            public int FeatureCount { get; private set; }
            public int XSites { get; private set; }
            public int YSites { get; private set; }
            public int SitesSeen { get; private set; }
            public int ImagesSeen { get; private set; }
            public FeatureSet Features { get; private set; }
            public Factory (int xsites, int ysites, FeatureSet f)
            {
                XSites = xsites;
                YSites = ysites;
                if(XSites == 0 || YSites == 0) throw new ArgumentException("site_features must not be size zero");
                FeatureCount = f.Length;
                Features = f;
                SitesSeen = 0;
                ImagesSeen = 0;
            }
            public ImageData FromImage(Bitmap img)
            {
                //TODO don't make the sitefeaturesarray! start with an 
                //empty imagedata object and fill it out.
                //That way you can verify the length invariants.
                SiteFeatureSet[,] sitefeatures = new SiteFeatureSet[XSites, YSites];
                for(int x = 0; x < XSites; x++) for(int y = 0; y < YSites; y++)
                {
                    sitefeatures[x,y] = new SiteFeatureSet(feature_set.ApplyToBitmap(img, x, y), feature_set);
                    //TODO Assert that feature_set.Length == sitefeatures[x,y].Length
                }
                SitesSeen += XSites*YSites;
                ImagesSeen += 1;
                return new ImageData(sitefeatures, feature_set.Length);
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
		public static ImageData FromImage(Bitmap img, ImageWindowScheme iws, FeatureSet feature_set)
		{
            SiteFeatureSet[,] sitefeatures = new SiteFeatureSet[iws.NumXs, iws.NumYs];
			for(int x = 0; x < iws.NumXs; x++) for(int y = 0; y < iws.NumYs; y++)
			{
				sitefeatures[x,y] = new SiteFeatureSet(feature_set.ApplyToBitmap(img, x, y), feature_set);
			}
			return new ImageData(sitefeatures, feature_set.Length);
		}
	}
}

