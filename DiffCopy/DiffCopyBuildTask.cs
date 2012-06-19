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
            base.Log.LogMessage(MessageImportance.High, "New Files");

            this.HandleFiles(result.NewFiles, newFiles);

            this.NewFiles = newFiles.ToArray();
        }

        private void HandleModifiedFiles(ComparisonResult result)
        {
            var modifiedFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "Modified Files");

            this.HandleFiles(result.ModifiedFiles, modifiedFiles);

            this.ModifiedFiles = modifiedFiles.ToArray();
        }

        private void HandleNotInSourceFiles(ComparisonResult result)
        {
            var notInSourceFiles = new List<ITaskItem>();
            base.Log.LogMessage(MessageImportance.High, "Files that exist on destination but not source");

            this.HandleFiles(result.NotInSource, notInSourceFiles);

            this.NotInSourceFiles = notInSourceFiles.ToArray();
        }

        private void HandleFiles(IEnumerable<string> results, List<ITaskItem> destination)
        {
            if (results.Count().Equals(0))
            {
                base.Log.LogMessage(MessageImportance.Normal, "No files found.");
            }
            else
            {
                foreach (var file in results)
                {
                    base.Log.LogMessage(MessageImportance.Normal, "\t{0}", file);

                    // given the file...
                    // C:\MyProject\Source\Program.cs
                    // output the following meta data
                    var metadata = new Dictionary<string, string>();
                    metadata.Add("FullPath", file);
                    metadata.Add("RootDir", @"C:\");
                    metadata.Add("Filename", Path.GetFileNameWithoutExtension(file));
                    metadata.Add("Extension", Path.GetExtension(file));
                    metadata.Add("RelativeDir", @"\Source");
                    metadata.Add("Directory", @"MyProject\Source\");
                    metadata.Add("RecursiveDir", @"MySolution\MyProject\Source\");// take off the C:\?
                    metadata.Add("Identity", @"Source\Program.cs");
                    metadata.Add("ModifiedTime", "2004-07-01 00:21:31.5073316");
                    metadata.Add("CreatedTime", "2004-07-01 00:21:31.5073316");
                    metadata.Add("AccessedTime", "2004-07-01 00:21:31.5073316");                    

                    var item = new TaskItem(file, metadata);
                    destination.Add(item);
                }
            }
        }
    }
}
