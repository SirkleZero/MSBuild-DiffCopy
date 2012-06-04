using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compare
{
    public class ComparisonResult
    {
        public ComparisonResult(IEnumerable<string> newFiles, IEnumerable<string> modifiedFiles, IEnumerable<string> notInSource)
        {
            this.NewFiles = newFiles;
            this.ModifiedFiles = modifiedFiles;
            this.NotInSource = notInSource;
        }

        public IEnumerable<string> NewFiles { get; private set; }
        public IEnumerable<string> ModifiedFiles { get; private set; }
        public IEnumerable<string> NotInSource { get; private set; }
    }
}
