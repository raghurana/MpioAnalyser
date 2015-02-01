using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MpioAnalyser.WinApp
{
    public class ServerSummaryParser
    {
        private const string MultiValueDelimeter = "|";
        private const string DiskLabel = "Disk";
        private const string DiskPathDelimeter = "~";

        public static IEnumerable<FinalCsvRecord> SummarizeServerRecords(ILookup<string, MpClaimCommandResult> serverRecords)
        {
            var records = new List<FinalCsvRecord>();

            foreach (var serverRecord in serverRecords)
            {
                var serverName                  = serverRecord.Key;
                var diskCommandResults          = serverRecords[serverName].ToArray();
                var successfulDiskCmdResults    = diskCommandResults.Where(c => c.CommandExecutedSuccessfully).ToArray();
                var unsuccessfulDiskCmdResults  = diskCommandResults.Except(successfulDiskCmdResults);

                var csvRecord = new FinalCsvRecord();
                csvRecord.ServerName                    = serverName;
                csvRecord.DiskNosQueriedSuccessfully    = GetDiskNosQueriedSuccessfully(successfulDiskCmdResults);
                csvRecord.PathsByDiskNumber             = GetPathsByDiskNumber(successfulDiskCmdResults);
                csvRecord.DiskPathInfo                  = GetDiskPathInfos(successfulDiskCmdResults);
                csvRecord.IsInGoodState                 = GetGoodState(successfulDiskCmdResults);
                csvRecord.FailureReasonsByDisk          = GetFailureReasonsByDisk(unsuccessfulDiskCmdResults);

                records.Add(csvRecord);
            }
            
            return records;
        }

        private static string GetDiskNosQueriedSuccessfully(IEnumerable<MpClaimCommandResult> diskCommandResults)
        {
            var driveNumbers =
                diskCommandResults
                    .Select(c => string.Format("{0}{1}", DiskLabel, c.DiskIndexNumber.ToString(CultureInfo.InvariantCulture)))
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
            foreach (var result in diskCommandResults.Where( r => r.PathInfos.Count >= 2 ))
            {
                paths.Add( string.Format( "{0}{1}{2}Path1(Id-{3} State-{4}){0}{1}{2}Path2(Id-{5} State-{6})",
                                            DiskLabel, result.DiskIndexNumber, DiskPathDelimeter,
                                            result.PathInfos[0].PathId, result.PathInfos[0].State,
                                            result.PathInfos[1].PathId, result.PathInfos[1].State ) );
            }

            return string.Join(MultiValueDelimeter, paths.ToArray());
        }

        private static bool GetGoodState(IEnumerable<MpClaimCommandResult> diskCommandResults)
        {
            return diskCommandResults.All( c => c.PathInfos.All( p => string.Equals( "Active/Optimized", p.State, StringComparison.CurrentCultureIgnoreCase ) ) );
        }

        private static string GetFailureReasonsByDisk( IEnumerable<MpClaimCommandResult> diskCommandResults )
        {
            return String.Join(MultiValueDelimeter, diskCommandResults.Select(r => r.FailureReason).ToArray());
        }
    }
}