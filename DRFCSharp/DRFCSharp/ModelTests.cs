using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DRFCSharp
{
	[TestFixture()]
	public class ModelTests
	{
		[Test()]
		public void SigmaOfZero ()
		{
			Assert.GreaterOrEqual(ModifiedModel.Sigma(0),0.49d);
			Assert.LessOrEqual(ModifiedModel.Sigma(0),0.51d);
		}
		[Test()]
		public void SigmaOfOneHundred ()
		{
			Assert.LessOrEqual(ModifiedModel.Sigma(100),0.00001d);
		}
		[Test()]
		public void SigmaOfNegativeOneHundred ()
		{
			Assert.GreaterOrEqual(ModifiedModel.Sigma(-100),0.99999d);
		}
		[Test()]
		public void ModelTrainingConverges ()
		{
			SiteFeatureSet[,] sitesarray = new SiteFeatureSet[ImageData.x_sites,ImageData.y_sites];
			SiteFeatureSet.Init(sitesarray);
			ImageData img = new ImageData(sitesarray);
			Label[,] labels = new Label[ImageData.x_sites,ImageData.y_sites];
			Classification cfc = new Classification(labels);
			ModifiedModel mfm = ModifiedModel.PseudoLikelihoodTrain(new ImageData[1]{img},new Classification[1]{cfc},1d);
			Assert.AreNotEqual(mfm.time_to_converge, ModifiedModel.MAX_ITERS);
		}
	}
}

