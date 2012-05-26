using System;
using System.IO;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	class MainClass
	{
		public static void PrintUsage()
		{
			Console.WriteLine("Usage: DRFCSharp" +
				"\n Options:" +
				"\n -n/--notraining: If you want to just load a previous model without training a new one" +
				"\n -l/--load <load_training_from_here>: Loads training from a serialized previous model" +
				"\n -s/--save <save_training_here>: Saves training to the filename specified" +
				"\n -i/--image <# of image to predict>: Uses the model to infer on this image number" +
				"\n If no image number is specified, defaults to 192 because I like that number." +
				"\n -t/--tau <double>: Controls the variance of the gaussian hyperparameter on v. Defaults to 0.001.");
		}
		public static void Main (string[] args)
		{
			string params_in = "";
			string params_out = "";
			bool deserialize_only = false;
			int image_num = 192;
			double tau = 0.001d;
			for(int i = 0; i < args.Length; i++)
			{
				if(args[i] == "-l" || args[i] == "--loadtraining")
				{
					params_in = args[i+1];
					i++;
				}
				if(args[i] == "-s" || args[i] == "--savetraining")
				{
					params_out = args[i+1];
					i++;
				}
				if(args[i] == "-i" || args[i] == "--image")
				{
					if(!Int32.TryParse(args[i+1], out image_num))
					{
						PrintUsage();
						return;
					}
					i++;
				}
				if(args[i] == "-t" || args[i] == "--tau")
				{
					if(!double.TryParse(args[i+1], out tau))
					{
						PrintUsage();
						return;
					}
					i++;
				}
				if(args[i] == "-n" || args[i] == "--notraining")
				{
					deserialize_only = true;
				}
			}
			if(image_num < 0 || (deserialize_only && string.IsNullOrEmpty(params_in)))
			{
				PrintUsage();
				return;
			}
			ImageData[] imgs = new ImageData[80];
			Classification[] cfcs = new Classification[80];
			string imgpath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
			int count = 0;
			
			ModifiedModel mfm;
			if(deserialize_only)
			{
				mfm = ModifiedModel.Deserialize(params_in);
			}
			for(int k = 0; k < 80; k++)
			{
				Console.WriteLine ("Importing "+k.ToString()+"th image");
				string prefix = k.ToString("D3");
				ImageData img = ImageData.FromImage(new Bitmap(imgpath+"RandCropRotate"+prefix+".jpg"));
				//Console.WriteLine (img[0,2].features[2]);
				Classification cfc = ImageData.ImportLabeling(imgpath+prefix+".txt");
				imgs[count] = img;
				cfcs[count] = cfc;
				count++;
			}
			
			mfm = ModifiedModel.PseudoLikelihoodTrain(params_in, params_out, imgs,cfcs,tau);
			Console.WriteLine("Model converged! Estimating image ...");
			string imagename = "RandCropRotate"+image_num.ToString("D3");
			ImageData input = ImageData.FromImage(new Bitmap(imgpath+imagename+".jpg"));
			
			Classification out_classed = mfm.LogisticInfer(input); //See what I did there?
			
			StreamWriter sw = new StreamWriter(imgpath+"predicted"+image_num.ToString("D3")+".txt");
			for(int i = 0; i < 16; i++)
			{
				for(int j = 0; j < 16; j++)
				{
					if(out_classed[j,i] == Label.OFF)
					{
						sw.Write('0');
					}
					else
					{
						sw.Write('1');
					}
					sw.Write(',');
				}
				sw.Write('\n');
			}
			sw.Close();
		}
	}
}
