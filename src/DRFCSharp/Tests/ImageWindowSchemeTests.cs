using System;
using NUnit.Framework;

namespace DRFCSharp
{
	[TestFixture]
	public class ImageWindowSchemeTests
	{
		private ImageWindowScheme iws;
		[TestFixtureSetUp]
		public void Init()
		{
			iws = new ImageWindowScheme(16, 16, 32, 33, 2);
		}
		[Test]
		public void NumWindowsExactTest()
		{
			Assert.AreEqual(2, iws.NumXs);
		}
		[Test]
		public void NumWindowsOffByOneTest()
		{
			Assert.AreEqual(3, iws.NumYs);
		}

	}
}

