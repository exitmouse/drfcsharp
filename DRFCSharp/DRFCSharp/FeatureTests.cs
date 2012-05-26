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
		public void CollectionOfLabelsInitialization ()
		{
			Label[,] t = new Label[ImageData.x_sites, ImageData.y_sites];
			Assert.AreEqual(t[0,0],Label.OFF);
		}
		[Test]
		public void CrossFeaturesConcatenate ()
		{
			double[] hard1 = new double[4]{1.2,7,2.4,-1.3};
			double[] hard2 = new double[4]{2,-3.8,7.6,100};
			double[] dubs1 = new double[SiteFeatureSet.NUM_FEATURES];
			double[] dubs2 = new double[SiteFeatureSet.NUM_FEATURES];
			for(int i = 0; i < SiteFeatureSet.NUM_FEATURES; i++)
			{
				dubs1[i] = 0d;
				dubs2[i] = 0d;
			}
			hard1.CopyTo(dubs1,0);
			hard2.CopyTo(dubs2,0);
			DenseVector f1 = new DenseVector(dubs1);
			DenseVector f2 = new DenseVector(dubs2);
			//DenseVector expected_result = new DenseVector(new double[2*SiteFeatureSet.NUM_FEATURES + 1]{1,1.2,7,2.4,-1.3, 0, 0, 0,2,-3.8,7.6,100, 0, 0, 0});
			Assert.AreEqual(SiteFeatureSet.CrossFeaturesConcatenate(new SiteFeatureSet(f1),new SiteFeatureSet(f2)).SubVector(1,4),new DenseVector(hard1));
		}
		[Test]
		public void TransformedFeatureVectors ()
		{
			double[] hard1 = new double[4]{1.2,7,2.4,-1.3};
			double[] fs = new double[SiteFeatureSet.NUM_FEATURES];
			for(int i = 0; i < SiteFeatureSet.NUM_FEATURES; i++) fs[i] = 0d;
			hard1.CopyTo(fs,0);
			DenseVector f1 = new DenseVector(fs);
			
			double[] exps = new double[SiteFeatureSet.NUM_FEATURES + 1];
			for(int i = 1; i < SiteFeatureSet.NUM_FEATURES; i++) exps[i] = 0d;
			exps[0] = 1d;
			hard1.CopyTo(exps,1);
			DenseVector expected_result = new DenseVector(exps);
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
		public void GetNewConnectionsFarEdge ()
		{
			List<Tuple<int, int>> STOPAUTOCOMPLETINGTHISVARIABLEDAMMIT = ImageData.GetNewConnections(ImageData.x_sites-1, ImageData.y_sites-1);
			Assert.IsEmpty(STOPAUTOCOMPLETINGTHISVARIABLEDAMMIT);
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
			DenseVector dvdx = ImageData.MakeGaussianDerivative();
			Assert.AreEqual(dv.Count,3);
			Assert.AreEqual(dvdx.Count,3);
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

