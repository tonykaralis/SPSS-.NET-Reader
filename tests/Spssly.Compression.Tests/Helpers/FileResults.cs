namespace Spssly.Compression.Tests.Helpers
{
    public class FileResults
    {
        public FileResults(int recordCount, int variableCount)
        {
            RecordCount = recordCount;
            VariableCount = variableCount;
        }

        public int RecordCount { get; private set; }
        public int VariableCount { get; private set; }
    }
}
