using System;
using System.Collections.Generic;

namespace DRFCSharp
{
    /// <summary>
    /// A FeatureApplicationScheme is a pairing between site coordinates and
    /// <see cref="Window"/>s in the image.  Essentially, it allows a
    /// <see cref="Feature"/> to easily figure out he image window it should be
    /// looking at for the feature it's trying to apply at a site. The
    /// Feature's calculate method has been called on, say, the <3,4>th site in
    /// the image, but in order to know the exact pixel window it should take
    /// the HOGS of, a FeatureApplicationScheme translates.
    /// </summary>
    public interface FeatureApplicationScheme
    {
        int NumScales { get; }
        //TODO NumXs and NumYs
        /// <summary>
        /// Gets the <see cref="DRFCSharp.Window"/> for the site with the specified x and y.
        /// </summary>
        /// <param name='x'>
        /// X.
        /// </param>
        /// <param name='y'>
        /// Y.
        /// </param>
        Window this[int x, int y, int scale] { get; }
    }
}

