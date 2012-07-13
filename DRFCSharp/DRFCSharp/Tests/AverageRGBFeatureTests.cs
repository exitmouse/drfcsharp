using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DRFCSharp
{
	[TestFixture]
	public class AverageRGBFeatureTests
	{
		private ImageWindowScheme iws;
		private AverageRGBFeature a50;
		[TestFixtureSetUp]
		public void Init()
		{
			iws = new ImageWindowScheme(2, 2, 256, 256, 4);
			a50 = new AverageRGBFeature(iws);
		}
		[Test]
		public void AverageRGBLengthTest()
		{
			List<double> result = ResourceManager.UsingTestingBitmap("testgrid1px.png", (bmp) => {
				return a50.Calculate(bmp, 6, 6);
			});
			Assert.AreEqual(a50.Length, result.Count);
		}
	}
}