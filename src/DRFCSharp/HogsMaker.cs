using System;
using System.Drawing;
using System.Collections.Concurrent;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DRFCSharp
{
	/// <summary>
	/// Calculates a histogram of oriented gradients using a GradientArrayMaker object
	/// and a provided number of orientation bins.
	/// </summary>
	public sealed class HogsMaker
	{
		/// <summary>
		/// Gets the number of orientation bins.
		/// </summary>
		/// <value>
		/// The number of orientation bins in the histograms this HogsMaker makes.
		/// </value>
		public int NumOrientations{ get; private set; }

		/// <summary>
		/// Gets or sets the gradient maker.
		/// </summary>
		/// <value>
		/// The gradient maker.
		/// </value>
		private GradientArrayMaker GradientMaker{ get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DRFCSharp.HogsMaker"/> class.
		/// </summary>
		/// <param name='gm'>
		/// A <see cref="DRFCSharp.GradientArrayMaker"/> object, to be used for HOGs calculations.
		/// </param>
		/// <param name='variance'>
		/// Variance.
		/// </param>
		public HogsMaker(GradientArrayMaker gm, int num_orientations)
		{
			GradientMaker = gm;
			NumOrientations = num_orientations;
		}

		/// <summary>
		/// Gets the histogram of oriented gradients corresponding to a particular window in the specified bmp.
		/// </summary>
		/// <returns>
		/// The hogs.
		/// </returns>
		/// <param name='bmp'>
		/// The bitmap.
		/// </param>
		/// <param name='w'>
		/// The window over which to make the histogram.
		/// </param>
		public double[] GetHogs(Bitmap bmp, Window w)
		{
			DenseVector[,] grads = GradientMaker.GetGradients(bmp);
			double[] hogs = new double[NumOrientations];
			for(int x = w.StartX; x < w.EndX; x++) for(int y = w.StartY; y < w.EndY; y++)
			{
				DenseVector g = grads[x,y];
				//PREPARE FOR HACK
				double angle = Math.Atan2 (g[1],g[0]);
				if(angle < 0) angle += 2*Math.PI;
				int orientation = (int)Math.Floor((((double)NumOrientations)/(2*Math.PI))*angle);
				orientation = orientation % NumOrientations; //Hack moar--mitigated now. Still bad.
				double magnitude = g.Norm(2);
				hogs[orientation] += magnitude;
			}
			return hogs;
		}

		/// <summary>
		/// Gets the histogram of oriented gradients corresponding to a particular window in the specified bmp,
		/// and smoothes it.
		/// </summary>
		/// <returns>
		/// The hogs.
		/// </returns>
		/// <param name='bmp'>
		/// The bitmap.
		/// </param>
		/// <param name='w'>
		/// The window over which to make the histogram.
		/// </param>
		public double[] GetSmoothedHogs(Bitmap bmp, Window w)
		{
			if(bmp == null) throw new ArgumentNullException("bmp", "Specify a non-null argument.");
			if(w == null) throw new ArgumentNullException("w", "Specify a non-null argument.");
			double[] hogs = GetHogs(bmp, w);
			double[] shogs = new double[NumOrientations];
			for(int i = 0; i < NumOrientations; i++)
			{
				double numerator = 0;
				double denom = 0;

				int b = Convert.ToInt32(Math.Ceiling(SMOOTHING_KERNEL_BANDWIDTH));
				for(int j = i - Math.Min(b, NumOrientations/2); j <= i + Math.Min(b, NumOrientations/2); j++)
				{
					// As long as the kernel bandwidth is not larger than NUM_ORIENTATIONS/2, we don't 
					// count directions multiple times.
					double coeff = SmoothingKernel(((double)(i-j))/SMOOTHING_KERNEL_BANDWIDTH);
					denom += coeff;
					numerator += coeff*hogs[(j+NumOrientations)%NumOrientations];
				}
				shogs[i] = numerator/denom;
				if(denom == 0) throw new NotFiniteNumberException();

				//Dealing with area mismatches
				shogs[i] *= 4096;
				shogs[i] /= w.Area;
			}
			return shogs;
		}

		//TODO: Make this a strategy
		private const double SMOOTHING_KERNEL_BANDWIDTH = 2.0d;
		private static double SmoothingKernel(double argument)
		{
			//Using triangular kernel.
			if(Math.Abs(argument) > 1d) return 0;
			return (1-Math.Abs(argument));
		}
	}
}

