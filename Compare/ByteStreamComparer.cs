using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compare.Extensions;

namespace Compare
{
    public class ByteStreamComparer : DirectoryComparer
    {
        private const int BufferSize = 4096;

        public ByteStreamComparer() { }

        public override IEnumerable<string> Compare(string source, string destination)
        {
            return this.Compare(source, destination, false);
        }

        public override IEnumerable<string> Compare(string source, string destination, bool pruneDestination)
        {
            base.Scan(source, destination, pruneDestination);

            var list = new List<string>();
            list.AddRange(base.NewFiles.Select(m => m = string.Concat(source, m)));

            byte[] sourceBytes;
            byte[] destinationBytes;

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
                            var sourceReader = new BinaryReader(sourceFile);
                            var destinationReader = new BinaryReader(destinationFile);
                            do
                            {
                                sourceBytes = sourceReader.ReadBytes(ByteStreamComparer.BufferSize);
                                destinationBytes = destinationReader.ReadBytes(ByteStreamComparer.BufferSize);

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
    }
}
