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
		public string TrainingCSVPath { get; private set; }
        public string TestCSVPath { get; private set; }
        public string OutputCSVPath { get; private set; }
		private static string feature_debug_path = string.Format("{0}/../../../../TestImages/",AppDomain.CurrentDomain.BaseDirectory); //Added first forward slash to make it work on linux.

        public class Builder
        {		
            public string TrainingImgPath { get; private set; }
			public string TestImgPath { get; private set; }
            public string TrainingCSVPath { get; private set; }
            public string TestCSVPath { get; private set; }
            public string OutputCSVPath { get; private set; }
            public Builder()
            {
                TrainingImgPath = string.Format("{0}../../../../Dataset/TrainingImages/",AppDomain.CurrentDomain.BaseDirectory);
				TestImgPath = string.Format ("{0}../../../../Dataset/TestImages/",AppDomain.CurrentDomain.BaseDirectory);
                TrainingCSVPath = string.Format("{0}../../../../Dataset/TrainingCSVs/",AppDomain.CurrentDomain.BaseDirectory);
                TestCSVPath = string.Format("{0}../../../../Dataset/TestCSVs/",AppDomain.CurrentDomain.BaseDirectory);
                OutputCSVPath = string.Format("{0}../../../../Dataset/OutputCSVs/",AppDomain.CurrentDomain.BaseDirectory);
            }
            public Builder SetTrainingImgPath(string val) { TrainingImgPath = val; return this; }
			public Builder SetTestImgPath(string val) { TestImgPath = val; return this; }
            public Builder SetTrainingCSVPath(string val) { TrainingCSVPath = val; return this; }
            public Builder SetTestCSVPath(string val) { TestCSVPath = val; return this; }
            public Builder SetOutputCSVPath(string val) { OutputCSVPath = val; return this; }
            public ResourceManager Build(){
                return new ResourceManager(this);
            }
        }

		private ResourceManager(Builder builder)
		{
			TrainingImgPath = builder.TrainingImgPath;
			TestImgPath = builder.TestImgPath;
			TrainingCSVPath = builder.TrainingCSVPath;
            TestCSVPath = builder.TestCSVPath;
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

        public List<T> EachTrainingCSV<T>(Func<StreamReader,T> block)
        {
            List<T> result = new List<T>();
            foreach(string path in Directory.GetFiles(TrainingCSVPath))
            {
                Console.WriteLine(path);
                using (StreamReader csvfile = new StreamReader(path))
                {
                    result.Add(block(csvfile));
                }
            }
            return result;
        }

        public List<T> EachTestCSV<T>(Func<StreamReader,T> block)
        {
            List<T> result = new List<T>();
            foreach(string path in Directory.GetFiles(TestCSVPath))
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

