using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DRFCSharp
{
	[TestFixture]
	public class ModelTests
	{
		[Test]
		public void SigmaOfZero ()
		{
			Assert.GreaterOrEqual(MathWrapper.Sigma(0),0.49d);
			Assert.LessOrEqual(MathWrapper.Sigma(0),0.51d);
		}
		[Test]
		public void SigmaOfNegativeOneHundred ()
		{
			Assert.LessOrEqual(MathWrapper.Sigma(-100),0.00001d);
		}
		[Test]
		public void SigmaOfOneHundred ()
		{
			Assert.GreaterOrEqual(MathWrapper.Sigma(100),0.99999d);
		}
		[Test]
		public void ModelTrainingConverges ()
		{
			SiteFeatureSet[,] sitesarray = new SiteFeatureSet[ImageData.x_sites,ImageData.y_sites];
			SiteFeatureSet.Init(sitesarray);
			ImageData img = new ImageData(sitesarray);
			Label[,] labels = new Label[ImageData.x_sites,ImageData.y_sites];
			Classification cfc = new Classification(labels);
			ModelBuilder mbr = new ModelBuilder(ConcatenateFeatures.INSTANCE, LinearBasis.INSTANCE, 1d, 3000, 1d, 1d);
			Model mfm = mbr.PseudoLikelihoodTrain("","", new ImageData[1]{img},new Classification[1]{cfc});
			Assert.AreNotEqual(mfm.TimeToConverge, mbr.MaxIters);
		}
		[Test]
		public void CanClassify ()
		{
			SiteFeatureSet[,] sitesarray = new SiteFeatureSet[ImageData.x_sites,ImageData.y_sites];
			SiteFeatureSet.Init(sitesarray);
			ImageData img = new ImageData(sitesarray);
			Label[,] labels = new Label[ImageData.x_sites,ImageData.y_sites];
			Classification cfc = new Classification(labels);
			ModelBuilder mbr = new ModelBuilder(ConcatenateFeatures.INSTANCE, LinearBasis.INSTANCE, 1d, 3000, 1d, 1d);
			Model mfm = mbr.PseudoLikelihoodTrain("", "", new ImageData[1]{img},new Classification[1]{cfc});
			Classification inferred = mfm.MaximumAPosterioriInfer(img);
			Assert.IsNotNull(inferred);
		}
	}
}

