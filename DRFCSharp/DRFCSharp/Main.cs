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
			Console.WriteLine("DRFCSharp: Trains a DRF model on a set of images, and predicts" +
				"\n the location of man-made structures in a test image. The classification is" +
				"\n printed to a csv file." +
				"\n Usage: DRFCSharp [logistic | ICM | MAP] outfile" +
				"\n Options:" +
				"\n -n/--notraining: Skips training step" +
				"\n -l/--load <load_training_from_here>: Loads training from a file" +
				"\n -s/--save <save_training_here>: Saves training to a file" +
				"\n -i/--image <# of image to predict>: Infers on this image number" +
				"\n -r/--range <# of images to train on>" +
				"\n --imgdir <path>" +
				"\n --labeldir <path>" +
				"\n --testdir <path>" +
				"\n If no image number is specified, defaults to 192 because I like that number." +
				"\n -t/--tau <double>: Controls the variance of the gaussian hyperparameter on v." +
				"\n Defaults to 0.0001.");
		}
		public static void Main (string[] args)
		{
			string params_in = "";
			string params_out = "";
			string imgpath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
			string labelpath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
			string testpath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
			string outpath = "";
			bool deserialize_only = false;
			int image_num = 192;
			double tau = 0.0001d;
			int range = 80;
			
			if(args.Length < 1)
			{
				PrintUsage();
				return;
			}
			string inference_algorithm = args[0].ToLower();
			if(inference_algorithm != "logistic" && inference_algorithm != "map" && inference_algorithm != "icm")
			{
				PrintUsage();
				return;
			}
				
			for(int i = 1; i < args.Length; i++)
			{
				if(args[i] == "-l" || args[i] == "--loadtraining")
				{
					params_in = args[i+1];
					i++;
				}
				else if(args[i] == "-s" || args[i] == "--savetraining")
				{
					params_out = args[i+1];
					i++;
				}
				else if(args[i] == "-i" || args[i] == "--image")
				{
					if(!Int32.TryParse(args[i+1], out image_num) || image_num < 0)
					{
						PrintUsage();
						return;
					}
					i++;
				}
				else if(args[i] == "-r" || args[i] == "--range")
				{
					if(!Int32.TryParse(args[i+1], out range) || range < 1)
					{
						PrintUsage();
						return;
					}
					i++;
				}
				else if(args[i] == "-t" || args[i] == "--tau")
				{
					if(!double.TryParse(args[i+1], out tau))
					{
						PrintUsage();
						return;
					}
					i++;
				}
				else if(args[i] == "-n" || args[i] == "--notraining")
				{
					deserialize_only = true;
				}
				else if(args[i] == "--imgdir")
				{
					imgpath = args[i+1];
					i++;
				}
				else if(args[i] == "--labeldir")
				{
					labelpath = args[i+1];
					i++;
				}
				else if(args[i] == "--testdir")
				{
					testpath = args[i+1];
					i++;
				}
				else
				{
					outpath = args[i];
					i++;
				}
			}
			if(deserialize_only && string.IsNullOrEmpty(params_in))
			{
				PrintUsage();
				return;
			}
			ImageData[] imgs = new ImageData[range];
			Classification[] cfcs = new Classification[range];
			int count = 0;
			
			ModifiedModel mfm;
			if(deserialize_only)
			{
				mfm = ModifiedModel.Deserialize(params_in);
			}
			else
			{
				for(int k = 0; k < range; k++)
				{
					Console.WriteLine ("Importing "+k.ToString()+"th image");
					string prefix = k.ToString("D3");
					ImageData img = ImageData.FromImage(new Bitmap(imgpath+prefix+".jpg"));
					//Console.WriteLine (img[0,2].features[2]);
					Console.WriteLine(labelpath+prefix+".txt");
					Classification cfc = ImageData.ImportLabeling(labelpath+prefix+".txt");
					imgs[count] = img;
					cfcs[count] = cfc;
					count++;
				}
				
				mfm = ModifiedModel.PseudoLikelihoodTrain(params_in, params_out, imgs,cfcs,tau);
				Console.WriteLine("Model converged! Estimating image ...");
			}
			string imagename = image_num.ToString("D3");
			ImageData input = ImageData.FromImage(new Bitmap(testpath+imagename+".jpg"));
			
			Classification out_classed; //See what I did there?
			if(inference_algorithm == "logistic")
			{
				Console.WriteLine("Inferring with Logistic classifier...");
				out_classed = mfm.LogisticInfer(input);
			}
			else if (inference_algorithm == "map")
			{
				Console.WriteLine("Inferring with MAP classifier...");
				out_classed = mfm.MaximumAPosterioriInfer(input);
			}
			else
			{
				Console.WriteLine("Inferring with ICM classifier...");
				out_classed = mfm.ICMInfer(input);
			}
			
			if(string.IsNullOrEmpty(outpath))
			{
				outpath = imgpath+"predicted"+image_num.ToString("D3")+".txt";
			}
			
			StreamWriter sw = new StreamWriter(outpath);
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
