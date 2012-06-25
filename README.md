MSBuild DiffCopy
================
A custom MSBuild task that will compare two directories and return a list of new, modified and orphaned files. This functionality is particularily useful for doing differential build/deployment on very large projects.

For example, rather than deploy a large number of files, or numerous large files that havent changed, simply deploy only the files that are new or have been modified.

Features
========

1. Compare files using an optimized (fast) byte comparison.
2. Compare operation returns a list of new and modified files.
3. Orphaned file detection. Find files that exist in the destination but not the source.