using FluentAssertions;

namespace Sorter.UnitTests
{
    public class ParsedLineTests
    {
        private readonly string[] input =
        [
            "415. Apple",
            "30432. Something something something",
            "1. Apple",
            "32. Cherry is the best",
            "2. Banana is yellow"
        ];

        private readonly string[] output =
        [
            "1. Apple",
            "415. Apple",
            "2. Banana is yellow",
            "32. Cherry is the best",
            "30432. Something something something"
        ];

        [Test]
        public void ParsedLine_Should_Be_Sorted_By_Rules()
        {
            //Arrange
            var parsed = input.Select(s => new ParsedLine(s)).ToArray();

            //Act
            Array.Sort(parsed);

            //Assert
            parsed.Select(s => s.Line).Should().Equal(output);
        }

        [Theory]
        [TestCase("-25. A", "-25. B", -1)]
        [TestCase("0. A", "0. A", 0)]
        [TestCase("25. A", "25. B", -1)]
        [TestCase("100. A", "1. A", 1)]
        [TestCase("-0. A", "1. A", -1)]
        [TestCase("-0. A", "-1. A", 1)]
        [TestCase("-2. A", "-1. A", -1)]
        [TestCase("-100. A", "1. A", -1)]
        [TestCase("1510261596. ae1e1cb6-5856-49c8-87f0-eecd8575b6b1", "-380096469. ae1e1cb6-5856-49c8-87f0-eecd8575b6b1", 1)]
        public void LineComparer_Should_Compare_Strings_By_Rules(string x, string y, int expected)
        {
            //Arrange
            var parsedX = new ParsedLine(x);
            var parsedY = new ParsedLine(y);

            //Act
            var actual = parsedX.CompareTo(parsedY);

            //Assert
            switch (expected)
            {
                case -1:
                    actual.Should().BeNegative();
                    break;
                case 0:
                    actual.Should().Be(0);
                    break;
                case 1:
                    actual.Should().BePositive();
                    break;
            }
        }
    }
}
