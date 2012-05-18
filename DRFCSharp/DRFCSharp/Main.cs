using System;
using System.IO;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//Hacky command line arg parsing:
			string params_in = args[0];
			string params_out = args[1];
			int image_num;
			Int32.TryParse(args[2], out image_num);
			string prediction_out = args[3];
			ImageData[] imgs = new ImageData[80];
			Classification[] cfcs = new Classification[80];
			string imgpath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
			int count = 0;
			for(int dig1 = 0; dig1 < 1; dig1++) for(int dig2 = 0; dig2 < 8; dig2++) for(int dig3 = 0; dig3 < 10; dig3++)
			{
				Console.WriteLine ("Importing "+dig1.ToString()+dig2.ToString()+dig3.ToString()+"th image");
				string prefix = dig1.ToString()+dig2.ToString()+dig3.ToString();
				ImageData img = ImageData.FromImage(new Bitmap(imgpath+"RandCropRotate"+prefix+".jpg"));
				//Console.WriteLine (img[0,2].features[2]);
				Classification cfc = ImageData.ImportLabeling(imgpath+prefix+".txt");
				imgs[count] = img;
				cfcs[count] = cfc;
				count++;
			}
			
			ModifiedModel mfm = ModifiedModel.PseudoLikelihoodTrain(params_in, params_out, imgs,cfcs,0.0001d);
			Console.WriteLine("Model converged! Estimating image ...");
			
			ImageData input = ImageData.FromImage(new Bitmap(imgpath+"RandCropRotate"+image_num.ToString()+".jpg"));
			
			Classification out_classed = mfm.MaximumAPosterioriInfer(input); //See what I did there?
			
			StreamWriter sw = new StreamWriter(imgpath+prediction_out);
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
