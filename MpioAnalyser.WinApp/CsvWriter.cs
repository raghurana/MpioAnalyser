using System.Collections.Generic;
using FileHelpers;

namespace MpioAnalyser.WinApp
{
    public class CsvWriter
    {
        public static void WriteToFile(string path, IEnumerable<FinalCsvRecord> records)
        {
            var engine = new FileHelperEngine<FinalCsvRecord>
                {
                    HeaderText = @"Server,Disks Queried Successfully,Paths By Disk,Disks Path Info,Is In Good State, Failure Reasons By Disk"
                };
            engine.WriteFile(path, records);
        }
    }
}