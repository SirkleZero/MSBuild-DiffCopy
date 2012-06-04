using System;
using System.Collections.Generic;

namespace Compare
{
    public interface IDirectoryComparer
    {
        ComparisonResult Compare(string source, string destination);
    }
}
