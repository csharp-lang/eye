using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Sorter
{
    internal class Program
    {
        private const int BytesInLine = 50; //To estimate number of lines in file

        private readonly static int MaxChunkSize = Array.MaxLength / BytesInLine / 4; // so every chunk is about 2GB
        private readonly static int MaxTaskCount = Environment.ProcessorCount / 2; //reduce workers because of possibility of Hyper-threading

        private const string FileExtension = ".txt";
        private const string DefaultFileName = "sample";
        private const string ChunkFileName = "chunk_";
        private const string PathToWorkDir = @"..\..\..\..\..\..\WorkDir";

        private const bool WithCompression = false; //compression does not improve results on my computer

        static async Task Main(string[] args)
        {
            var inputFileName = args.Length != 0 ? args[0] : DefaultFileName;
            var outputFileName = $"{inputFileName}_output";

            Stopwatch stopwatch = Stopwatch.StartNew();
            var inputFile = GetFilePathToWorkDir(inputFileName);

            var numberOfLines = GetNumberOfLinesInFile(inputFile);
            var chunkSize = GetChunkSize(numberOfLines);

            var sortingTasks = new Task[MaxTaskCount];
            Array.Fill(sortingTasks, Task.CompletedTask);
            var chunks = new ParsedLine[MaxTaskCount][];

            var (numberOfTempFiles, lastChunk) = await SortAndStoreToTempFiles(inputFile, chunks, sortingTasks, chunkSize);
            MergeChunkFilesIntoOutputFile(outputFileName, numberOfTempFiles, lastChunk, false);

            stopwatch.Stop();
            Console.WriteLine($"The file was sorted in {stopwatch.Elapsed}.");
            //DeleteChunkFiles(numberOfTempFiles);
        }

        #region Sort
        private static void Sort(ParsedLine[] chunk, int realNumberOfElements) => Array.Sort(chunk, 0, realNumberOfElements);

        private static async Task<(int numberOfTempFiles, IEnumerable<IEnumerable<ParsedLine>> lastChunk)> SortAndStoreToTempFiles(
            string inputFileName,
            ParsedLine[][] chunks,
            Task[] sortingTasks,
            int chunkSize)
        {
            var fileNamesIndexer = 0;

            var chunkIndex = 0;
            var chunk = new ParsedLine[chunkSize];
            chunks[chunkIndex] = chunk;

            int i = 0;
            foreach (var line in File.ReadLines(inputFileName))
            {
                chunk[i++] = new ParsedLine(line);

                if (i == chunkSize)
                {
                    var chunkNumberCopy = chunkIndex;
                    var sortTask = Task.Factory.StartNew(() => Sort(chunks[chunkNumberCopy], chunkSize));
                    sortingTasks[chunkIndex] = sortTask;

                    i = 0;
                    chunkIndex++;

                    if (chunkIndex == MaxTaskCount)
                    {
                        //Waiting for all task and no start new sorting
                        await Task.WhenAll(sortingTasks);
                        MergeAndWriteToFile($"{ChunkFileName}{fileNamesIndexer++}", chunks, WithCompression);

                        chunkIndex = 0;
                        chunk = chunks[chunkIndex];
                    }
                    else
                    {
                        //Create new chunk if needed
                        chunk = chunks[chunkIndex] ??= new ParsedLine[chunkSize];
                    }
                }
            }

            //Start last task
            var lastSortTask = Task.Factory.StartNew(() => Sort(chunks[chunkIndex], i));
            sortingTasks[chunkIndex] = lastSortTask;

            await Task.WhenAll(sortingTasks);

            var lastChunk = FixLastChunksToMerge(chunks, chunkIndex, i);

            return (fileNamesIndexer, lastChunk);
        }

        #endregion

        private static IEnumerable<IEnumerable<ParsedLine>> FixLastChunksToMerge(
            ParsedLine[][] chunks,
            int indexOflastChunkWithElements,
            int realNumberOfElementsInLastChunk)
        {
            var fixedLastChunk = chunks[indexOflastChunkWithElements].Take(realNumberOfElementsInLastChunk);
            var toMerge = chunks.Take(indexOflastChunkWithElements).Concat(Enumerable.Repeat(fixedLastChunk, 1));

            return toMerge;
        }

        //Do not store last chunks in file; merge them from memory
        private static void MergeChunkFilesIntoOutputFile(
            string outputFileName,
            int numberOfChunkFiles,
            IEnumerable<IEnumerable<ParsedLine>> lastChunk,
            bool withCompression)
        {
            //Chunks from files
            var enumerators = new List<IEnumerable<ParsedLine>>(numberOfChunkFiles);
            for (int j = 0; j < numberOfChunkFiles; j++)
            {
                var fileName = GetFilePathToWorkDir($"{ChunkFileName}{j}");

                if (withCompression)
                {
                    enumerators.Add(ReadCompressedFiles(fileName));
                }
                else
                {
                    enumerators.Add(File.ReadLines(fileName).Select(s => new ParsedLine(s)));
                }
            }

            enumerators.AddRange(lastChunk);

            MergeAndWriteToFile(outputFileName, enumerators, withCompression);
        }
        private static void MergeAndWriteToFile(
            string fileName,
            IEnumerable<IEnumerable<ParsedLine>> toMerge,
            bool withCompression = false)
        {
            var all = Merger.Merge(toMerge);
            var outputFile = Path.ChangeExtension(Path.Combine(PathToWorkDir, fileName), FileExtension);
            WriteToFile(outputFile, all, withCompression);
        }


        // just to compare with PLINQ
        private static IEnumerable<string> PLINQ_Sorting(string inputFile)
        {
            return File.ReadLines(inputFile)
                       .AsParallel()
                       .Select(x => new ParsedLine(x))
                       .Select(l => l.Line)
                       .AsSequential();
        }

        #region File helpers
        private static IEnumerable<ParsedLine> ReadCompressedFiles(string fileName)
        {
            using var fileStream = File.OpenRead(fileName);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
            using var textStream = new StreamReader(deflateStream);

            string? line;
            while ((line = textStream.ReadLine()) != null)
            {
                yield return new ParsedLine(line);
            }
        }
        private static void WriteToFile(string fileName, IEnumerable<string> lines, bool withCompression)
        {
            if (!withCompression)
            {
                File.WriteAllLines(fileName, lines);
                return;
            }

            using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            using var deflateStream = new DeflateStream(fileStream, CompressionLevel.Fastest);
            using var textStream = new StreamWriter(deflateStream);

            foreach (var line in lines)
            {
                textStream.WriteLine(line);
            }
        }
        private static void DeleteChunkFiles(int numberOfChunkFiles)
        {
            for (int j = 0; j < numberOfChunkFiles; j++)
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