using System;
using System.Diagnostics;
using System.Linq;
using DiffCopy;

namespace Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = @"C:\Users\jgall\Documents\Projects\Research\Knitting";
            var destination = @"C:\Users\jgall\Documents\Projects\Research\Knitting_OLD";
            //var destination = @"C:\Users\jgall\Documents\Projects\tstat";

            Program.ListDifferences(source, destination);

            //Program.ComparePerformance(source, destination);
            
            Console.ReadLine();
        }

        private static void ListDifferences(string source, string destination)
        {
            var comparer = new ByteStreamComparer();
            var results = comparer.Compare(source, destination);

            foreach (var result in results.ModifiedFiles)
            {
                Console.WriteLine(result);
            }
        }

        private static void ComparePerformance(string source, string destination)
        {
            for (var i = 0; i <= 4; i++)
            {
                var watch = new Stopwatch();
                watch.Start();
                IDirectoryComparer comparer = new ByteStreamComparer();
                var byteStreamResults = comparer.Compare(source, destination);
                watch.Stop();
                Console.WriteLine(string.Format("Byte Stream Comparer found changes in {0}", watch.Elapsed));
                watch.Reset();

                watch.Start();
                comparer = new Sha1Comparer();
                var sha1Results = comparer.Compare(source, destination);
                watch.Stop();
                Console.WriteLine(string.Format("Sha1 Comparer found changes in {0}", watch.Elapsed));
            }
        }
    }
}
