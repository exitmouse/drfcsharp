using System;
using System.IO;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Double;
using NDesk.Options;

namespace DRFCSharp
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            ResourceManager.Builder resources_builder = new ResourceManager.Builder();
            //Set paths here
            ResourceManager resources = resources_builder.Build();

            ImageWindowScheme iws = new ImageWindowScheme(16, 16, 256, 256, 3);

            FeatureSet.Builder feature_set_builder = new FeatureSet.Builder();
            feature_set_builder.AddFeature(new AverageRGBFeature(iws));
            FeatureSet feature_set = feature_set_builder.Build();

            ImageData.Factory idf = new ImageData.Factory(iws.XSites, iws.YSites, feature_set);

            ModelFactory.Builder mfb = new ModelFactory.Builder();
            //Set hyperparameters here
            ModelFactory model_factory = mfb.Build();

            Console.WriteLine("Importing ImageDatas:");
            List<ImageData> imgs = resources.EachImage((Bitmap bmp) => {
                    return idf.FromImage(bmp);
                    });
            Console.WriteLine("Importing Classifications:");
            List<Classification> classifications = resources.EachCSV((StreamReader csv) => {
                    return Classification.FromLabeling(csv, iws.XSites, iws.YSites);
                    });

            mfm = model_factory.PseudoLikelihoodTrain(saved_model_path, write_model_path, imgs,cfcs);
            Console.WriteLine("Model converged! Estimating image ...");

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
