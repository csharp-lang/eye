using System;
using System.Collections.Generic;
using System.IO;

namespace Validator
{
    internal class Program
    {
        private const string PathToWorkDir = @"..\..\..\..\..\..\WorkDir";
        private const string DefaultFileName = @"sample_output.txt";

        static void Main(string[] args)
        {
            var fileName = args.Length != 0 ? args[0] : DefaultFileName;
            var path = Path.Combine(PathToWorkDir, fileName);

            var lines = File.ReadLines(path);
            Validator.Validate(lines);
            Console.WriteLine("The file is valid.");
        }
    }

    internal static class Validator
    {
        public static void Validate(IEnumerable<string> lines)
        {
            ArgumentNullException.ThrowIfNull(nameof(lines));

            var result = 0;
            (int numberPart, string? stringPart) previousLine = (0, null);
            foreach (var currentLine in lines)
            {
                result++;
                if (previousLine.stringPart == null)
                {
                    previousLine = Parse(currentLine);
                    continue;
                }
                var (numberPart, stringPart) = Parse(currentLine);

                if (string.Compare(previousLine.stringPart, stringPart, StringComparison.Ordinal) > 0
                   || (string.Compare(previousLine.stringPart, stringPart, StringComparison.Ordinal) == 0 && previousLine.numberPart > numberPart))
                {
                    throw new ApplicationException($"Sorting is wrong. The number of erroneous line is {result}");
                }

                previousLine.numberPart = numberPart;
                previousLine.stringPart = stringPart;
            }
        }

        private static (int numberPart, string stringPart) Parse(string line)
        {
            var dot = line.IndexOf('.');
            var numberPart = int.Parse(line[..dot]);
            var stringPart = line[(dot + 2)..];
            return (numberPart, stringPart);
        }
    }
}
