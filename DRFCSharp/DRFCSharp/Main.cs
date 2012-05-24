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
			Console.WriteLine("Usage: DRFCSharp [-l <load_training_from_here>] [-s <save_training_here>] [-i <# of image to predict>]" +
				"\n If no image number is specified, defaults to 192 because I like that number.");
		}
		public static void Main (string[] args)
		{
			string params_in = "";
			string params_out = "";
			int image_num = 192;
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
			}
			if(image_num < 0)
			{
				PrintUsage();
				return;
			}
			ImageData[] imgs = new ImageData[80];
			Classification[] cfcs = new Classification[80];
			string imgpath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
			int count = 0;
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
			
			ModifiedModel mfm = ModifiedModel.PseudoLikelihoodTrain(params_in, params_out, imgs,cfcs,0.0001d);
			Console.WriteLine("Model converged! Estimating image ...");
			string imagename = "RandCropRotate"+image_num.ToString("D3");
			ImageData input = ImageData.FromImage(new Bitmap(imgpath+imagename+".jpg"));
			
			Classification out_classed = mfm.MaximumAPosterioriInfer(input); //See what I did there?
			
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
