using System;
using System.Text;
using Xunit;

namespace Wexpr.Tests
{
    public class ExpressionTests
    {
        [Fact]
        public void ExpressionCanCreateNull()
        {
            var nullExpr = Expression.CreateNull();
            Assert.NotNull(nullExpr);
            Assert.Equal(ExpressionType.Null, nullExpr.ExpressionType);
        }

        [Fact]
        public void ExpressionCanCreateValue()
        {
            var valueExpr = Expression.CreateFromString("val", ParseFlags.None);
            Assert.NotNull(valueExpr);
            Assert.Equal(ExpressionType.Value, valueExpr.ExpressionType);
            Assert.Equal("val", valueExpr.Value);
        }

        [Fact]
        public void ExpressionCanCreateQuotedValue()
        {
            var valueExpr = Expression.CreateFromString(" \"val\" ", ParseFlags.None);
            Assert.NotNull(valueExpr);
            Assert.Equal(ExpressionType.Value, valueExpr.ExpressionType);
            Assert.Equal("val", valueExpr.Value);
        }

        [Fact]
        public void ExpressionCanCreateEscapedValue()
        {
            var valueExpr = Expression.CreateFromString(" \"val\\\"\" ", ParseFlags.None);
            Assert.NotNull(valueExpr);
            Assert.Equal(ExpressionType.Value, valueExpr.ExpressionType);
            Assert.Equal("val\"", valueExpr.Value);
        }

        [Fact]
        public void ExpressionCanEncodeEscapedValue()
        {
            var valueExpr = Expression.CreateFromString(" \"val\\\"\" ", ParseFlags.None);

            var buf = valueExpr.CreateStringRepresentation(0, WriteFlags.None);

            Assert.Equal("\"val\\\"\"", buf);
        }

        [Fact]
        public void ExpressionCanCreateNumber()
        {
            var valueExpr = Expression.CreateFromString("2.45", ParseFlags.None);
            Assert.NotNull(valueExpr);
            Assert.Equal(ExpressionType.Value, valueExpr.ExpressionType);
            Assert.Equal("2.45", valueExpr.Value);
        }

        [Fact]
        public void ExpressionCanCreateArray()
        {
            var arrayExpr = Expression.CreateFromString("#(1 2 3)", ParseFlags.None);

            Assert.NotNull(arrayExpr);
            Assert.Equal(ExpressionType.Array, arrayExpr.ExpressionType);
            Assert.Equal(3, arrayExpr.ArrayCount);

            var item0 = arrayExpr.ArrayAt(0);
            var item1 = arrayExpr.ArrayAt(1);
            var item2 = arrayExpr.ArrayAt(2);

            Assert.Equal(ExpressionType.Value, item0.ExpressionType);
            Assert.Equal(ExpressionType.Value, item1.ExpressionType);
            Assert.Equal(ExpressionType.Value, item2.ExpressionType);

            Assert.Equal("1", item0.Value);
            Assert.Equal("2", item1.Value);
            Assert.Equal("3", item2.Value);
        }

        [Fact]
        public void ExpressionCanCreateMap()
        {
            var mapExpr = Expression.CreateFromString("@(a b c d)", ParseFlags.None);

            Assert.NotNull(mapExpr);
            Assert.Equal(ExpressionType.Map, mapExpr.ExpressionType);
            Assert.Equal(2, mapExpr.MapCount);

            // we can iterate multiple ways

            // first, by index
            bool seenA = false;
            bool seenC = false;

            var mapKey0 = mapExpr.MapKeyAt(0);
            var mapKey1 = mapExpr.MapKeyAt(1);
            var mapValue0 = mapExpr.MapValueAt(0);
            var mapValue1 = mapExpr.MapValueAt(1);

            var mapValue0Value = mapValue0.Value;
            var mapValue1Value = mapValue1.Value;

            if (mapKey0 == "a")
            {
                Assert.False(seenA); // shouldnt see A twice
                Assert.Equal("b", mapValue0Value);

                seenA = true;
            }

            if (mapKey1 == "a")
            {
                Assert.False(seenA); // shouldnt see A twice
                Assert.Equal("b", mapValue1Value);

                seenA = true;
            }

            if (mapKey0 == "c")
            {
                Assert.False(seenC); // shouldnt see A twice
                Assert.Equal("d", mapValue0Value);

                seenC = true;
            }

            if (mapKey1 == "c")
            {
                Assert.False(seenC); // shouldnt see A twice
                Assert.Equal("d", mapValue1Value);

                seenC = true;
            }

            Assert.True(seenA);
            Assert.True(seenC);

            // second by key
            var val0 = mapExpr.MapValueForKey("a");
            var val1 = mapExpr.MapValueForKey("c");

            var val0Value = val0.Value;
            var val1Value = val1.Value;

            Assert.Equal("b", val0Value);
            Assert.Equal("d", val1Value);
        }

        // exprCanLoadUTF8
        // exprCanIgnoreComments
        // exprCanIgnoreInlineComment
        // exprCanConvertBackToString

        [Fact]
        public void ExpressionCanUnderstandReference()
        {
            var expr = Expression.CreateFromString("@(first [val]\"name\")", ParseFlags.None);
            Assert.Equal(ExpressionType.Map, expr.ExpressionType);

            var val = expr.MapValueForKey("first");
            Assert.Equal(ExpressionType.Value, val.ExpressionType);
            Assert.Equal("name", val.Value); // ignored the reference
        }

        [Fact]
        public void ExpressionCanDerefReference()
        {
            var expr = Expression.CreateFromString("@(first [val]\"name\" second *[val])", ParseFlags.None);
            Assert.Equal(ExpressionType.Map, expr.ExpressionType);

            var val = expr.MapValueForKey("second");
            Assert.Equal(ExpressionType.Value, val.ExpressionType);
            Assert.Equal("name", val.Value); // used the reference
        }

        [Fact]
        public void ExpressionCanDerefArrayReference()
        {
            var expr = Expression.CreateFromString("@(first [val]#(1 2) second *[val])", ParseFlags.None);
            Assert.Equal(ExpressionType.Map, expr.ExpressionType);

            var val = expr.MapValueForKey("second");
            Assert.Equal(ExpressionType.Array, val.ExpressionType);
            Assert.Equal(2, val.ArrayCount);
        }

        [Fact]
        public void ExpressionCanDerefMapProperly()
        {
            var expr = Expression.CreateFromString("@(first [val] @(a b) second *[val])", ParseFlags.None);
            Assert.Equal(ExpressionType.Map, expr.ExpressionType);

            var val = expr.MapValueForKey("second");
            Assert.Equal(ExpressionType.Map, val.ExpressionType);

            var val2 = val.MapValueForKey("a");
            Assert.Equal("b", val2.Value);
        }

        [Fact]
        public void ExpressionCanDerefFromExternalTable()
        {
            var refTable = new ReferenceTable();
            refTable.SetExpressionForKey("name", Expression.CreateValue("Bob"));

            var expr = Expression.CreateFromStringWithExternalReferenceTable(
                "@(playerName *[name])", ParseFlags.None,
                refTable
            );

            Assert.Equal(ExpressionType.Map, expr.ExpressionType);

            var val = expr.MapValueForKey("playerName");
            Assert.Equal(ExpressionType.Value, val.ExpressionType);
            Assert.Equal("Bob", val.Value);
        }

        [Fact]
        public void ExpressionCanCreateString()
        {
            var expr = Expression.CreateFromString(
                "@(first #(a b) second \"20% cooler\")",
                ParseFlags.None
            );

            string notHumanReadableString1 = "@(second \"20% cooler\" first #(a b))";
            string notHumanReadableString2 = "@(first #(a b) second \"20% cooler\")";
            string humanReadableString1 =
                "@(\n" +
                "\tsecond \"20% cooler\"\n" +
                "\tfirst #(\n" +
                "\t\ta\n" +
                "\t\tb\n" +
                "\t)" +
                ")"
            ;
            string humanReadableString2 =
                "@(\n" +
                "\tfirst #(\n" +
                "\t\ta\n" +
                "\t\tb\n" +
                "\t)\n" +
                "\tsecond \"20% cooler\"\n" +
                ")"
            ;

            var buffer = expr.CreateStringRepresentation(0, WriteFlags.None);
            var buffer2 = expr.CreateStringRepresentation(0, WriteFlags.HumanReadable);

            Assert.True(
                (buffer == notHumanReadableString1) ||
                (buffer == notHumanReadableString2)
            );

            Assert.True(
                (buffer2 == humanReadableString1) ||
                (buffer2 == humanReadableString2)
            );
        }

        [Fact]
        public void ExpressionCanChangeType()
        {
            var expr = Expression.CreateNull();
            expr.ChangeType(ExpressionType.Value);

            Assert.Equal(ExpressionType.Value, expr.ExpressionType);
        }

        [Fact]
        public void ExpressionCanSetValue()
        {
            var expr = Expression.CreateNull();
            expr.ChangeType(ExpressionType.Value);
            expr.Value = "asdf";

            Assert.Equal("asdf", expr.Value);
        }

        [Fact]
        public void ExpressionCanAddToArray()
        {
            var expr = Expression.CreateNull();
            expr.ChangeType(ExpressionType.Array);

            var elem = Expression.CreateValue("a");
            expr.ArrayAddElementToEnd(elem);

            elem = Expression.CreateValue("b");
            expr.ArrayAddElementToEnd(elem);

            elem = Expression.CreateValue("c");
            expr.ArrayAddElementToEnd(elem);

            string[] expected = { "a", "b", "c" };

            for (int i=0; i < 3; ++i)
            {
                var val = expr.ArrayAt(i);
                Assert.Equal(ExpressionType.Value, val.ExpressionType);
                Assert.Equal(expected[i], val.Value);
            }
        }

        [Fact]
        public void ExpressionCanSetInMap()
        {
            var expr = Expression.CreateNull();
            expr.ChangeType(ExpressionType.Map);
            expr.MapSetValueForKey("key", Expression.CreateValue("value"));

            var val = expr.MapValueForKey("key");

            Assert.Equal(ExpressionType.Value, val.ExpressionType);
            Assert.Equal("value", val.Value);
        }

        [Fact]
        public void ExpressionCanHandleNullExpression()
        {
            var nullExpr = Expression.CreateFromString("null", ParseFlags.None);
            Assert.Equal(ExpressionType.Null, nullExpr.ExpressionType);

            var nilExpr = Expression.CreateFromString("nil", ParseFlags.None);
            Assert.Equal(ExpressionType.Null, nilExpr.ExpressionType);
        }

        [Fact]
        public void ExpressionCanHandleBinaryExpression()
        {
            var binExpr = Expression.CreateFromString("<aGVsbG8=>", ParseFlags.None);
            Assert.Equal(ExpressionType.BinaryData, binExpr.ExpressionType);

            byte[] buffer = binExpr.BinaryData;

            Assert.Equal(5, buffer.Length);

            var ascii = new ASCIIEncoding();
            Assert.Equal("hello", ascii.GetString(buffer));
        }
    }
}
