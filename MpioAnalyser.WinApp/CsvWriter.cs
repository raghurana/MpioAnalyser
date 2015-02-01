using System.Collections.Generic;
using FileHelpers;

namespace MpioAnalyser.WinApp
{
    public class CsvWriter
    {
        public static void WriteToFile(string path, IEnumerable<FinalCsvRecord> records)
        {
            var engine = new FileHelperEngine<FinalCsvRecord>();
            engine.WriteFile(path, records);
        }
    }
}