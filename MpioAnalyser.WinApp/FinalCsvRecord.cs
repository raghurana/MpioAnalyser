using FileHelpers;

namespace MpioAnalyser.WinApp
{
    [DelimitedRecord( "," )] 
    public class FinalCsvRecord
    {
        public string ServerName { get; set; }

        public string DiskNosQueriedSuccessfully { get; set; }

        public string PathsByDiskNumber { get; set; }

        public string DiskPathInfo { get; set; }

        public bool IsInGoodState { get; set; }

        public string FailureReasonsByDisk { get; set; }
    }
}