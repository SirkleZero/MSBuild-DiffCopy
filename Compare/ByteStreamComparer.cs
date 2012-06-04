using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compare.Extensions;

namespace Compare
{
    public class ByteStreamComparer : IDirectoryComparer
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

            var bufferSize = 1024 * 4;
            byte[] sourceBytes;
            byte[] destinationBytes;

            // compare the overlap files
            foreach (var file in overlapFiles)
            {
                using (var sourceFile = new FileStream(string.Concat(source, file), FileMode.Open, FileAccess.Read))
                {
                    using (var destinationFile = new FileStream(string.Concat(destination, file), FileMode.Open, FileAccess.Read))
                    {
                        if (sourceFile.Length.Equals(destinationFile.Length))
                        {
                            // if they are the same size, compare them.
                            var sourceReader = new BinaryReader(sourceFile);
                            var destinationReader = new BinaryReader(destinationFile);
                            do
                            {
                                sourceBytes = sourceReader.ReadBytes(bufferSize);
                                destinationBytes = destinationReader.ReadBytes(bufferSize);

                                if (sourceBytes.Length > 0)
                                {
                                    // compare the byte arrays
                                    if (!sourceBytes.ByteArrayCompare(destinationBytes))
                                    {
                                        // they aren't equal arrays of bytes. The files are different.
                                        list.Add(string.Concat(source, file));
                                        break;
                                    }
                                }
                            } while (sourceBytes.Length > 0);
                        }
                        else
                        {
                            // if they aren't the same size, they are different. add to output.
                            list.Add(string.Concat(source, file));
                        }
                    }
                }
            }
            return list;
        }

        #endregion
    }
}
