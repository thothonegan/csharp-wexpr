using System;
using System.Collections.Generic;
using System.Text;

namespace Wexpr
{
    //
    /// <summary>
    /// Base class for all Wexpr errors
    /// </summary>
    //
    public class Exception : System.Exception
    {
        public Exception(string msg) : base(msg) { }
        public Exception (int line, int column, string msg) : base (msg)
        {
            Line = line;
            Column = column;
        }

        //
        /// <summary>
        /// The line the error occurred. 0 if unknown.
        /// </summary>
        //
        public int Line
        { get; set; } = 0;

        //
        /// <summary>
        /// The column the error occurred. 0 if unknown.
        /// </summary>
        //
        public int Column
        { get; set; } = 0;
    }

    // --- Specific instances

    /// <summary> A string with a quote is missing the end quote. </summary>
    public class StringMissingEndingQuoteException : Exception
    {
        public StringMissingEndingQuoteException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> UTF8 was invalid. </summary>
    public class InvalidUTF8Exception : Exception
    {
        public InvalidUTF8Exception(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> Got extra data after we parsed the first object from the wexpr.</summary>
    public class ExtraDataAfterParsingRootException : Exception
    {
        public ExtraDataAfterParsingRootException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> An empty string was given when we require one. </summary>
    public class EmptyStringException : Exception
    {
        public EmptyStringException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> A string contains an invalid escape. </summary>
    public class InvalidStringEscapeException : Exception
    {
        public InvalidStringEscapeException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> Parsing a map its missing the ending paren. </summary>
    public class MapMissingEndParenException : Exception
    {
        public MapMissingEndParenException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> Map keys must be a value. </summary>
    public class MapKeyMustBeAValueException : Exception
    {
        public MapKeyMustBeAValueException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> A key had no value before the map ended. </summary>
    public class MapNoValueException : Exception
    {
        public MapNoValueException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> A reference is missing an end bracket. </summary>
    public class ReferenceMissingEndBracketException : Exception
    {
        public ReferenceMissingEndBracketException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> A reference we tried to isnert is missing an end bracket. </summary>
    public class ReferenceInsertMissingEndBracketException : Exception
    {
        public ReferenceInsertMissingEndBracketException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> Tried to look for a reference, but it didn't exist. </summary>
    public class ReferenceUnknownReferenceException : Exception
    {
        public ReferenceUnknownReferenceException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> Tried to find the ending paren, but it didn't exist.</summary>
    public class ArrayMissingEndParenException : Exception
    {
        public ArrayMissingEndParenException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary> A reference has an invalid character in it. </summary>
    public class ReferenceInvalidNameException : Exception
    {
        public ReferenceInvalidNameException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary>Binary data had no ending &gt.</summary>
    public class BinaryDataNoEndingException : Exception
    {
        public BinaryDataNoEndingException(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary>Unable to parse the Base64 data.</summary>
    public class BinaryDataInvalidBase64Exception : Exception
    {
        public BinaryDataInvalidBase64Exception(int r, int c, string msg) : base(r, c, msg) { }
    }

    /// <summary>The binary header didn't make sense.</summary>
    public class BinaryInvalidHeaderException : Exception
    {
        public BinaryInvalidHeaderException(string msg) : base(msg) { }
    }

    /// <summary>The version was unknown.</summary>
    public class BinaryUnknownVersionException : Exception
    {
        public BinaryUnknownVersionException(string msg) : base(msg) { }
    }

    /// <summary>Found multiple expression chunks.</summary>
    public class BinaryMultipleExpressionsException : Exception
    {
        public BinaryMultipleExpressionsException(string msg) : base(msg) { }
    }

    /// <summary>The chunk size said to expand past the buffer size.</summary>
    public class BinaryChunkBiggerThanDataException : Exception
    {
        public BinaryChunkBiggerThanDataException(string msg) : base(msg) { }
    }

    /// <summary>The length of buffer given wasn't big enough for a valid chunk.</summary>
    public class BinaryChunkNotBigEnoughException : Exception
    {
        public BinaryChunkNotBigEnoughException(string msg) : base(msg) { }
    }

    /// <summary>An unknown compression method was used.</summary>
    public class BinaryUnknownCompressionException : Exception
    {
        public BinaryUnknownCompressionException(string msg) : base(msg) { }
    }

    // --- csharp specific

    /// <summary>Tried to perform an operation which requires a specific expression type, and we aren't that type.</summary>
    public class InvalidExpressionTypeException : Exception
    {
        public InvalidExpressionTypeException(string msg) : base(msg) { }
    }
}
