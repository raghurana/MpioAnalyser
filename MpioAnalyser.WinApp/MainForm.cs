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

            string errorMessage;
            List<ServerDevicePair> serverDevicePairs;
            var isInputValid = InputValidator.Validate(textBox1.Lines, out serverDevicePairs, out errorMessage);
            if (!isInputValid)
            {
                MessageBox.Show(errorMessage);
                return;
            }

            foreach (var pair in serverDevicePairs)
            {
                for (var diskNumber = 0; diskNumber < pair.NoOfDisks; diskNumber++)
                {
                    #region Log Header Info

                    logger.Log(LogLevel.Debug, "");
                    logger.Log(LogLevel.Debug, "=========================");
                    logger.Log(LogLevel.Debug, string.Format("Server Name: {0}", pair.ServerName));
                    logger.Log(LogLevel.Debug, string.Format("Disk Number: {0}", diskNumber));
                    logger.Log(LogLevel.Debug, "=========================");

                    #endregion

                    try
                    {
                        var stdOutput   = MpClaimCommandRunner.RunCommand(pair.ServerName, diskNumber);
                        var outputLines = MpClaimCommandRunner.GetStandardOutputLines(stdOutput);

                        #region Log Trimmed Header Info

                        logger.Log(LogLevel.Debug, "-----------------------");
                        logger.Log(LogLevel.Debug, "Trimmed Standard Output");
                        logger.Log(LogLevel.Debug, "-----------------------");

                        #endregion

                        if (outputLines.Count >= 8)
                        {
                            #region Parseable Output Found

                            const int startLine = 0;
                            var result = new MpClaimCommandResult
                                {
                                    ServerName = pair.ServerName,
                                    DiskIndexNumber = diskNumber,
                                    CommandExecutedSuccessfully = true,
                                    Paths = MpioParser.GetPaths(outputLines[startLine]),
                                    ControllingDSM = MpioParser.GetControllingDsm(outputLines[startLine + 1]),
                                    SerialNumber = MpioParser.GetControllingDsm(outputLines[startLine + 2]),
                                };

                            var path1 = MpioParser.GetPathInfo(outputLines[startLine + 6]);
                            var path2 = MpioParser.GetPathInfo(outputLines[startLine + 7]);

                            result.PathInfos.Add(path1);
                            result.PathInfos.Add(path2);

                            results.Add(result);

                            #endregion
                        }

                        else
                        {
                            #region Parseable Output NOT Found

                            var result = new MpClaimCommandResult
                                {
                                    ServerName = pair.ServerName,
                                    DiskIndexNumber = diskNumber,
                                    CommandExecutedSuccessfully = false,
                                    CommandFailureReason = String.Join(". ", outputLines.ToArray())
                                };

                            results.Add(result);

                            #endregion
                        }
                    }

                    catch (Exception ex)
                    {
                        #region Command Failed to Execute on the current Disk Number

                        var result = new MpClaimCommandResult
                            {
                                ServerName = pair.ServerName,
                                DiskIndexNumber = diskNumber,
                                CommandExecutedSuccessfully = false,
                                CommandFailureReason = ex.Message
                            };

                        results.Add(result);

                        #endregion
                    }
                }
            }

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
