using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DiffCopy
{
    public class DiffCopyBuildTask : Task
    {
        public DiffCopyBuildTask() { }

        [Required]
        public string SourceDirectory { get; set; }
        [Required]
        public string DestinationDirectory { get; set; }

        [Output]
        public ITaskItem[] NewFiles { get; set; }
        [Output]
        public ITaskItem[] ModifiedFiles { get; set; }
        [Output]
        public ITaskItem[] NotInSourceFiles { get; set; }

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
    }
}
