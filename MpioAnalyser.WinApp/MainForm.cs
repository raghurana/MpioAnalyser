using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NLog;

namespace MpioAnalyser.WinApp
{
    public partial class MainForm : Form
    {
        #region Fields

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly List<MpClaimCommandResult> results = new List<MpClaimCommandResult>();

        #endregion

        #region Constructor(s)

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        private void button1_Click_1( object sender, EventArgs e )
        {
            results.Clear();

            #region Iterate Server and Disks

            foreach (var serverPlusDrives in textBox1.Lines)
            {
                var splits = serverPlusDrives.Split('~');
                var serverName = splits[0];
                var numberOfDrives = Int32.Parse(splits[1]);

                var currentDrvNum = 0;
                try
                {
                    for (var driveNum = 0; driveNum < numberOfDrives; driveNum++)
                    {
                        currentDrvNum = driveNum;
                        logger.Log(LogLevel.Debug, "");
                        logger.Log(LogLevel.Debug, "=========================");
                        logger.Log(LogLevel.Debug, "Server Name:" + serverName);
                        logger.Log(LogLevel.Debug, "Device Number:" + driveNum);
                        logger.Log(LogLevel.Debug, "=========================");

                        var stdOutput = MpClaimCommandRunner.RunCommand(serverName, currentDrvNum);
                        var outputLines = MpClaimCommandRunner.GetStandardOutputLines(stdOutput);

                        logger.Log(LogLevel.Debug, "----------------");
                        logger.Log(LogLevel.Debug, "Trimmed Standard Output");
                        logger.Log(LogLevel.Debug, "----------------");

                        #region Parse Mpio Command Result

                        if (outputLines.Count >= 8)
                        {
                            const int startLine = 0;
                            var result = new MpClaimCommandResult
                                {
                                    ServerName = serverName,
                                    DiskIndexNumber = driveNum,
                                    CommandExecutedSuccessfully = true,
                                    Paths = MpioParser.GetPaths(outputLines[startLine]),
                                    ControllingDSM = MpioParser.GetControllingDsm(outputLines[startLine + 1]),
                                    SerialNumber = MpioParser.GetControllingDsm(outputLines[startLine + 2]),
                                };

                            var path1 = MpioParser.GetPathInfo(outputLines[startLine + 6], logger);
                            var path2 = MpioParser.GetPathInfo(outputLines[startLine + 7], logger);

                            result.PathInfos.Add(path1);
                            result.PathInfos.Add(path2);

                            results.Add(result);
                        }

                        else
                        {
                            var result = new MpClaimCommandResult
                                {
                                    ServerName = serverName,
                                    DiskIndexNumber = driveNum,
                                    CommandExecutedSuccessfully = false,
                                    FailureReason = String.Join(". ", outputLines.ToArray())
                                };

                            results.Add(result);
                        }

                        #endregion
                    }
                }

                catch (Exception ex)
                {
                    var result = new MpClaimCommandResult
                        {
                            ServerName = serverName,
                            DiskIndexNumber = currentDrvNum,
                            CommandExecutedSuccessfully = false,
                            FailureReason = ex.Message
                        };

                    results.Add(result);
                }
            }

            #endregion

            dataGridView1.DataSource = results;
            label4.Text = results.Count.ToString(CultureInfo.InvariantCulture);

            try
            {
                var serverGroups = results.ToLookup( r => r.ServerName );
                var finalCsvRecords = ServerSummaryParser.SummarizeServerRecords(serverGroups);
                CsvWriter.WriteToFile("FinalCsvOut.csv", finalCsvRecords);
                MessageBox.Show( "Done" );
            }

            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to save results to csv due to: {0}", ex.Message));
            }
        }

        private void dataGridView1_RowHeaderMouseClick( object sender, DataGridViewCellMouseEventArgs e )
        {
            if (e.RowIndex <= 0 || e.RowIndex >= results.Count) return;
            var currentStatus = results[e.RowIndex];
            dataGridView2.DataSource = currentStatus.PathInfos;
        }
    }
}
