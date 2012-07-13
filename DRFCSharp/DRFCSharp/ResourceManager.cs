using System;
using System.Drawing;

namespace DRFCSharp
{
	public sealed class ResourceManager
	{
		private string img_path = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
		private string label_path = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
		public string ImgPath {
			get{
				return img_path;
			}
			set{
				img_path = value;
			}
		}
		public string LabelPath {
			get{
				return label_path;
			}
			set{
				label_path = value;
			}
		}
		private static string feature_test_path = string.Format("{0}../../../../TestImages/",AppDomain.CurrentDomain.BaseDirectory);
		public ResourceManager()
		{
			img_path = "";
			label_path = "";
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
			using (Bitmap bmp = new Bitmap(img_path + name))
			{
				return block(bmp);
			}
		}
	}
}

