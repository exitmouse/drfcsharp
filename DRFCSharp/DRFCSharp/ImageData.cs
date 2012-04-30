using System;
using System.Collections.Generic;

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
		private static bool IsEarlier(int x1, int y1, int x2, int y2)
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
	}
}

