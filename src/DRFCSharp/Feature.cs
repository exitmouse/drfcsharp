using System;
using System.Collections.Generic;
using System.Drawing;

namespace DRFCSharp
{
    public interface Feature
    {
        /*Consider having a Register(FeatureManager fm) method,
         * which would add a delegate to a queue that all get called during certain parts of the
         * feature calculation process; different queues for different things. Some queues would have access to HOGs, others wouldn't, etc.
         * The Register(Feature f) method on FeatureManager would handle calling f.Register(this);*/
        /*Tim's idea is better: Wrap around bitmap to only calculate the histogram once and associate it with the bitmap in the class.
         * That means this interface is the correct one, modulo Bitmap being replaced by that wrapper class.*/
        /*Even better plan: do as below, but then have a static global cache of HOGs of bitmaps, so you don't recalculate the HOGs.*/
        /// <summary>
        /// Given a bitmap, and site coordinates x and y, returns the
        /// vector-valued result of the feature. The site coordinates will
        /// probably be used to correspond to windows of the image using a
        /// FeatureApplicationScheme, but this is not mandated here.
        /// </summary>
        /// <param name='bmp'>
        /// The bitmap to calculate the result of the feature on.
        /// </param>
        /// <param name='x'>
        /// The site x coordinate to look at.
        /// </param>
        /// <param name='y'>
        /// The site y coordinate to look at.
        /// </param>
        List<double> Calculate(Bitmap bmp, int x, int y);
        int Length{ get; }
        string Name(int idx);
    }
}

