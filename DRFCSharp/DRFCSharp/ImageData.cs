using System;
using System.Drawing;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	public class ImageData
	{
		public const int x_sites = 16; //Make sure these divide the image dimensions. The size of the sites is deduced from them.
		public const int y_sites = 16;
		public const double variation = 0.5d; //Make sure 6*variation is odd.
		public SiteFeatureSet[,] site_features;
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
			throw new NotImplementedException();
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
			for(int x = 0; x < width; x++)
			{
				for(int y = 0; y < height; y++)
				{
					toReturn[x,y] = new DenseVector(2);
					toReturn[x,y][0] = x_derivatives[x,y];
					toReturn[x,y][1] = y_derivatives[x,y];
				}
			}
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
	}
}

