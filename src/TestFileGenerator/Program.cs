using System;
using System.IO;

namespace TestFileGenerator
{
    internal class Program
    {
        private const int PrintOutput = 10_000_000;
        private const string FileName = @"..\..\..\..\..\..\WorkDir\sample.txt";

        private const decimal Fraction = 0.1M;
        private const int LineNumberWithTheSameString = (int)(1 / Fraction);
        private static readonly string TheSameString = Guid.NewGuid().ToString();

        static void Main(string[] args)
        {
            string? input;
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter number of lines in file.");
                input = Console.ReadLine();
            }
            else
            {
                input = args[0];
            }

            if (!long.TryParse(input, out var numberOfLines))
            {
                Console.WriteLine("Input is not valid number.");
                return;
            }

            using StreamWriter outputFile = File.CreateText(FileName);

            for (var i = 0; i < numberOfLines; i++)
            {
                var numberPart = Random.Shared.Next(int.MinValue, int.MaxValue);
                var stringPart = (i % LineNumberWithTheSameString) != 0
                    ? Guid.NewGuid().ToString()
                    : TheSameString;

                outputFile.Write(numberPart);
                outputFile.Write(". ");
                outputFile.WriteLine(stringPart);

                if ((i % PrintOutput) == 0)
                {
                    Console.WriteLine($"The {i + 1} lines were generated.");
                }
            }

            Console.WriteLine($"The {numberOfLines} lines were generated.");
        }
    }
}
