using NUnit.Framework;
using System;

namespace DRFCSharp
{
	[TestFixture]
	public class WindowTests
	{
		[Test]
		public void EndCalculationTest()
		{
			Window w = Window.FromSize(12,0,4,4);
			Assert.AreEqual(16, w.EndX);
			Assert.AreEqual(4, w.EndY);
		}
		[Test]
		public void NegativeWindowsNullTest()
		{
			Window w = Window.FromSize(0,0,1,-1);
			Assert.IsNull(w);
		}
		[Test]
		public void ZeroWindowsOkayTest()
		{
			Window w = Window.FromSize(0,0,0,0);
			Assert.IsNotNull(w);
		}
		[Test]
		public void SizeCalculationTest()
		{
			Window w = Window.FromBounds(0, 4, 6, 10);
			Assert.AreEqual(6, w.Width);
			Assert.AreEqual(6, w.Height);
		}
		[Test]
		public void OnePixelWindowBoundsTest()
		{
			Window w = Window.FromBounds(0, 0, 1, 1);
			Assert.IsNotNull(w);
		}
		[Test]
		public void SizeCalculationHandlesZeroTest()
		{
			Window w = Window.FromBounds(0, 0, 0, 6);
			Assert.AreEqual(0, w.Width);
			Assert.AreEqual(6, w.Height);
		}
		[Test]
		public void AreaCalculationTest()
		{
			Window w = Window.FromSize(12, -39, 40, 39);
			Assert.AreEqual(40*39, w.Area);
			Window x = Window.FromSize(5, 5, 0, 10);
			Assert.AreEqual(0, x.Area);
		}
		[Test]
		public void FromCenterGivesCorrectWidthTest()
		{
			Window w = Window.FromCenter(0, 0, 40, 41);
			Assert.AreEqual(40, w.Width);
			Assert.AreEqual(41, w.Height);
		}
		[Test]
		public void FromCenterGivesExpectedBoundsTest()
		{
			Window w = Window.FromCenter(0, 0, 3, 5);
			Assert.AreEqual(-1, w.StartX);
			Assert.AreEqual(2, w.EndX);
			Assert.AreEqual(-2, w.StartY);
			Assert.AreEqual(3, w.EndY);
		}
		[Test]
		public void ConstrainDoesNothingWithinBoundsTest()
		{
			Window w = Window.FromBounds(0, 2, 20, 20);
			Window w_constrained = w.Constrain(0, -1, 40, 21);
			Assert.That(w.Equals(w_constrained));
		}
		[Test]
		public void ConstrainFailsEntirelyOutsideBoundsBothDirectionsTest()
		{
			Window w = Window.FromBounds(0,0,1,1);
			Assert.IsNull(w.Constrain(4, 4, 5, 5));
		}
		[Test]
		public void ConstrainFailsEntirelyOutsideBoundsOneDirectionTest()
		{
			Window w = Window.FromSize(0,0,1,1);
			Assert.IsNull(w.Constrain(10,-2,20,2));
		}
		[Test]
		public void ConstrainFailsWithReversedBoundariesTest()
		{
			Window w = Window.FromBounds(0,0,1,1);
			Assert.IsNull(w.Constrain(2, 2, -2, -2));
		}
	}
}

