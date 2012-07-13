using System;
using System.IO;
using System.Text;

namespace DRFCSharp
{
	public class Classification
	{
		private Label[,] _site_labels;
		public int XSites { get; private set; }
		public int YSites { get; private set; }
		public int NumOnSites {
			get{
				int result = 0;
				foreach(Label l in _site_labels)
					if(l == Label.ON)
						result++;
				return result;
			}
		}
		public int NumSites {
			get{
				return XSites * YSites;
			}
		}
		public Classification(Label[,] site_labels)
		{
			this._site_labels = site_labels;
			XSites = site_labels.GetLength(0);
			YSites = site_labels.GetLength(1);
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
			for(int y = 0; y < YSites; y++){
				for(int x = 0; x < XSites; x++){
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
		public static Classification FromLabeling(string filename, int x_sites, int y_sites)
		{
			Label[,] labels = new Label[x_sites, y_sites];
			using(StreamReader csvfile = new StreamReader(filename))
			{

				for(int col = 0; col < y_sites; col++)
				{
					string line = csvfile.ReadLine();
					string[] vals = line.Split(',');
					for(int row = 0; row < x_sites; row++)
					{
						int val = Int32.Parse(vals[row]);
						if(val > 0)
							labels[row,col] = Label.ON;
						else
							labels[row,col] = Label.OFF;
					}
				}
			}
			return new Classification(labels);
		}
	}
}

