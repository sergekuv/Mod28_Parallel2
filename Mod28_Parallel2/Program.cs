using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mod28_Parallel2;


internal class Program
{
    static void Main(string[] args)
    {
        int arrayLength = 1_000_000;
        int[] array = Enumerable.Range(1, arrayLength).Select (_ => Random.Shared.Next(0,10)).ToArray();
        Console.WriteLine($"Calculating sum for array containing {arrayLength:n0}");

        for (int iteration = 0; iteration < 5; iteration++)
        {
            Console.WriteLine($"\n== iteration {iteration} ==");

            Stopwatch sw = Stopwatch.StartNew();
            long sum = SerialForSum(array, 0, arrayLength);
            sw.Stop();
            Console.WriteLine($"Serial for \tSum = \t{sum:n0} \tcalculations took \t{sw.ElapsedTicks:n0}\t ticks");


            sw = Stopwatch.StartNew();
            sum = SerialLinqSum(array);
            sw.Stop();
            Console.WriteLine($"Serial Linq \tSum = \t{sum:n0} \tcalculations took \t{sw.ElapsedTicks:n0}\t ticks");

            sw = Stopwatch.StartNew();
            sum = ParallelLinqSum(array);
            sw.Stop();
            Console.WriteLine($"Parallel Linq \tSum = \t{sum:n0} \tcalculations took \t{sw.ElapsedTicks:n0}\t ticks");


            sw = Stopwatch.StartNew();
            long sum1 = 0, sum2 = 0, sum3 = 0, sum4 = 0;
            Parallel.Invoke(
                () => { sum1 = SerialForSum(array, 0, arrayLength / 4); },
                () => { sum2 = SerialForSum(array, arrayLength / 4, arrayLength / 2); },
                () => { sum3 = SerialForSum(array, arrayLength / 2, arrayLength - arrayLength / 4); },
                () => { sum4 = SerialForSum(array, arrayLength - arrayLength / 4, arrayLength); }
                );
            sum = sum1 + sum2 + sum3 + sum4;
            sw.Stop();
            Console.WriteLine($"Parallel Invoke 4\tSum = \t{sum:n0} \tcalculations took \t{sw.ElapsedTicks:n0}\t ticks");

            sw = Stopwatch.StartNew();
            sum1 = sum2 = 0;
            Parallel.Invoke(
                () => { sum1 = SerialForSum(array, 0, arrayLength / 2); },
                () => { sum2 = SerialForSum(array, arrayLength / 2, arrayLength); }
                );
            sum = sum1 + sum2;
            sw.Stop();
            Console.WriteLine($"Parallel Invoke 2\tSum = \t{sum:n0} \tcalculations took \t{sw.ElapsedTicks:n0}\t ticks");


            sum = 0;
            sw = Stopwatch.StartNew();
            int cores = 4;
            List<Task> tasks = new List<Task>();
            var sums = new ConcurrentBag<long>();
            for (int i = 0; i < cores; i++)
            {
                int startIndex = i * arrayLength / cores;
                int endIndex = startIndex + arrayLength / cores;
                Task task = Task.Run(() =>
                {
                    sums.Add(SerialForSum(array, startIndex, endIndex));
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());

            foreach (long item in sums)
            {
                sum += item;
            }
            sw.Stop();
            Console.WriteLine($"List of Tasks {cores} cores \tSum = \t{sum:n0} \tcalculations took \t{sw.ElapsedTicks:n0}\t ticks");


            sum = 0;
            sw = Stopwatch.StartNew();
            cores = 2;
            tasks = new List<Task>();
            sums = new ConcurrentBag<long>();
            for (int i = 0; i < cores; i++)
            {
                int startIndex = i * arrayLength / cores;
                int endIndex = startIndex + arrayLength / cores;
                Task task = Task.Run(() =>
                {
                    sums.Add(SerialForSum(array, startIndex, endIndex));
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());

            foreach (long item in sums)
            {
                sum += item;
            }
            sw.Stop();
            Console.WriteLine($"List of Tasks {cores} cores\tSum = \t{sum:n0} \tcalculations took \t{sw.ElapsedTicks:n0}\t ticks");

        }
    }

    static long SerialLinqSum(int[] array) => array.Sum();

    static long ParallelLinqSum(int[] array) => array.AsParallel().Sum();

    static long SerialForSum(int[] array, int startIndex, int endIndex)
    {
        long sum = 0;
        for(int i = startIndex; i < endIndex; i++)
        {
            sum += array[i];
        }
        return sum;
    }
}