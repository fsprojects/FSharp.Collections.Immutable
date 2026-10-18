// The F# test project sets this in testconfig.json. The MSTest runner reads only <AssemblyName>.testconfig.json, which
// MSBuild makes from a testconfig.json in the project folder, so a linked file would be ignored; the attribute is explicit.
[assembly: Microsoft.VisualStudio.TestTools.UnitTesting.Parallelize(Workers = 0, Scope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope.MethodLevel)]
