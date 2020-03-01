using System;
using Xunit;

namespace Wexpr.Tests
{
    public class ExpressionTypeTests
    {
        [Fact]
        public void ExpressionTypeCanReturnString()
        {
            var nullStr = ExpressionType.Null.ToString();
            var valueStr = ExpressionType.Value.ToString();
            var mapStr = ExpressionType.Map.ToString();
            var arrayStr = ExpressionType.Array.ToString();

            Assert.Equal("Null", nullStr);
            Assert.Equal("Value", valueStr);
            Assert.Equal("Map", mapStr);
            Assert.Equal("Array", arrayStr);
        }
    }
}
