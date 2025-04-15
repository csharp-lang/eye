using FluentAssertions;

namespace Sorter.UnitTests
{
    public class MergerTests
    {
        [Theory]
        [TestCaseSource(nameof(MergeTestData))]
        public void MergeTest(IEnumerable<string>[] inputsStrings, IEnumerable<string> expected)
        {
            //Arrange
            var parsedInput = inputsStrings.Select(x => x.Select(line => new ParsedLine(line))).ToArray();

            //Act
            var actual = Merger.Merge(parsedInput).ToArray();

            //Assert
            actual.Should().Equal(expected);
        }


        #region Test data

        public static IEnumerable<object[]> MergeTestData =>
        [
            [Inputs, Expected],
            [InputsWithEmptyArrays, Expected],
            [InputsForMergeWithPriorityQueue, ExpectedForMergeWithPriorityQueue],
        ];

        private readonly static IEnumerable<string>[] Inputs =
        [
            [
                "415. Apple",
                "30432. Something something something",
            ],
            [
                "1. Apple",
                "32. Cherry is the best"
            ],
            [
                "2. Banana is yellow"
            ]
        ];

        private readonly static IEnumerable<string> Expected =
        [
            "1. Apple",
            "415. Apple",
            "2. Banana is yellow",
            "32. Cherry is the best",
            "30432. Something something something"
        ];

        private readonly static IEnumerable<string>[] InputsWithEmptyArrays =
        [
            [
                "415. Apple",
                "30432. Something something something",
            ],
            [],
            [],
            [
                "1. Apple",
                "32. Cherry is the best"
            ],
            [
                "2. Banana is yellow"
            ],
            []
        ];

        private readonly static IEnumerable<string>[] InputsForMergeWithPriorityQueue =
        [
            [
                "1. Abc",
                "2. Bbc",
                "3. Cbc",
            ],
            [
                "7. Gbc",
                "8. Hbc",
                "9. Ibc",
                "10. Jbc",
            ],
            [
                "11. Kbc",
                "12. Lbc",
                "13. Mbc",
                "14. Nbc",
                "15. Obc"
            ],
            [
                "10. Abc",
                "4. Dbc",
                "5. Ebc",
                "6. Fbc",
            ],
            [
                "14. Mbc",
                "16. Pbc",
                "17. Qbc",
                "18. Rbc",
            ],
            [
                "19. Sbc",
                "20. Tbc"
            ],
            [
                "21. Ubc",
                "22. Vbc",
                "23. Wbc",
                "24. Xbc",
                "25. Ybc",
                "26. Zbc"
            ]
        ];

        private readonly static IEnumerable<string> ExpectedForMergeWithPriorityQueue =
        [
            "1. Abc",
            "10. Abc",
            "2. Bbc",
            "3. Cbc",
            "4. Dbc",
            "5. Ebc",
            "6. Fbc",
            "7. Gbc",
            "8. Hbc",
            "9. Ibc",
            "10. Jbc",
            "11. Kbc",
            "12. Lbc",
            "13. Mbc",
            "14. Mbc",
            "14. Nbc",
            "15. Obc",
            "16. Pbc",
            "17. Qbc",
            "18. Rbc",
            "19. Sbc",
            "20. Tbc",
            "21. Ubc",
            "22. Vbc",
            "23. Wbc",
            "24. Xbc",
            "25. Ybc",
            "26. Zbc",
        ];

        #endregion
    }
}