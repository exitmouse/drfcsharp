using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace DRFCSharp
{
	public sealed class ResourceManager
	{
		public string TrainingImgPath { get; private set; }
		public string TestImgPath { get; private set; }
		public string CSVPath { get; private set; }
        public string OutputCSVPath { get; private set; }
		private static string feature_debug_path = string.Format("{0}/../../../../TestImages/",AppDomain.CurrentDomain.BaseDirectory); //Added first forward slash to make it work on linux.

        public class Builder
        {		
            public string TrainingImgPath { get; private set; }
			public string TestImgPath { get; private set; }
            public string CSVPath { get; private set; }
            public string OutputCSVPath { get; private set; }
            public Builder()
            {
                TrainingImgPath = string.Format("{0}../../../../Dataset/TrainingImages/",AppDomain.CurrentDomain.BaseDirectory);
				TestImgPath = string.Format ("{0}../../../../Dataset/TestImages/",AppDomain.CurrentDomain.BaseDirectory);
                CSVPath = string.Format("{0}../../../../Dataset/CSVs/",AppDomain.CurrentDomain.BaseDirectory);
                OutputCSVPath = string.Format("{0}../../../../Dataset/OutputCSVs/",AppDomain.CurrentDomain.BaseDirectory);
            }
            public Builder SetTrainingImgPath(string val) { TrainingImgPath = val; return this; }
			public Builder SetTestImgPath(string val) { TestImgPath = val; return this; }
            public Builder SetCSVPath(string val) { CSVPath = val; return this; }
            public Builder SetOutputCSVPath(string val) { OutputCSVPath = val; return this; }
            public ResourceManager Build(){
                return new ResourceManager(this);
            }
        }

		private ResourceManager(Builder builder)
		{
			TrainingImgPath = builder.TrainingImgPath;
			TestImgPath = builder.TestImgPath;
			CSVPath = builder.CSVPath;
            OutputCSVPath = builder.OutputCSVPath;
		}

		public static T UsingDebugBitmap<T>(string name, Func<Bitmap,T> block)
		{
			using (Bitmap bmp = new Bitmap(feature_debug_path + name))
			{
				return block(bmp);
			}
		}
		public T UsingTrainingBitmap<T>(string name, Func<Bitmap,T> block)
		{
			using (Bitmap bmp = new Bitmap(TrainingImgPath + name))
			{
				return block(bmp);
			}
		}
        
        public void UsingOutputCSV(string name, Action<StreamWriter> block)
        {
            using (StreamWriter sw = new StreamWriter(OutputCSVPath + name))
            {
                block(sw);
            }
        }
        
        public List<T> EachTrainingImage<T>(Func<Bitmap,T> block)
        {
            List<T> result = new List<T>();
            foreach(string path in Directory.GetFiles(TrainingImgPath))
            {
                Console.WriteLine(path);
                using (Bitmap bmp = new Bitmap(path))
                {
                    result.Add(block(bmp));
                }
            }
            return result;
        }
		
		public List<T> EachTestImage<T>(Func<Bitmap,T> block)
		{
			List<T> result = new List<T>();
			foreach(string path in Directory.GetFiles(TestImgPath))
			{
                Console.WriteLine(path);
				using (Bitmap bmp = new Bitmap(path))
				{
					result.Add(block(bmp));
				}
			}
			return result;
		}

        public List<T> EachCSV<T>(Func<StreamReader,T> block)
        {
            List<T> result = new List<T>();
            foreach(string path in Directory.GetFiles(CSVPath))
            {
                Console.WriteLine(path);
                using (StreamReader csvfile = new StreamReader(path))
                {
                    result.Add(block(csvfile));
                }
            }
            return result;
        }

	}
}

