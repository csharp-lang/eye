using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    internal class Program
    {
        private const int BytesInLine = 50; //To estimate number of lines in file

        private readonly static int MaxChunkSize = Array.MaxLength / BytesInLine / 4; // so every chunk is about 500MB
        private readonly static int MaxSortingTaskCount = Environment.ProcessorCount / 2; //reduce workers because of possibility of Hyper-threading

        private const string FileExtension = ".txt";
        private const string DefaultFileName = "sample";
        private const string ChunkFileName = "chunk_";
        private const string PathToWorkDir = @"..\..\..\..\..\..\WorkDir";

        static async Task Main(string[] args)
        {
            var inputFileName = args.Length != 0 ? args[0] : DefaultFileName;
            var outputFileName = $"{inputFileName}_output";

            Stopwatch stopwatch = Stopwatch.StartNew();
            var inputFile = GetFilePathToWorkDir(inputFileName);

            var numberOfLines = GetNumberOfLinesInFile(inputFile);
            var chunkSize = GetChunkSize(numberOfLines);
            var maxIndexOfTempFiles = -1; //to start with zero after incrementing
            using BlockingCollection<List<ParsedLine>> chunkQueue = new(MaxSortingTaskCount);
            {
                var tasks = new List<Task>(MaxSortingTaskCount + 1);
                tasks.Add(Task.Run(() => ReadChunks(inputFile, chunkSize, chunkQueue)));
                for (var i = 0; i < MaxSortingTaskCount; i++)
                {
                    tasks.Add(Task.Run(() => SortChunks(chunkQueue, ref maxIndexOfTempFiles)));
                }

                await Task.WhenAll(tasks);
            }

            MergeChunkFilesIntoOutputFile(outputFileName, maxIndexOfTempFiles);
            stopwatch.Stop();
            Console.WriteLine($"The file was sorted in {stopwatch.Elapsed}.");
            DeleteChunkFiles(maxIndexOfTempFiles);
        }
        private static void ReadChunks(
            string inputFileName,
            int chunkSize,
            BlockingCollection<List<ParsedLine>> chunkQueue)
        {
            try
            {
                var chunk = new List<ParsedLine>(chunkSize);

                foreach (var line in File.ReadLines(inputFileName))
                {
                    chunk.Add(new ParsedLine(line));

                    if (chunk.Count == chunkSize)
                    {
                        chunkQueue.Add(chunk);
                        chunk = new List<ParsedLine>(chunkSize);
                    }
                }

                if (chunk.Count > 0)
                {
                    chunkQueue.Add(chunk); // Add the last chunk
                }
            }
            finally
            {
                chunkQueue.CompleteAdding();
            }
        }

        private static void SortChunks(BlockingCollection<List<ParsedLine>> chunkQueue, ref int numberOfTempFiles)
        {
            foreach (var chunk in chunkQueue.GetConsumingEnumerable())
            {
                chunk.Sort();
                var lines = chunk.Select(pl => pl.Line);
                var currentFile = Interlocked.Increment(ref numberOfTempFiles);
                var fileName = $"{ChunkFileName}{currentFile}";

                WriteToFile(fileName, lines);
            }
        }

        private static void MergeChunkFilesIntoOutputFile(
            string outputFileName,
            int maxIndexOfTempFiles)
        {
            var enumerators = new List<IEnumerable<ParsedLine>>(maxIndexOfTempFiles);
            for (int j = 0; j <= maxIndexOfTempFiles; j++)
            {
                var fileName = GetFilePathToWorkDir($"{ChunkFileName}{j}");
                enumerators.Add(File.ReadLines(fileName).Select(s => new ParsedLine(s)));
            }

            var merged = Merger.Merge(enumerators);
            WriteToFile(outputFileName, merged);
        }

        #region File helpers

        private static void WriteToFile(string fileName, IEnumerable<string> lines)
        {
            var outputFile = GetFilePathToWorkDir(fileName);
            File.WriteAllLines(outputFile, lines);
        }
        private static void DeleteChunkFiles(int maxIndexOfTempFiles)
        {
            for (int j = 0; j <= maxIndexOfTempFiles; j++)
            {
                var fileName = GetFilePathToWorkDir($"{ChunkFileName}{j}");
                File.Delete(fileName);
            }
        }
        private static string GetFilePathToWorkDir(string fileName) => Path.ChangeExtension(Path.Combine(PathToWorkDir, fileName), FileExtension);

        #endregion

        #region Helpers
        private static long GetNumberOfLinesInFile(string path)
        {
            var fileSize = new FileInfo(path).Length;
            return fileSize / BytesInLine;
        }

        private static int GetChunkSize(long numberOfLines)
        {
            var numberOfChunks = (numberOfLines / MaxChunkSize) + 1; //Ceiling
            return (int)(numberOfLines / numberOfChunks);
        }

        #endregion
    }
}