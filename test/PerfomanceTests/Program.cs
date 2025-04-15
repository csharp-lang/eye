using BenchmarkDotNet.Running;

namespace PerfomanceTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CompareToImplementations>();
        }
    }
}
