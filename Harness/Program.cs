using System;
using System.Diagnostics;
using System.Linq;
using Compare;

namespace Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = @"C:\Users\jgall\Documents\Projects\Research\Knitting";
            var destination = @"C:\Users\jgall\Documents\Projects\Research\Knitting_OLD";
            //var destination = @"C:\Users\jgall\Documents\Projects\tstat";

            for (var i = 0; i <= 4; i++)
            {
                var watch = new Stopwatch();
                watch.Start();
                IDirectoryComparer comparer = new ByteStreamComparer();
                var byteStreamResults = comparer.Compare(source, destination);
                watch.Stop();
                Console.WriteLine(string.Format("Byte Stream Comparer found {0} changes in {1}", byteStreamResults.Count(), watch.Elapsed));
                watch.Reset();

                watch.Start();
                comparer = new Sha1Comparer();
                var sha1Results = comparer.Compare(source, destination);
                watch.Stop();
                Console.WriteLine(string.Format("Sha1 Comparer found {0} changes in {1}", sha1Results.Count(), watch.Elapsed));
            }
            
            Console.ReadLine();
        }
    }
}
