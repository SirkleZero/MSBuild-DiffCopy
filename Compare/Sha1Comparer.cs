using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Compare
{
    public class Sha1Comparer : IDirectoryComparer
    {
        #region IDirectoryComparer Members

        public IEnumerable<string> Compare(string source, string destination)
        {
            var list = new List<string>();

            var sourceFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
            var destinationFiles = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories);

            // need to strip the root directories from the file lists above. This will give us relative paths for comparison.
            var strippedSourceFiles = sourceFiles.Select(m => m = m.Replace(source, string.Empty));
            var strippedDestinationFiles = destinationFiles.Select(m => m = m.Replace(destination, string.Empty));

            // these are the files that need to be compared. Anything from inside source that doesn't exist in here
            // needs to be copied, anything from destination could be deleted. Perhaps a "prune" argument.
            var overlapFiles = strippedSourceFiles.Intersect(strippedDestinationFiles);

            // anything not overlapping needs to be added.
            var newFiles = strippedSourceFiles.Except(strippedDestinationFiles);
            list.AddRange(newFiles.Select(m => m = string.Concat(source, m)));

            // compare the overlap files
            foreach (var file in overlapFiles)
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

        #endregion
    }
}
