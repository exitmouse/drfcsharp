using System;
using System.Drawing;

namespace DRFCSharp
{
	public sealed class ResourceManager
	{
		private static string img_path = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
		private static string label_path = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
		private static string feature_test_path = string.Format("{0}../../../../TestImages/",AppDomain.CurrentDomain.BaseDirectory);
		private ResourceManager()
		{
		}
		public static T UsingTestingBitmap<T>(string name, Func<Bitmap,T> block)
		{
			using (Bitmap bmp = new Bitmap(feature_test_path + name))
			{
				return block(bmp);
			}
		}
	}
}

