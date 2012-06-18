using System;
using System.Text;

namespace DRFCSharp
{
	public class Classification
	{
		public Label[,] _site_labels;
		public Classification(Label[,] site_labels)
		{
			if(site_labels.GetLength(0) != ImageData.x_sites || site_labels.GetLength(1) != ImageData.y_sites)
				throw new ArgumentException("Wrong size of labeling for this model");
			this._site_labels = site_labels;
		}
		public Label this[int x,int y]{
			get{
				return this._site_labels[x,y];
			}
			set{
				_site_labels[x,y]=value;
			}
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for(int y = 0; y < ImageData.y_sites; y++){
				for(int x = 0; x < ImageData.x_sites; x++){
					if(this[x,y]==Label.OFF)
						sb.Append("0");
					if(this[x,y]==Label.ON)
						sb.Append("1");
					sb.Append(",");
				}
				sb.Append("\n");
			}
			return sb.ToString();
		}
	}
}

