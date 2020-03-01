using System;
using Xunit;

namespace Wexpr.Tests
{
    public class ReferenceTableTests
    {
        [Fact]
        public void ReferenceTableCanCreate()
        {
            var table = new ReferenceTable();

            Assert.Equal(0, table.Count);
            Assert.Null(table.ExpressionForKey("unknown"));
        }

        [Fact]
        public void ReferenceTableCanSetKey()
        {
            var table = new ReferenceTable();
            var val = Expression.CreateValue("asdf");
            table.SetExpressionForKey("key", val);

            var valFromTable = table.ExpressionForKey("key");

            Assert.NotNull(valFromTable);
            Assert.Equal("asdf", valFromTable.Value);
        }

    }
}
