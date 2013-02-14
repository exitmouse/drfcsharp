using System;

namespace DRFCSharp
{
	//TODO make this take a scale parameter or something.
        /// <summary>
        /// An ImageWindowScheme is a type of FeatureApplicationScheme
        /// particularly suited to dividing an image into windows. It keeps
        /// track of the pixel boundaries for each site index.
        /// </summary>
	public class ImageWindowScheme : FeatureApplicationScheme
	{
		public int WindowWidth { get; private set; }
		public int WindowHeight { get; private set; }
		public int ImageWidth { get; private set; }
		public int ImageHeight { get; private set; }
		public int NumScales { get; private set; }
		public int XSites {
			get{
				return ((ImageWidth-1)/WindowWidth)+1; //TODO Unit test this
			}
		}
		public int YSites {
			get{
				return ((ImageHeight-1)/WindowHeight)+1;
			}
		}

		public ImageWindowScheme(int w, int h, int iw, int ih, int num_scales)
		{
			WindowWidth = w;
			WindowHeight = h;
			ImageWidth = iw;
			ImageHeight = ih;
			NumScales = num_scales;
		}

		public Window this[int x, int y, int scale]
		{
			get{ 
				if(WindowWidth * x >= ImageWidth || WindowHeight * y >= ImageHeight || scale >= NumScales || x < 0 || y < 0 || scale < 0) throw new System.ArgumentOutOfRangeException();
				int scalepow = 1 << scale;
				Window indexed = Window.FromCenter(x*WindowWidth + WindowWidth/2, y*WindowHeight + WindowHeight/2, scalepow*WindowWidth, scalepow*WindowHeight);
				return indexed.Constrain(0, 0, ImageWidth, ImageHeight);
			}
		}
	}
}

