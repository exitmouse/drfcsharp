using NUnit.Framework;
using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	[TestFixture]
	public class FeatureTests
	{
		[Test]
		public void CrossFeatures ()
		{
			DenseVector f1 = new DenseVector(new double[SiteFeatureSet.NUM_FEATURES]{1.2,7,2.4,-1.3});
			DenseVector f2 = new DenseVector(new double[SiteFeatureSet.NUM_FEATURES]{2,-3.8,7.6,100});
			DenseVector expected_result = new DenseVector(new double[2*SiteFeatureSet.NUM_FEATURES + 1]{1,1.2,7,2.4,-1.3,2,-3.8,7.6,100});
			Assert.AreEqual(SiteFeatureSet.CrossFeatures(new SiteFeatureSet(f1),new SiteFeatureSet(f2)),expected_result);
		}
		[Test]
		public void TransformedFeatureVectors ()
		{
			DenseVector f1 = new DenseVector(new double[SiteFeatureSet.NUM_FEATURES]{1.2,7,2.4,-1.3});
			DenseVector expected_result = new DenseVector(new double[SiteFeatureSet.NUM_FEATURES+1]{1,1.2,7,2.4,-1.3});
			Assert.AreEqual(SiteFeatureSet.TransformedFeatureVector(new SiteFeatureSet(f1)),expected_result);
	
		}
		[Test]
		public void GetNeighborsSanity ()
		{
			List<Tuple<int,int>> test = ImageData.GetNeighbors(1,1);
			Assert.Contains(Tuple.Create<int,int>(0,1),test);
		}
		[Test]
		public void GetNeighborsCorners ()
		{
			List<Tuple<int,int>> test = ImageData.GetNeighbors(1,1);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(0,0)));
		}
		[Test]
		public void GetNeighborsEdges ()
		{
			List<Tuple<int,int>> test = ImageData.GetNeighbors(ImageData.x_sites - 1,0);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(ImageData.x_sites,0)));
		}
		[Test]
		public void GetNewConnectionsSanity ()
		{
			List<Tuple<int,int>> test = ImageData.GetNewConnections(1,1);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(0,1)));
			Assert.Contains(Tuple.Create<int,int>(2,1),test);
		}
		[Test]
		public void InBoundsFarEdge ()
		{
			Assert.IsFalse(ImageData.InBounds(ImageData.x_sites,0));
		}
		[Test]
		public void SiteFeatureSetInitialization ()
		{
			SiteFeatureSet[,] sitesarray = new SiteFeatureSet[3,3];
			SiteFeatureSet.Init(sitesarray);
			Assert.IsNotNull(sitesarray[1,1]);
		}
		[Test]
		public void GaussiansAreCorrectSize ()
		{
			DenseVector dv = ImageData.MakeGaussian();
			Assert.AreEqual(dv.Count,3);
		}
		[Test]
		public void GaussianMeanIsCentered ()
		{
			DenseVector dv = ImageData.MakeGaussian();
			int midindex = dv.Count/2;
			Assert.GreaterOrEqual(dv[midindex], dv[midindex-1]);
			Assert.GreaterOrEqual(dv[midindex], dv[midindex+1]);
		}
	}
}

