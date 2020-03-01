using System;
using System.Collections.Generic;
using System.Text;

namespace Wexpr
{
    //
    /// <summary>
    /// Different types an expression can be.
    /// These numbers are also used in binary formats as needed.
    /// </summary>
    //
    public enum ExpressionType
    {
        Null = 0x00,
        Value = 0x01,
        Array = 0x02,
        Map = 0x03,
        BinaryData = 0x04,
        Invalid = 0xFF
    }

    //
    /// <summary>
    /// Extension methods for ExpressionType
    /// </summary>
    //
    public static class ExpressionTypeExtensions
    {
        public static string ToString(this ExpressionType self)
        {
            switch (self)
            {
                case ExpressionType.Invalid: return "Invalid";
                case ExpressionType.Null: return "Null";
                case ExpressionType.Value: return "Value";
                case ExpressionType.Map: return "Map";
                case ExpressionType.Array: return "Array";

                default:
                    return null;
            }
        }
    }
}
