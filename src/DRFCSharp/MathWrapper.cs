using System;
namespace DRFCSharp
{
    /// <summary>
    /// Wraps around a few basic math functions to shield from numerical errors.
    /// </summary>
    public sealed class MathWrapper
    {
        private MathWrapper()
        {
        }
        public static double Exp(double x){ return Math.Exp(x); }
        public static double Log(double x){ 
            if(x <= double.Epsilon)
            {
                throw new ArgumentException("Log of number smaller than epsilon.");
                return Math.Log (double.Epsilon);
            }
            return Math.Log(x);
        }
        // Logistic function
        public static double Sigma(double x){
            double result = 1/(1+Exp(-x));
            if(result < 0 || result > 1)
            {
                throw new NotFiniteNumberException("Sigma should be between 0 and 1");
            }
            if(result < double.Epsilon) {
                return double.Epsilon;
            }
            if(result > 1 - 2*double.Epsilon) {
                throw new ArgumentException("Too close to 1");
            } 
            return result; 
        }
    }
}

