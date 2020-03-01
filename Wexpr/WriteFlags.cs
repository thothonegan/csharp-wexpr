
using System;

namespace Wexpr
{
    //
    /// <summary>
    /// Flags which alter writing of wexpr strings.
    /// </summary>
    //
    [Flags]
    public enum WriteFlags
    {
        /// <summary>
        /// No special flags
        /// </summary>
        None = 0,

        /// <summary>
        /// Instead of trying to compress down, will add newlines and indentation to make it more readable.
        /// </summary>
        HumanReadable = (1 << 0)
    }
}