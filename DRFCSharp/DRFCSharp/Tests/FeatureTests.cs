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
			Label[,] t = new Label[5, 5];
			Assert.AreEqual(t[0,0],Label.OFF);
		}
		[Test]
		public void CrossFeaturesConcatenate ()
		{
			double[] hard1 = new double[4]{1.2,7,2.4,-1.3};
			double[] hard2 = new double[4]{2,-3.8,7.6,100};
			DenseVector f1 = new DenseVector(hard1);
			DenseVector f2 = new DenseVector(hard2);
			CrossFeatureStrategy crosser = ConcatenateFeatures.INSTANCE;
			//DenseVector expected_result = new DenseVector(new double[2*SiteFeatureSet.NUM_FEATURES + 1]{1,1.2,7,2.4,-1.3, 0, 0, 0,2,-3.8,7.6,100, 0, 0, 0});
			Assert.AreEqual(crosser.Cross(new SiteFeatureSet(f1),new SiteFeatureSet(f2)).SubVector(1,4),new DenseVector(hard1));
		}
		[Test]
		public void CrossFeaturesConcatenateComponents ()
		{
			double[] hard1 = new double[4]{1.2,7,2.4,-1.3};
			double[] hard2 = new double[4]{2,-3.8,7.6,100};
			DenseVector f1 = new DenseVector(hard1);
			DenseVector f2 = new DenseVector(hard2);
			CrossFeatureStrategy crosser = ConcatenateFeatures.INSTANCE;
			//DenseVector expected_result = new DenseVector(new double[2*SiteFeatureSet.NUM_FEATURES + 1]{1,1.2,7,2.4,-1.3, 0, 0, 0,2,-3.8,7.6,100, 0, 0, 0});
			Assert.AreEqual(crosser.Cross(new SiteFeatureSet(f1),new SiteFeatureSet(f2)).Count, crosser.Components(f1.Count));
		}
		[Test]
		public void TransformedFeatureVectors ()
		{
			double[] hard1 = new double[4]{1.2,7,2.4,-1.3};
			DenseVector f1 = new DenseVector(hard1);
			double[] exps = new double[hard1.Length + 1];
			for(int i = 1; i < hard1.Length; i++) exps[i] = 0d;
			exps[0] = 1d;
			hard1.CopyTo(exps,1);
			DenseVector expected_result = new DenseVector(exps);
			TransformFeatureStrategy transformer = LinearBasis.INSTANCE;
			Assert.AreEqual(transformer.Transform(new SiteFeatureSet(f1)),expected_result);
		}
		[Test]
		public void TransformedFeatureVectorsComponents ()
		{
			double[] hard1 = new double[4]{1.2,7,2.4,-1.3};
			DenseVector f1 = new DenseVector(hard1);
			TransformFeatureStrategy transformer = LinearBasis.INSTANCE;
			Assert.AreEqual(transformer.Transform(new SiteFeatureSet(f1)).Count, transformer.Components(f1.Count));
		}
		[Test]
		public void SiteFeatureSetInitialization ()
		{
			SiteFeatureSet[,] sitesarray = new SiteFeatureSet[3,3];
			SiteFeatureSet.Init(sitesarray, new DenseVector(1, 2.3d));
			Assert.IsNotNull(sitesarray[1,1]);
			Assert.AreEqual(2.3d, sitesarray[1,1].Features[0]);
		}
		[Test]
		public void GaussiansAreCorrectSize ()
		{
			DenseVector dv = GradientArrayMaker.MakeGaussian(0.5);
			DenseVector dvdx = GradientArrayMaker.MakeGaussianDerivative(0.5);
			Assert.AreEqual(dv.Count,3);
			Assert.AreEqual(dvdx.Count,3);
		}
		[Test]
		public void GaussianMeanIsCentered ()
		{
			DenseVector dv = GradientArrayMaker.MakeGaussian(0.5);
			int midindex = dv.Count/2;
			Assert.GreaterOrEqual(dv[midindex], dv[midindex-1]);
			Assert.GreaterOrEqual(dv[midindex], dv[midindex+1]);
		}
	}
}

