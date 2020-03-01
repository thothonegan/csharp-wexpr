using System;
using Xunit;

namespace Wexpr.Tests
{
    public class ExpressionErrorsTests
    {
        [Fact]
        public void ExpressionErrorsEmptyIsInvalid()
        {
            var ex = Assert.Throws<EmptyStringException>(() =>
            {
                Expression.CreateFromString("", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(1, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsExtraDataAfterExpression()
        {
            var ex = Assert.Throws<ExtraDataAfterParsingRootException>(() =>
            {
                Expression.CreateFromString("#(1) 1", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(6, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsArrayMissingEndParen()
        {
            var ex = Assert.Throws<ArrayMissingEndParenException>(() =>
            {
                Expression.CreateFromString("#(", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(3, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsMapMissingEndParen()
        {
            var ex = Assert.Throws<MapMissingEndParenException>(() =>
            {
                Expression.CreateFromString("@(", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(3, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsMapKeysMustBeValues()
        {
            var ex = Assert.Throws<MapKeyMustBeAValueException>(() =>
            {
                Expression.CreateFromString("@(#() a)", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(3, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsReferenceMissingItsEndingBracket()
        {
            var ex = Assert.Throws<ReferenceMissingEndBracketException>(() =>
            {
                Expression.CreateFromString("[", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(1, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsReferenceInvalid()
        {
            var ex = Assert.Throws<ReferenceUnknownReferenceException>(() =>
            {
                Expression.CreateFromString("*[asdf]", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(8, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsBlankIsError()
        {
            var ex = Assert.Throws<EmptyStringException>(() =>
            {
                Expression.CreateFromString("", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(1, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsJustCommentIsError()
        {
            var ex = Assert.Throws<EmptyStringException>(() =>
            {
                Expression.CreateFromString(" ;(-- asdf --)  ", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(17, ex.Column);
        }

        [Fact]
        public void ExpressionErrorsInvalidReferenceName()
        {
            var ex = Assert.Throws<ReferenceInvalidNameException>(() =>
            {
                Expression.CreateFromString("[asd-b] c", ParseFlags.None);
            });

            Assert.Equal(1, ex.Line);
            Assert.Equal(1, ex.Column);
        }

        [Fact]
        public void ExpressionErrorProperLineWhenUnixStyleLineEnding()
        {
            var ex = Assert.Throws<ExtraDataAfterParsingRootException>(() =>
            {
                Expression.CreateFromString("\n#(a) 1", ParseFlags.None);
            });

            Assert.Equal(2, ex.Line);
            Assert.Equal(6, ex.Column);
        }

        [Fact]
        public void ExpressionErrorProperLineWhenWindowsStyleLineEnding()
        {
            var ex = Assert.Throws<ExtraDataAfterParsingRootException>(() =>
            {
                Expression.CreateFromString("\r\n#(a) 1", ParseFlags.None);
            });

            Assert.Equal(2, ex.Line);
            Assert.Equal(6, ex.Column);
        }
    }
}
