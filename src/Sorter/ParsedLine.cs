using System;

namespace Sorter
{
    internal readonly struct ParsedLine(string line) : IComparable<ParsedLine>
    {
        private readonly int dotPosition = line.IndexOf('.');
        public readonly string Line = line;

        public int CompareTo(ParsedLine other)
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

}
