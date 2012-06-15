using System;
using System.Collections.Generic;
using System.IO;
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
                this.HandleNewFiles(result);
                this.HandleModifiedFiles(result);
                this.HandleNotInSourceFiles(result);
            }
            catch(Exception e)
            {
                base.Log.LogError(string.Format("The build task failed. The message was '{0}'", e.Message));
                return false;
            }

            return true;
        }

        private void HandleNewFiles(ComparisonResult result)
        {
            var newFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "New Files", null);
            foreach (var file in result.NewFiles)
            {
                base.Log.LogMessage(MessageImportance.Normal, "\t{0}", file);
                newFiles.Add(new TaskItem(file));
            }
            this.NewFiles = newFiles.ToArray();
        }

        private void HandleModifiedFiles(ComparisonResult result)
        {
            var modifiedFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "Modified Files", null);
            foreach (var file in result.ModifiedFiles)
            {
                base.Log.LogMessage(MessageImportance.Normal, "\t{0}", file);
                modifiedFiles.Add(new TaskItem(file));
            }
            this.ModifiedFiles = modifiedFiles.ToArray();
        }

        private void HandleNotInSourceFiles(ComparisonResult result)
        {
            var notInSourceFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "Files that don't exist in destination", null);
            foreach (var file in result.NotInSource)
            {
                base.Log.LogMessage(MessageImportance.Normal, "\t{0}", file);
                notInSourceFiles.Add(new TaskItem(file));
            }
            this.NotInSourceFiles = notInSourceFiles.ToArray();
        }
    }
}
