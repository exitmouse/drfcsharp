using System;

namespace DRFCSharp
{
	public sealed class Window
	{
		private int _startx;
		private int _starty;
		private int _width;
		private int _height;

		public int StartX {
			get { return _startx; }
			private set { _startx = value; }
		}

		public int StartY {
			get { return _starty; }
			private set { _starty = value; }
		}

		/// <summary>
		/// Gets the ending x coordinate
		/// </summary>
		/// <value>
		/// The ending x coordinate, not inclusive. A window
		/// with StartX = 5 and Width = 2 has EndX = 7 but should
		/// consist of pixels 5 and 6.
		/// </value>
		public int EndX {
			get { return _startx + _width; }
			private set { _width = value - _startx; }
		}

		public int EndY {
			get { return _starty + _height; }
			private set { _height = value - _starty; }
		}

		public int Width {
			get { return _width; }
			private set { _width = value; }
		}

		public int Height {
			get { return _height; }
			private set { _height = value; }
		}

		public int Area {
			get { return _width * _height; }
		}

		private Window(int startx, int starty, int width, int height)
		{
			_startx = startx;
			_starty = starty;
			_width = width;
			_height = height;
		}

		public Window Constrain(int minx, int miny, int maxx, int maxy)
		{
			Window result = Window.FromBounds(Math.Max(StartX, minx), Math.Max(StartY, miny), Math.Min(EndX, maxx), Math.Min(EndY, maxy));
			if(result.Area <= 0) return null;
			else return result;
		}

		public static Window FromSize(int startx, int starty, int width, int height){
			return new Window(startx, starty, width, height);
		}

		public static Window FromBounds(int startx, int starty, int endx, int endy){
			return new Window(startx, starty, endx-startx, endy-starty);
		}

		public static Window FromCenter(int centx, int centy, int width, int height){
			return FromSize(centx - width/2, centy - height/2, width, height);
		}
	}
}

