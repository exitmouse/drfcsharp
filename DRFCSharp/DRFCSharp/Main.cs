using System;
using MathNet.Numerics.LinearAlgebra.Double;

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
			for(int i = 0; i < 16; i++)
			{
				for(int j = 0; j < 16; j++)
				{
					labels[i,j] = Label.ON;
				}
			}
			labels[0,0] = Label.OFF;
			img.site_features[0,0] = new SiteFeatureSet(new DenseVector(SiteFeatureSet.NUM_FEATURES,5d)); //Training _does not like it_ when the same features lead to different outcomes.
			//Our features have double precision though, so we'll be fine.
			Classification cfc = new Classification(labels);
			ModifiedModel mfm = ModifiedModel.PseudoLikelihoodTrain(new ImageData[1]{img},new Classification[1]{cfc},1d);
			Console.WriteLine(mfm.time_to_converge);
			Classification out_classed = mfm.MaximumAPosterioriInfer(img); //See what I did there?
			for(int i = 0; i < 16; i++)
				Console.WriteLine(out_classed[i,i]);
		}
	}
}
