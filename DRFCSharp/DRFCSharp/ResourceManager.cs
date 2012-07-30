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
            public string ImgPath { get; private set; }
            public string LabelPath { get; private set; }
            public Builder()
            {
                ImgPath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
                LabelPath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
            }
            public Builder SetImgPath(string val) { ImgPath = val; return this; }
            public Builder SetLabelPath(string val) { LabelPath = val; return this; }
            public ResourceManager Build(){
                return new ResourceManager(this);
            }
        }

		private ResourceManager(Builder builder)
		{
			ImgPath = builder.ImgPath;
			LabelPath = builder.LabelPath;
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

