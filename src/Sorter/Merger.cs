using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sorter
{
    internal static class Merger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> Merge(IEnumerable<IEnumerable<ParsedLine>> inputs)
        {
            var enumerators = inputs.Select(e => e.GetEnumerator()).ToArray();

            if (enumerators.Length > 4)
            {
                return MergeWithPriorityQueue(enumerators);
            }
            else
            {
                return SimplyMerge(enumerators);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<string> SimplyMerge(IEnumerator<ParsedLine>[] enumerators)
        {
            var validEnumeratorsCount = enumerators.Length;

            for (var i = 0; i < validEnumeratorsCount; i++)
            {
                if (!enumerators[i].MoveNext())
                {
                    enumerators[i].Dispose();
                    if (i != validEnumeratorsCount - 1)
                    {
                        enumerators[i] = enumerators[validEnumeratorsCount - 1];
                        i--; //process ith enumerator again
                    }
                    validEnumeratorsCount--;
                }
            }

            while (validEnumeratorsCount > 1)
            {
                ParsedLine min = enumerators[0].Current;
                var minIndex = 0;

                for (var i = 1; i < validEnumeratorsCount; i++)
                {
                    if (min.CompareTo(enumerators[i].Current) > 0)
                    {
                        min = enumerators[i].Current;
                        minIndex = i;
                    }
                }

                //return min element
                yield return min.Line;

                if (!enumerators[minIndex].MoveNext())
                {
                    enumerators[minIndex].Dispose();
                    if (minIndex != validEnumeratorsCount - 1)
                    {
                        enumerators[minIndex] = enumerators[validEnumeratorsCount - 1];
                    }
                    validEnumeratorsCount--;
                }
            }

            yield return enumerators[0].Current.Line;
            while (enumerators[0].MoveNext())
            {
                yield return enumerators[0].Current.Line;
            }

            enumerators[0].Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<string> MergeWithPriorityQueue(IEnumerator<ParsedLine>[] enumerators)
        {
            var queue = new PriorityQueue<int, ParsedLine>();

            for (var i = 0; i < enumerators.Length; i++)
            {
                if (enumerators[i].MoveNext())
                {
                    queue.Enqueue(i, enumerators[i].Current);
                }
                else
                {
                    enumerators[i].Dispose();
                }
            }

            var minIndex = queue.Dequeue();
            while (queue.Count > 0)
            {
                var min = enumerators[minIndex].Current;
                //return min element
                yield return min.Line;

                if (enumerators[minIndex].MoveNext())
                {
                    minIndex = queue.EnqueueDequeue(minIndex, enumerators[minIndex].Current);
                }
                else
                {
                    enumerators[minIndex].Dispose();
                    minIndex = queue.Dequeue();
                }
            }

            yield return enumerators[minIndex].Current.Line;

            while (enumerators[minIndex].MoveNext())
            {
                yield return enumerators[minIndex].Current.Line;
            }

            enumerators[minIndex].Dispose();
        }
    }
}
