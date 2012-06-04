﻿using System;
using System.Collections.Generic;

namespace Compare
{
    public interface IDirectoryComparer
    {
        IEnumerable<string> Compare(string source, string destination);

        IEnumerable<string> Compare(string source, string destination, bool pruneDestination);
    }
}