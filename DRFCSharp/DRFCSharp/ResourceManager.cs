using System;
using System.Drawing;

namespace DRFCSharp
{
	public sealed class ResourceManager
	{
		public string ImgPath { get; private set; }
		public string LabelPath { get; private set; }
		private static string feature_test_path = string.Format("{0}/../../../../TestImages/",AppDomain.CurrentDomain.BaseDirectory); //Added first forward slash to make it work on linux.

        public class Builder
        {		
            public string ImgPath { get; set; }
            public string LabelPath { get; set; }
            public Builder()
            {
                ImgPath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
                LabelPath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
            }
            public ResourceManager Build(){
                return new ResourceManager(ImgPath, LabelPath);
            }
        }

		public ResourceManager(string img_path, string label_path)
		{
			ImgPath = img_path;
			LabelPath = label_path;
		}
		public static T UsingTestingBitmap<T>(string name, Func<Bitmap,T> block)
		{
			using (Bitmap bmp = new Bitmap(feature_test_path + name))
			{
				return block(bmp);
			}
		}
		public T UsingBitmap<T>(string name, Func<Bitmap,T> block)
		{
			using (Bitmap bmp = new Bitmap(ImgPath + name))
			{
				return block(bmp);
			}
		}
	}
}

