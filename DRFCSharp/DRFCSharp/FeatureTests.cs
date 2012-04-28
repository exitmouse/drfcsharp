using NUnit.Framework;
using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	[TestFixture()]
	public class FeatureTests
	{
		[Test()]
		public void CrossFeatures ()
		{
			DenseVector f1 = new DenseVector(new double[5]{1.2,3,7,2.4,-1.3});
			DenseVector f2 = new DenseVector(new double[5]{2,-3.8,7.6,100,5});
			DenseVector expected_result = new DenseVector(new double[11]{1,1.2,3,7,2.4,-1.3,2,-3.8,7.6,100,5});
			Assert.AreEqual(SiteFeatureSet.CrossFeatures(new SiteFeatureSet(f1),new SiteFeatureSet(f2)),expected_result);
		}
		[Test()]
		public void TransformedFeatureVectors ()
		{
			DenseVector f1 = new DenseVector(new double[5]{1.2,3,7,2.4,-1.3});
			DenseVector expected_result = new DenseVector(new double[6]{1,1.2,3,7,2.4,-1.3});
			Assert.AreEqual(SiteFeatureSet.TransformedFeatureVector(new SiteFeatureSet(f1)),expected_result);
	
		}
		[Test()]
		public void GetNeighborsSanity ()
		{
			List<Tuple<int,int>> test = ImageData.GetNeighbors(1,1);
			Assert.Contains(Tuple.Create<int,int>(0,1),test);
		}
		[Test()]
		public void GetNeighborsCorners ()
		{
			List<Tuple<int,int>> test = ImageData.GetNeighbors(1,1);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(0,0)));
		}
		[Test()]
		public void GetNeighborsEdges ()
		{
			List<Tuple<int,int>> test = ImageData.GetNeighbors(ImageData.x_sites - 1,0);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(ImageData.x_sites,0)));
		}
		[Test()]
		public void InBoundsFarEdge ()
		{
			Assert.IsFalse(ImageData.InBounds(ImageData.x_sites,0));
		}
		[Test()]
		public void SiteFeatureSetInitialization ()
		{
			SiteFeatureSet[,] sitesarray = new SiteFeatureSet[3,3];
			SiteFeatureSet.Init(sitesarray);
			Assert.IsNotNull(sitesarray[1,1]);
		}
	}
}
