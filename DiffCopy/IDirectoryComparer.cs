using System;
using System.Collections.Generic;

namespace DiffCopy
{
    public interface IDirectoryComparer
    {
        ComparisonResult Compare(string source, string destination);
    }
}
