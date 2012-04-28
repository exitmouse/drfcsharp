using System;

namespace DRFCSharp
{
	public class Classification
	{
		public Label[,] site_labels;
		
		public Classification(Label[,] site_labels)
		{
			if(site_labels.GetLength(0) != ImageData.x_sites || site_labels.GetLength(1) != ImageData.y_sites)
				throw new ArgumentException("Wrong size of labeling for this model");
			this.site_labels = site_labels;
		}
		
		public Label this[int value1,int value2]{
			get{
				return this.site_labels[value1,value2];
			}
		}
	}
}

