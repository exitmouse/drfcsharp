using System;

namespace DRFCSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			SiteFeatureSet[,] sitesarray = new SiteFeatureSet[ImageData.x_sites,ImageData.y_sites];
			SiteFeatureSet.Init(sitesarray);
			ImageData img = new ImageData(sitesarray);
			Label[,] labels = new Label[ImageData.x_sites,ImageData.y_sites];
			Classification cfc = new Classification(labels);
			ModifiedModel mfm = ModifiedModel.PseudoLikelihoodTrain(new ImageData[1]{img},new Classification[1]{cfc},1d);
			Console.WriteLine(mfm.time_to_converge);
		}
	}
}
