using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DiffCopy
{
    /// <summary>
    /// A custom MSBuild Task object that is used to compare two directories for efficient deployment of large numbers of files.
    /// </summary>
    public class DiffCopyBuildTask : Task
    {
        #region constructors

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="DiffCopyBuildTask"/> class.</para>
        /// </summary>
        public DiffCopyBuildTask() { }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or Sets the source directory to compare to the destination directory. Must be an actual path on disk.
        /// </summary>
        [Required]
        public string SourceDirectory { get; set; }

        /// <summary>
        /// Gets or Sets the destination directory that the source directory will be compared to. Must be an actual path on disk.
        /// </summary>
        [Required]
        public string DestinationDirectory { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="ITaskItem[]"/> that contains the files that exist in the source but not the destination directory.
        /// </summary>
        [Output]
        public ITaskItem[] NewFiles { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="ITaskItem[]"/> that contains the files that exist in both the source and destination directories that are different.
        /// </summary>
        [Output]
        public ITaskItem[] ModifiedFiles { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="ITaskItem[]"/> that contains the files that exist in the destination, but not the source directory.
        /// </summary>
        [Output]
        public ITaskItem[] NotInSourceFiles { get; set; }

        #endregion

        #region public methods

        /// <summary>
        ///     Executes the Task.
        /// </summary>
        /// <returns>true if the task successfully executed; otherwise, false.</returns>
        public override bool Execute()
        {
            if (!Directory.Exists(this.SourceDirectory))
            {
                // can't run if the directory doesn't exist
                base.Log.LogError("The source directory specified does not exist.");
                return false;
            }
            if (!Directory.Exists(this.DestinationDirectory))
            {
                // can't run if the directory doesn't exist
                base.Log.LogError("The destination directory specified does not exist.");
                return false;
            }

            base.Log.LogMessage(MessageImportance.Normal, "Comparing {0} to {1}", this.SourceDirectory, this.DestinationDirectory);

            var comparer = new ByteStreamComparer();
            var result = comparer.Compare(this.SourceDirectory, this.DestinationDirectory);

            try
            {
                this.HandleNewFiles(result, this.SourceDirectory);
                this.HandleModifiedFiles(result, this.SourceDirectory);
                this.HandleNotInSourceFiles(result, this.DestinationDirectory);
            }
            catch (Exception e)
            {
                base.Log.LogError(string.Format("The build task failed. The message was '{0}'", e.StackTrace));
                return false;
            }

            return true;
        }

        #endregion

        #region private methods

        private void HandleNewFiles(ComparisonResult result, string rootPath)
        {
            var newFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "New Files", null);

            this.HandleFiles(result.NewFiles, newFiles, rootPath);

            this.NewFiles = newFiles.ToArray();
        }

        private void HandleModifiedFiles(ComparisonResult result, string rootPath)
        {
            var modifiedFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "Modified Files", null);

            this.HandleFiles(result.ModifiedFiles, modifiedFiles, rootPath);

            this.ModifiedFiles = modifiedFiles.ToArray();
        }

        private void HandleNotInSourceFiles(ComparisonResult result, string rootPath)
        {
            var notInSourceFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "Files that exist on destination but not source", null);

            this.HandleFiles(result.NotInSource, notInSourceFiles, rootPath);

            this.NotInSourceFiles = notInSourceFiles.ToArray();
        }

        private void HandleFiles(IEnumerable<string> results, List<ITaskItem> destination, string rootPath)
        {
            if (results.Count().Equals(0))
            {
                base.Log.LogMessage(MessageImportance.Normal, "\tNo files found.");
            }
            else
            {
                foreach (var file in results)
                {
                    base.Log.LogMessage(MessageImportance.Normal, "\t{0}", file);

                    var info = new FileInfo(file);
                    var modified = info.LastWriteTime;
                    var created = info.CreationTime;
                    var accessed = info.LastAccessTime;
                    var fullPath = Path.GetFullPath(file);

                    var metadata = new Dictionary<string, string>();
                    metadata.Add("FullPath", fullPath);
                    metadata.Add("RootDir", Path.GetPathRoot(file));
                    metadata.Add("Filename", Path.GetFileNameWithoutExtension(file));
                    metadata.Add("Extension", Path.GetExtension(file));
                    metadata.Add("ModifiedTime", modified.ToString("yyyy-MM-dd hh:mm:ss.FFFFFFF"));
                    metadata.Add("CreatedTime", created.ToString("yyyy-MM-dd hh:mm:ss.FFFFFFF"));
                    metadata.Add("AccessedTime", accessed.ToString("yyyy-MM-dd hh:mm:ss.FFFFFFF"));

                    var recursiveDirectory = file.Replace(rootPath, string.Empty).Replace(Path.GetFileName(file), string.Empty);
                    metadata.Add("RecursiveDir", recursiveDirectory);

                    // NOTE: The following well-known meta data aren't implemented at this time.
                    // RelativeDir, Directory, Identity

                    var item = new TaskItem(file, metadata);
                    destination.Add(item);
                }
            }
        }

        #endregion
    }
}
