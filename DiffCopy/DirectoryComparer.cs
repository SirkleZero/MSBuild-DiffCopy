using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiffCopy
{
    public abstract class DirectoryComparer : IDirectoryComparer
    {
        public DirectoryComparer() { }

        protected IEnumerable<string> SourceFiles { get; private set; }
        protected IEnumerable<string> DestinationFiles { get; private set; }
        protected IEnumerable<string> FilesToCompare { get; private set; }
        protected IEnumerable<string> NewFiles { get; private set; }
        protected IEnumerable<string> FilesToDelete { get; private set; }

        protected void Scan(string source, string destination)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new DirectoryNotFoundException(string.Format("{0} was not found.", source));
            }
            if (string.IsNullOrEmpty(destination))
            {
                throw new DirectoryNotFoundException(string.Format("{0} was not found.", destination));
            }
            if (!Directory.Exists(source))
            {
                throw new DirectoryNotFoundException(string.Format("{0} was not found.", source));
            }
            if (!Directory.Exists(destination))
            {
                throw new DirectoryNotFoundException(string.Format("{0} was not found.", destination));
            }

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
            var toDeleteFiles = strippedDestinationFiles.Except(strippedSourceFiles);
            this.FilesToDelete = toDeleteFiles.Select(m => m = string.Concat(destination, m));
        }

        #region IDirectoryComparer Members

        /// <summary>
        /// Compares the file contents for two directories and returns information about the differences.
        /// </summary>
        /// <param name="source">The source directory to compare to the destination.</param>
        /// <param name="destination">The destination directory where, ultimately, files would be copied.</param>
        /// <returns>A <see cref="ComparisonResult"/> object that contains the result of the comparison operation.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public abstract ComparisonResult Compare(string source, string destination);

        #endregion
    }
}
