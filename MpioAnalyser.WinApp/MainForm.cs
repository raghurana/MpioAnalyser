using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using NLog;

namespace MpioAnalyser.WinApp
{
    public partial class MainForm : Form
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly List<MpClaimCommandResult> results = new List<MpClaimCommandResult>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click_1( object sender, EventArgs e )
        {
            results.Clear();

            var trace = new StringBuilder();

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
                        logger.Log(LogLevel.Debug,"");
                        logger.Log(LogLevel.Debug, "=========================" );
                        logger.Log(LogLevel.Debug, "Server Name:" + serverName );
                        logger.Log(LogLevel.Debug, "Device Number:" + driveNum );
                        logger.Log(LogLevel.Debug, "=========================" );

                        var stdOutput = MpClaimCommandRunner.RunCommand(serverName, currentDrvNum);
                        var outputLines = MpClaimCommandRunner.GetStandardOutputLines(stdOutput);

                        logger.Log(LogLevel.Debug,"----------------");
                        logger.Log(LogLevel.Debug,"Trimmed Standard Output");
                        logger.Log(LogLevel.Debug,"----------------" );

                        if (outputLines.Count >= 8)
                        {
                            const int startLine = 0;
                            var result = new MpClaimCommandResult
                                {
                                    ServerName = serverName,
                                    DriveNumber = driveNum,
                                    CommandExecutedSuccessfully = true,
                                    Paths = MpioParser.GetPaths(outputLines[startLine]),
                                    ControllingDSM = MpioParser.GetControllingDsm(outputLines[startLine + 1]),
                                    SerialNumber = MpioParser.GetControllingDsm(outputLines[startLine + 2]),
                                };

                            var path1 = MpioParser.GetPathInfo(outputLines[startLine + 6], trace);
                            var path2 = MpioParser.GetPathInfo(outputLines[startLine + 7], trace);

                            result.PathInfos.Add(path1);
                            result.PathInfos.Add(path2);

                            results.Add(result);
                        }

                        else
                        {
                            var result = new MpClaimCommandResult
                                {
                                    ServerName = serverName,
                                    DriveNumber = driveNum,
                                    CommandExecutedSuccessfully = false,
                                    FailureReason = String.Join(". ", outputLines.ToArray())
                                };

                            results.Add( result );
                        }
                    }
                }

                catch (Exception ex)
                {
                    var result = new MpClaimCommandResult
                    {
                        ServerName = serverName,
                        DriveNumber = currentDrvNum,
                        CommandExecutedSuccessfully = false,
                        FailureReason = ex.Message
                    };

                    results.Add( result );
                }
            }
            
            dataGridView1.DataSource = results;
            label4.Text = results.Count.ToString(CultureInfo.InvariantCulture);
            MessageBox.Show( "Done" );
        }

        private void dataGridView1_RowHeaderMouseClick( object sender, DataGridViewCellMouseEventArgs e )
        {
            if (e.RowIndex <= 0 || e.RowIndex >= results.Count) return;
            var currentStatus = results[e.RowIndex];
            dataGridView2.DataSource = currentStatus.PathInfos;
        }
    }
}
