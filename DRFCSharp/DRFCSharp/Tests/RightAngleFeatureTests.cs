using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DRFCSharp
{
	[TestFixture]
	public class RightAngleFeatureTests
	{
		private GradientArrayMaker gm;
		private ImageWindowScheme iws;
		private HogsMaker hm50, hm2;
		private RightAngleFeature raf50, raf2;
		[TestFixtureSetUp]
		public void Init()
		{
			gm = new GradientArrayMaker(0.5d);
			iws = new ImageWindowScheme(2, 2, 256, 256, 4);
			hm50 = new HogsMaker(gm, 50);
			raf50 = new RightAngleFeature(hm50, iws, 2);
			hm2 = new HogsMaker(gm, 2);
			raf2 = new RightAngleFeature(hm2, iws, 1);
		}
		[Test]
		public void NumPeaksLargerThanNumOrientationsTest()
		{
			RightAngleFeature raftoobig = new RightAngleFeature(hm2, iws, 60);
			List<double> result = ResourceManager.UsingTestingBitmap("testgrid1px.png", (bmp) => {
				return raftoobig.Calculate(bmp, 6, 6);
			});
			Assert.AreEqual(raftoobig.Length, result.Count);
		}
	}
}

