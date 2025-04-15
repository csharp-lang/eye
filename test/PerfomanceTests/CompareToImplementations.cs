using System;
using BenchmarkDotNet.Attributes;
using Sorter;

namespace PerfomanceTests
{
    [MemoryDiagnoser]
    public class CompareToImplementations
    {
        private readonly string string1 = new("30432. Something something something");
        private readonly string string2 = new("32. Cherry is the best");

        private readonly ParsedLineReadOnlyMemory parsedLineReadOnlyMemory1;
        private readonly ParsedLineReadOnlyMemory parsedLineReadOnlyMemory2;

        private readonly ParsedLineReadOnlySpanOrdinal parsedLineReadOnlySpanOrdinal1;
        private readonly ParsedLineReadOnlySpanOrdinal parsedLineReadOnlySpanOrdinal2;

        private readonly ParsedLineReadOnlySpanSequence parsedLineReadOnlySpanSequence1;
        private readonly ParsedLineReadOnlySpanSequence parsedLineReadOnlySpanSequence2;

        private readonly ParsedLineReadOnlySpanSequenceWithoutDot parsedLineReadOnlySpanSequenceWithoutDot1;
        private readonly ParsedLineReadOnlySpanSequenceWithoutDot parsedLineReadOnlySpanSequenceWithoutDot2;

        private readonly ParsedLineString parsedLineString1;
        private readonly ParsedLineString parsedLineString2;

        private readonly ParsedLine parsedLineSorter1;
        private readonly ParsedLine parsedLineSorter2;

        public CompareToImplementations()
        {
            parsedLineReadOnlyMemory1 = new ParsedLineReadOnlyMemory(string1);
            parsedLineReadOnlyMemory2 = new ParsedLineReadOnlyMemory(string2);

            parsedLineReadOnlySpanOrdinal1 = new ParsedLineReadOnlySpanOrdinal(string1);
            parsedLineReadOnlySpanOrdinal2 = new ParsedLineReadOnlySpanOrdinal(string2);

            parsedLineReadOnlySpanSequence1 = new ParsedLineReadOnlySpanSequence(string1);
            parsedLineReadOnlySpanSequence2 = new ParsedLineReadOnlySpanSequence(string2);

            parsedLineReadOnlySpanSequenceWithoutDot1 = new ParsedLineReadOnlySpanSequenceWithoutDot(string1);
            parsedLineReadOnlySpanSequenceWithoutDot2 = new ParsedLineReadOnlySpanSequenceWithoutDot(string2);

            parsedLineString1 = new ParsedLineString(string1);
            parsedLineString2 = new ParsedLineString(string2);

            parsedLineSorter1 = new ParsedLine(string1);
            parsedLineSorter2 = new ParsedLine(string2);
        }

        [Benchmark]
        public int ReadOnlyMemory() => parsedLineReadOnlyMemory1.CompareTo(parsedLineReadOnlyMemory2);

        [Benchmark]
        public int ReadOnlySpanOrdinal() => parsedLineReadOnlySpanOrdinal1.CompareTo(parsedLineReadOnlySpanOrdinal2);

        [Benchmark]
        public int ReadOnlySpanSequence() => parsedLineReadOnlySpanSequence1.CompareTo(parsedLineReadOnlySpanSequence2);

        [Benchmark]
        public int StringCompare() => parsedLineString1.CompareTo(parsedLineString2);

        [Benchmark(Baseline = true)]
        public int SorterCompare() => parsedLineSorter1.CompareTo(parsedLineSorter2);

        [Benchmark]
        public int ReadOnlySpanSequenceWithoutDot() => parsedLineReadOnlySpanSequenceWithoutDot1.CompareTo(parsedLineReadOnlySpanSequenceWithoutDot2);

        internal readonly struct ParsedLineReadOnlyMemory : IComparable<ParsedLineReadOnlyMemory>
        {
            private readonly ReadOnlyMemory<char> Phrase { get; }

            private readonly int dotPosition;
            public readonly string Line;

            public ParsedLineReadOnlyMemory(string line)
            {
                Line = line;
                dotPosition = line.IndexOf('.');
                Phrase = line.AsMemory(dotPosition + 2);
            }

            public readonly int CompareTo(ParsedLineReadOnlyMemory other)
            {
                var stringComparisionResult = Phrase.Span.CompareTo(other.Phrase.Span, StringComparison.Ordinal);

                if (stringComparisionResult != 0)
                {
                    return stringComparisionResult;
                }

                return int.Parse(Line.AsSpan(0, dotPosition)).CompareTo(int.Parse(other.Line.AsSpan(0, other.dotPosition)));
            }
        }

        internal readonly struct ParsedLineReadOnlySpanOrdinal(string line) : IComparable<ParsedLineReadOnlySpanOrdinal>
        {
            private readonly int dotPosition = line.IndexOf('.');
            public readonly string Line = line;

            public readonly int CompareTo(ParsedLineReadOnlySpanOrdinal other)
            {
                var stringComparisionResult = Line.AsSpan(dotPosition + 2)
                                                  .CompareTo(other.Line.AsSpan(other.dotPosition + 2), StringComparison.Ordinal);

                if (stringComparisionResult != 0)
                {
                    return stringComparisionResult;
                }

                return int.Parse(Line.AsSpan(0, dotPosition)).CompareTo(int.Parse(other.Line.AsSpan(0, other.dotPosition)));
            }
        }

        internal readonly struct ParsedLineReadOnlySpanSequence(string line) : IComparable<ParsedLineReadOnlySpanSequence>
        {
            private readonly int dotPosition = line.IndexOf('.');
            public readonly string Line = line;

            public readonly int CompareTo(ParsedLineReadOnlySpanSequence other)
            {
                var stringComparisionResult = Line.AsSpan(dotPosition + 2)
                                                  .SequenceCompareTo(other.Line.AsSpan(other.dotPosition + 2));

                if (stringComparisionResult != 0)
                {
                    return stringComparisionResult;
                }

                return int.Parse(Line.AsSpan(0, dotPosition)).CompareTo(int.Parse(other.Line.AsSpan(0, other.dotPosition)));
            }
        }

        internal readonly struct ParsedLineReadOnlySpanSequenceWithoutDot(string line) : IComparable<ParsedLineReadOnlySpanSequenceWithoutDot>
        {
            public readonly string Line = line;

            public readonly int CompareTo(ParsedLineReadOnlySpanSequenceWithoutDot other)
            {
                var dotPosition = Line.IndexOf('.');
                var otherDotPosition = other.Line.IndexOf('.');

                var stringComparisionResult = Line.AsSpan(dotPosition + 2)
                                                  .SequenceCompareTo(other.Line.AsSpan(otherDotPosition + 2));

                if (stringComparisionResult != 0)
                {
                    return stringComparisionResult;
                }

                return int.Parse(Line.AsSpan(0, dotPosition)).CompareTo(int.Parse(other.Line.AsSpan(0, otherDotPosition)));
            }
        }

        internal readonly struct ParsedLineString(string line) : IComparable<ParsedLineString>
        {
            private readonly int dotPosition = line.IndexOf('.');
            public readonly string Line = line;

            public readonly int CompareTo(ParsedLineString other)
            {
                var stringComparisionResult = string.CompareOrdinal(Line, dotPosition + 2, other.Line, other.dotPosition + 2, int.MaxValue);

                if (stringComparisionResult != 0)
                {
                    return stringComparisionResult;
                }

                return int.Parse(Line.AsSpan(0, dotPosition)).CompareTo(int.Parse(other.Line.AsSpan(0, other.dotPosition)));
            }
        }
    }
}
