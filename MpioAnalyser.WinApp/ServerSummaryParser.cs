using System.Collections.Generic;
using System.Linq;

namespace MpioAnalyser.WinApp
{
    public class ServerSummaryParser
    {
        private const string MultiValueDelimeter = "|";
        private const string DiskLabel = "Disk-";
        private const string DiskPathDelimeter = "~";

        public static IEnumerable<FinalCsvRecord> SummarizeServerRecords(Lookup<string, MpClaimCommandResult> serverRecords)
        {
            var records = new List<FinalCsvRecord>();

            foreach (var serverRecord in serverRecords)
            {
                var serverName          = serverRecord.Key;
                var diskCommandResults  = serverRecords[serverName].ToArray();
                var successfulDiskCmdResults = diskCommandResults.Where(c => c.CommandExecutedSuccessfully).ToArray();

                var csvRecord = new FinalCsvRecord();
                csvRecord.ServerName = serverName;
                csvRecord.DiskNosQueriedSuccessfully = GetDiskNosQueriedSuccessfully(successfulDiskCmdResults);
                csvRecord.PathsByDiskNumber = GetPathsByDiskNumber(successfulDiskCmdResults);
                csvRecord.DiskPathInfo = GetDiskPathInfos(successfulDiskCmdResults);
                csvRecord.IsInGoodState =
            }
            
            return null;
        }

        private static string GetDiskNosQueriedSuccessfully(IEnumerable<MpClaimCommandResult> diskCommandResults)
        {
            var driveNumbers =
                diskCommandResults
                    .Select(c => c.DiskIndexNumber.ToString())
                    .ToArray();

            return string.Join(MultiValueDelimeter, driveNumbers);
        }

        private static string GetPathsByDiskNumber(IEnumerable<MpClaimCommandResult> diskCommandResults)
        {
            var paths =
                diskCommandResults
                    .Select(c => string.Format("{0}{1}{3}{2}", DiskLabel, c.DiskIndexNumber, c.Paths, DiskPathDelimeter))
                    .ToArray();

            return string.Join( MultiValueDelimeter, paths );
        }
        
        private static string GetDiskPathInfos(IEnumerable<MpClaimCommandResult> diskCommandResults)
        {
            var paths = new List<string>();
            foreach (var result in diskCommandResults)
            {
                paths.Add( string.Format("{0}{1}{2}Path1Id-{3} Path1State-{4} Path2Id-{5} Path2State-{6}", 
                    DiskLabel, result.DiskIndexNumber, DiskPathDelimeter, 
                    result.PathInfos[0].PathId, result.PathInfos[0].State, 
                    result.PathInfos[1].PathId, result.PathInfos[1].State));
            }

            return string.Join(MultiValueDelimeter, paths.ToArray());
        }
    }
}