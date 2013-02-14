using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DRFCSharp
{
	[TestFixture]
	public class ImageDataTests
	{
		private ImageData imgd;

		[TestFixtureSetUp]
		public void Init ()
		{
			imgd = ResourceManager.UsingTestingBitmap("testgrid1px.png", img => ImageData.FromImage(img));
		}
		[Test]
		public void GetNeighborsSanity ()
		{
			List<Tuple<int,int>> test = imgd.GetNeighbors(1,1);
			Assert.Contains(Tuple.Create<int,int>(0,1),test);
		}
		[Test]
		public void GetNeighborsCorners ()
		{
			List<Tuple<int,int>> test = imgd.GetNeighbors(1,1);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(0,0)));
		}
		[Test]
		public void GetNeighborsEdges ()
		{
			List<Tuple<int,int>> test = imgd.GetNeighbors(imgd.XSites - 1,0);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(imgd.XSites,0)));
		}
		[Test]
		public void GetNewConnectionsSanity ()
		{
			List<Tuple<int,int>> test = imgd.GetNewConnections(1,1);
			Assert.IsFalse(test.Contains(Tuple.Create<int,int>(0,1)));
			Assert.Contains(Tuple.Create<int,int>(2,1),test);
		}
		[Test]
		public void GetNewConnectionsFarEdge ()
		{
			List<Tuple<int, int>> STOPAUTOCOMPLETINGTHISVARIABLEDAMMIT = imgd.GetNewConnections(imgd.XSites-1, imgd.YSites-1);
			Assert.IsEmpty(STOPAUTOCOMPLETINGTHISVARIABLEDAMMIT);
		}
		[Test]
		public void InBoundsFarEdge ()
		{
			Assert.IsFalse(imgd.InBounds(imgd.XSites,0));
		}
	}
}

