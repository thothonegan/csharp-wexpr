using System;

namespace Wexpr
{
    //
    /// <summary>
    /// These flags alter parsing of wexpr on load
    /// </summary>
    //
    [Flags]
    public enum ParseFlags
    {
        //
        /// <summary>
        /// No special flags
        /// </summary>
        //
        None = 0

        // future flags should be (1 << 0), (1 << 1), etc
    }
}
