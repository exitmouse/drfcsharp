using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class SiteFeatureSet
	{
		public DenseVector Features{ get; private set; }
		public SiteFeatureSet (DenseVector features)
		{
			this.Features = features;
		}
		
		public double[] ToArray(){
			return Features.ToArray();
		}

		public override bool Equals (object obj)
		{
			if(obj is SiteFeatureSet)
			{
				return Features.Equals((obj as SiteFeatureSet).Features);
			}
			else return false;
		}

		public override string ToString()
		{
			return string.Format("[SiteFeatureSet: Features={0}]", Features);
		}

		public static void Init(SiteFeatureSet[,] sitesarray, DenseVector init)
		{
			for(int i = 0; i < sitesarray.GetLength(0); i++)
			{
				for(int j = 0; j < sitesarray.GetLength(1); j++)
				{
					sitesarray[i,j] = new SiteFeatureSet((DenseVector)init.Clone());
				}
			}
		}
	}
}

