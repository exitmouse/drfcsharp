using System;

namespace DRFCSharp
{
	public class ResourceManagerBuilder
	{		
		public string ImgPath { get; set; }
		public string LabelPath { get; set; }
		public ResourceManagerBuilder()
		{
			ImgPath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
			LabelPath = string.Format("{0}../../../../Dataset/",AppDomain.CurrentDomain.BaseDirectory);
		}
		public ResourceManager Build(){
			return new ResourceManager(ImgPath, LabelPath);
		}
	}
}

