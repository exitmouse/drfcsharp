using NUnit.Framework;
using System;
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
	}
}

