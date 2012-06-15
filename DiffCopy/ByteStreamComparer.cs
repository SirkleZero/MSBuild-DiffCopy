using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiffCopy.Extensions;

namespace DiffCopy
{
    public class ByteStreamComparer : DirectoryComparer
    {
        private const int BufferSize = 4096;

        public ByteStreamComparer() { }

        public override ComparisonResult Compare(string source, string destination)
        {
            base.Scan(source, destination);

            var modifiedFiles = new List<string>();

            byte[] sourceBytes;
            byte[] destinationBytes;
            BinaryReader sourceReader = null;
            BinaryReader destinationReader = null;

            // compare the overlap files
            foreach (var file in base.FilesToCompare)
            {
                using (var sourceFile = new FileStream(string.Concat(source, file), FileMode.Open, FileAccess.Read))
                {
                    using (var destinationFile = new FileStream(string.Concat(destination, file), FileMode.Open, FileAccess.Read))
                    {
                        if (sourceFile.Length.Equals(destinationFile.Length))
                        {
                            // if they are the same size, compare them.
                            sourceReader = new BinaryReader(sourceFile);
                            destinationReader = new BinaryReader(destinationFile);
                            do
                            {
                                sourceBytes = sourceReader.ReadBytes(ByteStreamComparer.BufferSize);
                                destinationBytes = destinationReader.ReadBytes(ByteStreamComparer.BufferSize);

                                if (sourceBytes.Length > 0)
                                {
                                    // if the arrays of bytes aren't equal, then the files are different. add them to the return set.
                                    if (!sourceBytes.ByteArrayCompare(destinationBytes))
                                    {
                                        modifiedFiles.Add(string.Concat(source, file));
                                        break;
                                    }
                                }
                            } while (sourceBytes.Length > 0);
                        }
                        else
                        {
                            // if the files aren't the same size, obviously they are different. add them to the return set.
                            modifiedFiles.Add(string.Concat(source, file));
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
