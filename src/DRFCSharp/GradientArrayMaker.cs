using System;
using System.Drawing;
using System.Collections.Concurrent;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	/// <summary>
	/// Calculates and caches gradient arrays from bitmaps based on hyperparameters
	/// provided at initialization.
	/// </summary>
	public sealed class GradientArrayMaker
	{
		/// <summary>
		/// Gets the variance.
		/// </summary>
		/// <value>
		/// The variance of the gaussian blur applied to the images while calculating
		/// the gradients. Six times this number must be odd, so it should be 2n + 0.5.
		/// </value>
		public double Variance{ get; private set; }

		/// <summary>
		/// Gets or sets the gradient cache.
		/// </summary>
		/// <value>
		/// The cache of pre-calculated gradient arrays of bitmaps we have already seen.
		/// </value>
		private ConcurrentDictionary<Bitmap, DenseVector[,]> GradientCache{ get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DRFCSharp.GradientArrayMaker"/> class.
		/// </summary>
		/// <param name='variance'>
		/// Variance of the Gaussian smoothing. Six times this number must be odd, so it should be 2n + 0.5.
		/// </param>
		public GradientArrayMaker(double variance)
		{
			GradientCache = new ConcurrentDictionary<Bitmap, DenseVector[,]>();
			Variance = variance;
		}

		/// <summary>
		/// Gets the gradient array corresponding to a particular bitmap. It will look in the
		/// cache first, to reduce recalculation cost.
		/// </summary>
		/// <returns>
		/// The gradients, as an array of DenseVectors.
		/// </returns>
		/// <param name='bmp'>
		/// The image.
		/// </param>
		public DenseVector[,] GetGradients(Bitmap bmp){
			DenseVector[,] grads;
			if(GradientCache.TryGetValue(bmp, out grads)){
				return grads;
			}
			else{
				grads = SmoothedGradientArray(bmp);
				//Not inconceivable that it's been added by the time we get here, but if so no big deal.
				//We recalculate once, and we should get the same result both times.
				GradientCache.TryAdd(bmp, grads);
				return grads;
			}
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
		private DenseVector[,] SmoothedGradientArray(Bitmap img)
		{
			DenseVector one_d_gaussian = MakeGaussian(Variance);
			DenseVector one_d_gaussian_derivative = MakeGaussianDerivative(Variance);
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

		/// <summary>
		/// Convolve the specified 2d array together with a 1d kernel, either horizontally or vertically.
		/// </summary>
		/// <param name='twod_array'>
		/// A 2d array of doubles.
		/// </param>
		/// <param name='kernel'>
		/// The convolution kernel.
		/// </param>
		/// <param name='is_horizontal'>
		/// Whether we should convolve horizontally or vertically.
		/// </param>
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
						int kernelidx = i - kernel.Count/2; //Assuming the kernel is of odd dimension. 
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

		/// <summary>
		/// Makes a 1d Gaussian kernel for smoothing.
		/// </summary>
		/// <returns>
		/// The gaussian.
		/// </returns>
		public static DenseVector MakeGaussian(double sigma)
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
			int dim = Convert.ToInt32(6*sigma);
			if(dim % 2 == 0) throw new ArgumentException("Sigma should be 1/6th an odd number.");
			int midindex = dim/2; //Integer division, but zero-indexing saves us.
			DenseVector gaussian = new DenseVector(dim);
			for(int i = 0; i < dim; i++)
			{
				int arg = i - midindex;
				gaussian[i] = 1d/Math.Sqrt(2d*Math.PI*Math.Pow(sigma,2d)) * Math.Exp(-1*Math.Pow(arg,2)/(2*Math.Pow(sigma,2)));
			}
			return gaussian;
		}

		/// <summary>
		/// Makes a 1d derivative-of-Gaussian kernel for convolution.
		/// </summary>
		/// <returns>
		/// The gaussian derivative.
		/// </returns>
		public static DenseVector MakeGaussianDerivative(double sigma)
		{
			int dim = Convert.ToInt32(6*sigma);
			if(dim % 2 == 0) throw new ArgumentException("Sigma should be 1/6th an odd number.");
			int midindex = dim/2; //Integer division, but zero-indexing saves us.
			DenseVector gaussian_derivative = new DenseVector(dim);
			for(int i = 0; i < dim; i++)
			{
				int arg = i - midindex;
				gaussian_derivative[i] = ((double)arg)/Math.Sqrt(2d*Math.PI*Math.Pow(sigma,2d)) * Math.Exp(-1*Math.Pow(arg,2)/(2*Math.Pow(sigma,2)));
			}
			return gaussian_derivative;
		}
	}
}

