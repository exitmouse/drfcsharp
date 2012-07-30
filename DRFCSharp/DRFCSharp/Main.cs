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
				"\n Usage: DRFCSharp [logistic | ICM | MAP | features <filename>] outfile" +
				"\n Options:" +
				"\n --xsites, --ysites: Set the number of x and y sites in the images. Defaults to 24 and 16 respectively" +
				"\n -n/--notraining: Skips training step" +
				"\n -l/--load <load_training_from_here>: Loads training from a file" +
				"\n -s/--save <save_training_here>: Saves training to a file" +
				"\n -i/--image <# of image to predict>: Infers on this image number" +
			    "\n -ei/--endimage <# of last image to predict>: Infers on everything between -i and this" +
			    "\n Will not let you have control of file naming, though" +
				"\n -r/--range <# of images to train on>" +
				"\n --imgdir <path>" +
				"\n --labeldir <path>" +
				"\n If no image number is specified, defaults to 192 because I like that number." +
				"\n -t/--tau <double>: Controls the variance of the gaussian hyperparameter on v." +
				"\n Defaults to 0.0001." +
			    "\n If the mode is specified as features <filename>: Only calculates features for filename" +
			    "\n and prints them to outfile.");
		}
		public static void Main (string[] args)
		{
			string saved_model_path = "";
			string write_model_path = "";
			string test_image_name = "";
			ResourceManager.Builder resources_builder = new ResourceManager.Builder();
            ModelFactory.Builder mfb = new ModelFactory.Builder();
			string outpath = "";
			bool deserialize_only = false;
			int image_num = 192;
			int end_image_num = -1; // infer between image_num and this inclusive, -1 indicates a single image rather than a range
			int range = 80;
			int xsites = 24;
			int ysites = 16;
			
			if(args.Length < 1)
			{
				PrintUsage();
				return;
			}
			string inference_algorithm = args[0].ToLower();
			if(inference_algorithm != "logistic" && inference_algorithm != "map" && inference_algorithm != "icm" && inference_algorithm != "features")
			{
				PrintUsage();
				return;
			}

			for(int i = 1; i < args.Length; i++)
			{
				if(inference_algorithm == "features" && i == 1)
				{
					test_image_name = args[i];
				}
				else if(args[i] == "-l" || args[i] == "--loadtraining")
				{
					saved_model_path = args[i+1];
					i++;
				}
				else if(args[i] == "-s" || args[i] == "--savetraining")
				{
					write_model_path = args[i+1];
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
				else if(args[i] == "-ei" || args[i] == "--endimage")
				{
					if(!Int32.TryParse(args[i+1], out end_image_num) || end_image_num < image_num)
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
				else if(args[i] == "-x" || args[i] == "--xsites")
				{
					if(!Int32.TryParse(args[i+1], out xsites) || xsites < 1)
					{
						PrintUsage();
						return;
					}
					i++;
				}
				else if(args[i] == "-y" || args[i] == "--ysites")
				{
					if(!Int32.TryParse(args[i+1], out ysites) || ysites < 1)
					{
						PrintUsage();
						return;
					}
					i++;
				}
				else if(args[i] == "-t" || args[i] == "--tau")
				{
                    double tau;
					if(!double.TryParse(args[i+1], out tau))
					{
						PrintUsage();
						return;
					}
                    mfb.SetTau(tau);
					i++;
				}
				else if(args[i] == "-n" || args[i] == "--notraining")
				{
					deserialize_only = true;
				}
				else if(args[i] == "--imgdir")
				{
					resources_builder.SetImgPath(args[i+1]);
					i++;
				}
				else if(args[i] == "--labeldir")
				{
					resources_builder.SetLabelPath(args[i+1]);
					i++;
				}
				else
				{
					outpath = args[i];
				}
			}
			if(deserialize_only && string.IsNullOrEmpty(saved_model_path))
			{
				PrintUsage();
				return;
			}
			/*ImageData.x_sites = xsites;
			ImageData.y_sites = ysites;
			Console.WriteLine("{0} X Sites", ImageData.x_sites);
			Console.WriteLine("{0} Y Sites", ImageData.y_sites);*/
            FeatureSet feature_set = new FeatureSet.Builder().Build();
            ImageData.Factory idf = new ImageData.Factory(xsites, ysites, feature_set);
			
			if(inference_algorithm == "features")
			{
				ImageData test_img = ResourceManager.UsingTestingBitmap(test_image_name, thingy => idf.FromImage(thingy));
				StreamWriter sw = new StreamWriter(outpath);
                sw.WriteLine("Features at each site:");
				for(int x = 0; x < test_img.XSites; x++)
				{
                    sw.WriteLine("    X: {0}", x);
					for(int y = 0; y < test_img.YSites; y++)
					{
						sw.WriteLine("        Y: {0}\n            {1}", y, test_img[x,y].ToString());
					}
				}
				sw.Close();
				return;
			}
			
			
			ImageData[] imgs = new ImageData[range];
			Classification[] cfcs = new Classification[range];
			int count = 0;
			ResourceManager resources = resources_builder.Build();
			Model mfm;
			if(deserialize_only)
			{
				mfm = ModelFactory.Deserialize(saved_model_path);
			}
			else
			{
				for(int k = 0; k < range; k++)
				{
					Console.WriteLine ("Importing "+k.ToString()+"th image");
					string prefix = k.ToString("D3");
					string suffix = prefix +".jpg";
					ImageData img = resources.UsingBitmap(suffix, bmp => idf.FromImage(bmp));
					//Console.WriteLine (img[0,2].features[2]);
					Classification cfc = Classification.FromLabeling(resources_builder.LabelPath+prefix+".txt", img.XSites, img.YSites);
					imgs[count] = img;
					cfcs[count] = cfc;
					//if (k == 5) Console.WriteLine (SiteFeatureSet.TransformedFeatureVector(img[0,2]));
					count++;
				}
				ModelFactory model_factory = mfb.Build();
				mfm = model_factory.PseudoLikelihoodTrain(saved_model_path, write_model_path, imgs,cfcs);
				Console.WriteLine("Model converged! Estimating image ...");
			}
			
			end_image_num = Math.Max(end_image_num, image_num);
			for (int i = image_num; i <= end_image_num; i++)
			{
				string imagename = i.ToString("D3");
				ImageData input = resources.UsingBitmap(imagename+".jpg", bmp => idf.FromImage(bmp));
				
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
				
				outpath = resources_builder.ImgPath+"predicted"+i.ToString("D3")+".txt";
				
				StreamWriter sw = new StreamWriter(outpath);
				sw.Write(out_classed.ToString());
				sw.Close();
			}
		}
	}
}
