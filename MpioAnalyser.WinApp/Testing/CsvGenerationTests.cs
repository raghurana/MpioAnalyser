using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MpioAnalyser.WinApp.Testing
{
    [TestClass]
    public class CsvGenerationTests
    {
        private const string CsvFileName = "TestCsvFileName.csv";

        [TestMethod]
        public void ServerWithTwoDisks_BothDiskCommandsExecutedSuccessfully_BothDisksActive()
        {
            var commandResults = GetSingleServerDummyOutputWith2ActivePaths();
            var csvRecords = ServerSummaryParser.SummarizeServerRecords(commandResults.ToLookup(r => r.ServerName));
            CsvWriter.WriteToFile( CsvFileName, csvRecords);
        }

        [TestMethod]
        public void ServerWithTwoDisks_BothDiskCommandsExecutedSuccessfully_OneDiskActive()
        {
            var commandResults = GetSingleServerDummyOutputWith2ActivePaths().ToList();
            commandResults[1].PathInfos[1].State = "Down";
            var csvRecords = ServerSummaryParser.SummarizeServerRecords( commandResults.ToLookup( r => r.ServerName ) );
            CsvWriter.WriteToFile( CsvFileName, csvRecords );
        }

        [TestMethod]
        public void ServerWithTwoDisks_ThirdDiskNotFound_TwoDisksActive()
        {
            var commandResults = GetSingleServerDummyOutputWith2ActivePaths().ToList();
            commandResults.Add(new MpClaimCommandResult
                {
                    ServerName = commandResults[0].ServerName,
                    DiskIndexNumber = 2,
                    CommandExecutedSuccessfully = false,
                    CommandFailureReason = "Cannot find specified drive number"
                });

            var csvRecords = ServerSummaryParser.SummarizeServerRecords( commandResults.ToLookup( r => r.ServerName ) );
            CsvWriter.WriteToFile( CsvFileName, csvRecords );
        }

        #region Private Methods

        private static IEnumerable<MpClaimCommandResult> GetSingleServerDummyOutputWith2ActivePaths()
        {
            var path1 = new PathInfo
                {
                    PathId = "0000000077030002",
                    State = "Active/Optimized",
                    ScsiAddress = "003|000|002|000",
                    Weight = "0"
                };

            var path2 = new PathInfo
                {
                    PathId = "0000000077030000 ",
                    State = "Active/Optimized",
                    ScsiAddress = "003|000|000|000",
                    Weight = "0"
                };

            var result1 = new MpClaimCommandResult
                {
                    ServerName = "Server1",
                    DiskIndexNumber = 0,
                    Paths = "02 Paths",
                    ControllingDSM = "Microsoft DSM",
                    SerialNumber = "600D310010B00000000011",
                    CommandExecutedSuccessfully = true,
                    PathInfos = {path1, path2}
                };

            var result2 = new MpClaimCommandResult
                {
                    ServerName = "Server1",
                    DiskIndexNumber = 1,
                    Paths = "02 Paths",
                    ControllingDSM = "Microsoft DSM",
                    SerialNumber = "600D310010B00000000011",
                    CommandExecutedSuccessfully = true,
                    PathInfos = {path1, path2}
                };

            yield return result1;
            yield return result2;
        }

        #endregion
    }
}