using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ImageData
	{
		public const int x_sites = 24; //Make sure these divide the image dimensions. The size of the sites is deduced from them.
		public const int y_sites = 16;
		public const double variation = 0.5d; //Make sure 6*variation is odd.
		public const int NUM_ORIENTATIONS = 16;
		public SiteFeatureSet[,] site_features;
		public static int Ons_seen = 0;
		public static int Sites_seen = 0;
		public static int Images_seen = 0;
		public ImageData (SiteFeatureSet[,] site_features)
		{
			if(site_features.GetLength(0) != x_sites || site_features.GetLength(1) != y_sites)
				throw new ArgumentException("Wrong size of test input for this model");
			this.site_features = site_features;
		}
		
		public SiteFeatureSet this[int value1,int value2]{
			get{
				return this.site_features[value1,value2];
			}
		}
		public static List<Tuple<int,int>> GetNeighbors(int x, int y)
		{
			List<Tuple<int,int>> toReturn = new List<Tuple<int, int>>();
			for(int horz = -1; horz <=1; horz++)
				for(int vert = -1; vert <=1; vert++)
					if(Math.Abs(horz+vert)==1) //Hacky way of ensuring precisely one of the components is nonzero.
						if(InBounds(x+horz,y+vert))
							toReturn.Add(Tuple.Create<int,int>(x+horz,y+vert));
			return toReturn;
		}
		public static bool InBounds(int x, int y)
		{
			if(x >= 0 && x < x_sites && y >= 0 && y < y_sites)
				return true;
			return false;
		}
		private static bool IsEarlier(int x1, int y1, int x2, int y2)
		{
			if(y1 == y2) return x1 < x2;
			else return y1 < y2;
		}
		public static List<Tuple<int,int>> GetNewConnections(int x, int y)
		{
			List<Tuple<int,int>> toReturn = GetNeighbors(x,y);
			toReturn.RemoveAll((t) => IsEarlier(t.Item1,t.Item2,x,y));
			return toReturn;
		}
		public static ImageData FromImage(Bitmap img)
		{
			DenseVector[,] grads = SmoothedGradientArray(img);
			SiteFeatureSet[,] sitefeatures = new SiteFeatureSet[x_sites,y_sites];
			int width_of_site = img.Width/x_sites;
			int height_of_site = img.Height/y_sites;
			for(int x = 0; x < x_sites; x++) for(int y = 0; y < y_sites; y++)
			{
				//Console.WriteLine("X = {0}, Y = {1}",x,y);
				DenseVector single_site_features = new DenseVector(SiteFeatureSet.NUM_FEATURES);
				
				for(int scalepow = 0; scalepow < 3; scalepow++) //TODO maybe less hardcode?
				{
					double[] histogram_over_orientations = new double[NUM_ORIENTATIONS]; //TODO maybe refactor into a function
					int scale = 16;
					for(int useless = 0; useless < scalepow; useless++) scale *= 2; //HACK this is just bad code and I feel bad now.
					
					int num_pixels_at_scale = 0;
					for(int u = (x*width_of_site+width_of_site/2)-scale/2; u < (x*width_of_site+width_of_site/2) + scale/2; u++)
					{
						for(int v = (y*height_of_site+height_of_site/2)-scale/2; v < (y*height_of_site+height_of_site/2) + scale/2; v++)
						{
							if(u >= img.Width || v >= img.Height || u < 0 || v < 0) continue;
							num_pixels_at_scale++;
							DenseVector g = grads[u,v];
							//PREPARE FOR HACK
							double angle = Math.Atan2 (g[1],g[0]);
							if(angle < 0) angle += 2*Math.PI;
							int orientation = (int)Math.Floor((((double)NUM_ORIENTATIONS)/(2*Math.PI))*angle);
							orientation = orientation % NUM_ORIENTATIONS; //Hack moar--mitigated now. Still bad.
							double magnitude = g.Norm(2);
							histogram_over_orientations[orientation] += magnitude;
						}
					}
					double[] smoothed_histogram = new double[NUM_ORIENTATIONS];
					for(int i = 0; i < NUM_ORIENTATIONS; i++)
					{
						double numerator = 0;
						double denom = 0;
						for(int j = 0; j < NUM_ORIENTATIONS; j++)
						{
							//Many things are wrong with this. It seems this should work _badly_.
							//Possibly I am misunderstanding the paper.
							double coeff = SmoothingKernel(((double)(i-j))/2d);
							denom += coeff;
							numerator += coeff*histogram_over_orientations[j];
						}
						smoothed_histogram[i] = numerator/denom;
					}
					for(int i = 0; i < NUM_ORIENTATIONS; i++) smoothed_histogram[i] *= 4096d / ((double)num_pixels_at_scale);
					/*//TODO Decide whether we want this normalization. Added it because of edges not getting as many data points.
					double sum = 0;
					for(int i = 0; i < NUM_ORIENTATIONS; i++) sum += smoothed_histogram[i];
					for(int i = 0; i < NUM_ORIENTATIONS; i++) smoothed_histogram[i] /= sum;*/
					
					
					//Page 20 of paper says that the single-site features were the first three moments and two orientation-based intrascale features.
					//However, we can't use the absolute location of the orientation because our images are distributed in a way that's rotationally
					//invariant. Our images are not taken with upright cameras.
					for(int i = 0; i < 3; i++)
					{
						single_site_features[scalepow*4 + i]=Moment(smoothed_histogram,i);
						if(double.IsNaN(single_site_features[i]))
						{
							throw new NotImplementedException();
						}
					}
					single_site_features[scalepow*4 + 3] = RightAngleFinder(smoothed_histogram);
					//double[] avgs = AverageRGB(img, x, y);
					//for(int i = 0; i < 3; i++) single_site_features[scalepow*7+4+i] = avgs[i];
				}
				//Console.WriteLine(single_site_features);
				sitefeatures[x,y] = new SiteFeatureSet(single_site_features);
			}
			return new ImageData(sitefeatures);
		}
		public static double[] AverageRGB(Bitmap img, int sitex, int sitey)
		{
			double[] colors = new double[3];
			int count = 0;
			int width_of_site = img.Width/ImageData.x_sites;
			int height_of_site = img.Height/ImageData.y_sites;
			for(int x = sitex*width_of_site; x < (sitex+1)*width_of_site; x++) for(int y = sitey*height_of_site; y < (sitey+1)*height_of_site; y++)
			{
				Color pixval = img.GetPixel(x,y);
				colors[0] += pixval.R;
				colors[1] += pixval.G;
				colors[2] += pixval.B;
				
				count++;
			}
			double multfactor = 1/((double)count);
			for(int i = 0; i < 3; i++) colors[i] *= multfactor;
			return colors;
		}
		public static double RightAngleFinder(double[] histogram)
		{
			int maxindex = 0;
			int secondbestindex = -1;
			for(int i = 0; i < histogram.Length; i++)
			{
				if(maxindex == 0)
				{
					maxindex = i;
				}
				else if(histogram[i] >= histogram[maxindex])
				{
					secondbestindex = maxindex;
					maxindex = i;
				}
				else if(secondbestindex == -1 || histogram[i] > histogram[secondbestindex])
				{
					secondbestindex = i;
				}
			}
			if(maxindex == secondbestindex || secondbestindex == -1)
			{
				//My algorithm sucks
				throw new NotImplementedException();
			}
			double ang1 = (2*Math.PI/((double)NUM_ORIENTATIONS))*((double)maxindex);
			double ang2 = (2*Math.PI/((double)NUM_ORIENTATIONS))*((double)secondbestindex);
			double ang = ang1-ang2;
			double interim = Math.Sin(ang);
			double toReturn = Math.Abs (interim);
			if(toReturn < 0.0001d) toReturn += 0.001d; //Not sure if this will help. Hack; we only use the sine function to give better falloff anyway so I don't feel too guilty. But still.
			return toReturn;
		}
		public static double Moment(double[] histogram, int p)
		{
			if(p == 0)
			{
				double sum = 0;
				for(int i = 0; i < histogram.Length; i++)
				{
					sum += histogram[i];
				}
				return sum/((double)histogram.Length);
			}
			else
			{
				double v_0 = Moment(histogram, 0);
				double numerator = 0;
				double denom = 0;
				for(int i = 0; i < histogram.Length; i++)
				{
					if(histogram[i] <= v_0) continue;
					else
					{
						numerator += Math.Pow((histogram[i]-v_0),p+1);
						denom += histogram[i]-v_0;
					}
				}
				return numerator/denom;
			}
		}
		public static double SmoothingKernel(double argument)
		{
			//Using triangular kernel.
			if(Math.Abs(argument) > 1d) return 0;
			return (1-Math.Abs(argument));
		}
		/// <summary>
		/// Returns a smoothed gradient array, indexed so that SmoothedGradientArray(img)[x,y] is
		/// the gradient at img.GetPixel(x,y).
		/// </summary>
		/// <returns>
		/// The smoothed gradient array.
		/// </returns>
		/// <param name='img'>
		/// A bitmap image.
		/// </param>
		public static DenseVector[,] SmoothedGradientArray(Bitmap img)
		{
			DenseVector one_d_gaussian = MakeGaussian();
			DenseVector one_d_gaussian_derivative = MakeGaussianDerivative();
			int width = img.Width;
			int height = img.Height;
			double[,] luminances = new double[width,height]; //Yeah, yeah, this double array would be the transpose of the image if we rendered it, but we won't.
			for(int x = 0; x < width; x++)
			{
				for(int y = 0; y < height; y++)
				{
					Color pxval = img.GetPixel(x,y);
					luminances[x,y] = 0.299*pxval.R + 0.587*pxval.G + 0.114*pxval.B;
				}
			}
			double[,] x_derivatives = Convolve(Convolve(luminances,one_d_gaussian_derivative,true),one_d_gaussian,false); //Convolve horizontally with the derivative first, then vertically with the gaussian.
			double[,] y_derivatives = Convolve(Convolve(luminances,one_d_gaussian_derivative,false),one_d_gaussian,true); //Other way
			DenseVector[,] toReturn = new DenseVector[width, height];
			
			/*
			#region ONLY ONCE AND FOR TESTING I AM SORRY
			Bitmap xmap = new Bitmap(width,height);
			Bitmap ymap = new Bitmap(width,height);
			int minx = 0;
			int miny = 0;
			for(int x = 0; x < width; x++) for(int y = 0; y < width; y++){ 
				if(x_derivatives[x,y] < minx) minx = (int)x_derivatives[x,y];
				if(y_derivatives[x,y] < miny) miny = (int)y_derivatives[x,y];
			}
			#endregion
			*/
			for(int x = 0; x < width; x++)
			{
				for(int y = 0; y < height; y++)
				{
					toReturn[x,y] = new DenseVector(2);
					toReturn[x,y][0] = x_derivatives[x,y];
					
					//TODO REMOVE BELOW
					//int xo = (int) x_derivatives[x,y];
					//xo -= minx;
					//xmap.SetPixel(x,y,Color.FromArgb(xo,xo,xo));
					//TODO REMOVE ABOVE
					
					
					toReturn[x,y][1] = y_derivatives[x,y];
					
					//TODO REMOVE BELOW
					//ymap.SetPixel(x,y,Color.FromArgb((byte)(y_derivatives[x,y]),(byte)(y_derivatives[x,y]),(byte)(y_derivatives[x,y])));
					//TODO REMOVE ABOVE
				}
			}
			//xmap.Save(string.Format("XMAP{0}.png",Images_seen));
			//ymap.Save(string.Format("YMAP{0}.png",Images_seen));
			Images_seen++;
			return toReturn;
		}
		public static double[,] Convolve(double[,] twod_array, DenseVector kernel, bool is_horizontal)
		{
			int width = twod_array.GetLength(0);
			int height = twod_array.GetLength(1); //Still using the transposed convention
			double[,] toReturn = new double[width,height];
			for(int x = 0; x < width; x++)
			{
				for(int y = 0; y < height; y++)
				{
					toReturn[x,y] = 0;
					for(int i = 0; i < kernel.Count; i++)
					{
						int kernelidx = i - kernel.Count/2;
						double term;
						if(is_horizontal)
						{
							int arrayidx = x+kernelidx;
							//Reflect at boundaries
							if(arrayidx < 0)
							{
								arrayidx = -arrayidx;
							}
							if(arrayidx >= width)
							{
								arrayidx -= 2*(arrayidx-(width-1));
							}
							term = twod_array[arrayidx,y]*kernel[i];
						}
						else
						{
							int arrayidx = y+kernelidx;
							//Reflect at boundaries
							if(arrayidx < 0)
							{
								arrayidx = -arrayidx;
							}
							if(arrayidx >= height)
							{
								arrayidx -= 2*(arrayidx-(height-1));
							}
							term = twod_array[x,arrayidx]*kernel[i];
						}
						toReturn[x,y] += term;
					}
				}
			}
			return toReturn;
		}
		public static DenseVector MakeGaussian()
		{
			/*From wikipedia:
			 *"In theory, the Gaussian function at every point on the image will
			 *be non-zero, meaning that the entire image would need to be
			 *included in the calculations for each pixel. In practice, when
			 *computing a discrete approximation of the Gaussian function, pixels
			 *at a distance of more than 3σ are small enough to be considered
			 *effectively zero. Thus contributions from pixels outside that range
			 *can be ignored. Typically, an image processing program need only
			 *calculate a matrix with dimensions [6σ] × [6σ] (where [] is the
			 *ceiling function) to ensure a result sufficiently close to that
			 *obtained by the entire gaussian distribution.
			 */
			int dim = Convert.ToInt32(6*variation);
			int midindex = dim/2; //Integer division, but zero-indexing saves us.
			DenseVector gaussian = new DenseVector(dim);
			for(int i = 0; i < dim; i++)
			{
				int arg = i - midindex;
				gaussian[i] = 1d/Math.Sqrt(2d*Math.PI*Math.Pow(variation,2d)) * Math.Exp(-1*Math.Pow(arg,2)/(2*Math.Pow(variation,2)));
			}
			return gaussian;
		}
		public static DenseVector MakeGaussianDerivative()
		{
			int dim = Convert.ToInt32(6*variation);
			int midindex = dim/2; //Integer division, but zero-indexing saves us.
			DenseVector gaussian_derivative = new DenseVector(dim);
			for(int i = 0; i < dim; i++)
			{
				int arg = i - midindex;
				gaussian_derivative[i] = ((double)arg)/Math.Sqrt(2d*Math.PI*Math.Pow(variation,2d)) * Math.Exp(-1*Math.Pow(arg,2)/(2*Math.Pow(variation,2)));
			}
			return gaussian_derivative;
		}
		public static Classification ImportLabeling(string filename)
		{
			Label[,] labels = new Label[x_sites,y_sites];
			using(StreamReader csvfile = new StreamReader(filename))
			{
				for(int col = 0; col < x_sites; col++)
				{
					string line = csvfile.ReadLine();
					string[] vals = line.Split(',');
					for(int row = 0; row < y_sites; row++)
					{
						int val = Int32.Parse(vals[row]);
						if(val > 0)
							labels[row,col] = Label.ON;
						else
							labels[row,col] = Label.OFF;
						Sites_seen += 1;
						if(labels[row,col]==Label.ON) Ons_seen += 1;
					}
				}
			}
			return new Classification(labels);
		}
	}
}

