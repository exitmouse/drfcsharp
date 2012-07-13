using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DRFCSharp
{
	[TestFixture]
	public class MomentFeatureTests
	{
		private GradientArrayMaker gm;
		private ImageWindowScheme iws;
		private HogsMaker hm50;
		private MomentFeature avg50, second50;
		[TestFixtureSetUp]
		public void Init()
		{
			gm = new GradientArrayMaker(0.5d);
			iws = new ImageWindowScheme(2, 2, 256, 256, 4);
			hm50 = new HogsMaker(gm, 50);
			avg50 = new MomentFeature(hm50, iws, 0);
			second50 = new MomentFeature(hm50, iws, 2);
		}
		[Test]
		public void MomentLengthTest()
		{
			List<double> result = ResourceManager.UsingTestingBitmap("testgrid1px.png", bmp => avg50.Calculate(bmp, 6, 6));
			Assert.AreEqual(avg50.Length, result.Count);
			result = ResourceManager.UsingTestingBitmap("testgrid1px.png", bmp => second50.Calculate(bmp, 6, 6));
			Assert.AreEqual(second50.Length, result.Count);
		}
	}
}