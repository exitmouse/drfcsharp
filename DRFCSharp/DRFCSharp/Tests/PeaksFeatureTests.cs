using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DRFCSharp
{
	[TestFixture]
	public class PeaksFeatureTests
	{
		private GradientArrayMaker gm;
		private ImageWindowScheme iws;
		private HogsMaker hm50, hm2;
		private PeaksFeature pf50, pf2;
		[TestFixtureSetUp]
		public void Init()
		{
			gm = new GradientArrayMaker(0.5d);
			iws = new ImageWindowScheme(2, 2, 256, 256, 4);
			hm50 = new HogsMaker(gm, 50);
			pf50 = new PeaksFeature(hm50, iws);
			hm2 = new HogsMaker(gm, 2);
			pf2 = new PeaksFeature(hm2, iws);
		}
		[Test]
		public void PeaksLengthTest()
		{
			List<double> result = ResourceManager.UsingTestingBitmap("testgrid1px.png", (bmp) => {
				return pf50.Calculate(bmp, 6, 6);
			});
			Assert.AreEqual(pf50.Length, result.Count);
		}
	}
}

