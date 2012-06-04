using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compare
{
    public abstract class DirectoryComparer : IDirectoryComparer
    {
        public DirectoryComparer() { }

        protected IEnumerable<string> SourceFiles { get; private set; }
        protected IEnumerable<string> DestinationFiles { get; private set; }
        protected IEnumerable<string> FilesToCompare { get; private set; }
        protected IEnumerable<string> NewFiles { get; private set; }
        protected IEnumerable<string> FilesToDelete { get; private set; }

        protected void Scan(string source, string destination, bool pruneDestination)
        {
            this.SourceFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
            this.DestinationFiles = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories);

            // need to strip the root directories from the file lists above. This will give us relative paths for comparison.
            var strippedSourceFiles = this.SourceFiles.Select(m => m = m.Replace(source, string.Empty));
            var strippedDestinationFiles = this.DestinationFiles.Select(m => m = m.Replace(destination, string.Empty));

            // these are the files that need to be compared. Anything from inside source that doesn't exist in here
            // needs to be copied, anything from destination could be deleted. Perhaps a "prune" argument.
            this.FilesToCompare = strippedSourceFiles.Intersect(strippedDestinationFiles);

            // anything not overlapping needs to be added.
            var newFiles = strippedSourceFiles.Except(strippedDestinationFiles);
            this.NewFiles = newFiles.Select(m => m = string.Concat(source, m));

            // anything in the destination directory that isn't in the source directory. This prunes files from
            // the destination that don't exist in the source.
            if (pruneDestination)
            {
                var toDeleteFiles = strippedDestinationFiles.Except(strippedSourceFiles);
                this.FilesToDelete = toDeleteFiles.Select(m => m = string.Concat(destination, m));
            }
        }

        #region IDirectoryComparer Members

        public abstract IEnumerable<string> Compare(string source, string destination);

        public abstract IEnumerable<string> Compare(string source, string destination, bool pruneDestination);

        #endregion
    }
}
