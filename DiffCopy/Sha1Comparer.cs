using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DiffCopy
{
    public class Sha1Comparer : DirectoryComparer
    {
        public Sha1Comparer() { }

        public override ComparisonResult Compare(string source, string destination)
        {
            base.Scan(source, destination);

            var modifiedFiles = new List<string>();
            string sourceHash = null;
            string destinationHash = null;

            // compare the overlap files
            foreach (var file in base.FilesToCompare)
            {
                using (var sourceFile = new FileStream(string.Concat(source, file), FileMode.Open, FileAccess.Read))
                {
                    using (var destinationFile = new FileStream(string.Concat(destination, file), FileMode.Open, FileAccess.Read))
                    {
                        using (var cryptoProvider = new SHA1CryptoServiceProvider())
                        {
                            sourceHash = BitConverter.ToString(cryptoProvider.ComputeHash(sourceFile));
                            destinationHash = BitConverter.ToString(cryptoProvider.ComputeHash(destinationFile));

                            // if the hashes don't match, the files are different. add them to the return set.
                            if (!sourceHash.Equals(destinationHash, StringComparison.OrdinalIgnoreCase))
                            {
                                modifiedFiles.Add(string.Concat(source, file));
                            }
                        }
                    }
                }
            }

            var newFiles = new List<string>(base.NewFiles);
            var notInSourceFiles = new List<string>(base.FilesToDelete);
            var result = new ComparisonResult(newFiles, modifiedFiles, notInSourceFiles);

            return result;
        }
    }
}
