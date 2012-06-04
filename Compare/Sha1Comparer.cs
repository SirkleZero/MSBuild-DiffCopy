using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Compare
{
    public class Sha1Comparer : DirectoryComparer
    {
        public Sha1Comparer() { }

        public override IEnumerable<string> Compare(string source, string destination)
        {
            return this.Compare(source, destination, false);
        }

        public override IEnumerable<string> Compare(string source, string destination, bool pruneDestination)
        {
            base.Scan(source, destination, pruneDestination);

            var list = new List<string>();
            list.AddRange(base.NewFiles.Select(m => m = string.Concat(source, m)));

            // compare the overlap files
            foreach (var file in base.FilesToCompare)
            {
                using (var sourceFile = new FileStream(string.Concat(source, file), FileMode.Open, FileAccess.Read))
                {
                    using (var destinationFile = new FileStream(string.Concat(destination, file), FileMode.Open, FileAccess.Read))
                    {
                        using (var cryptoProvider = new SHA1CryptoServiceProvider())
                        {
                            string sourceHash = BitConverter.ToString(cryptoProvider.ComputeHash(sourceFile));
                            string destinationHash = BitConverter.ToString(cryptoProvider.ComputeHash(destinationFile));
                            if (!sourceHash.Equals(destinationHash, StringComparison.OrdinalIgnoreCase))
                            {
                                list.Add(string.Concat(source, file));
                            }
                        }
                    }
                }
            }

            return list;
        }
    }
}
