using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MpioAnalyser.WinApp
{
    public partial class MainForm : Form
    {
        private readonly List<CheckStatus> results = new List<CheckStatus>();

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
                        trace.AppendLine("");
                        trace.AppendLine( "=========================" );
                        trace.AppendLine( "Server Name:" + serverName );
                        trace.AppendLine( "Device Number:" + driveNum );
                        trace.AppendLine( "=========================" );

                        var process = new Process();
                        var startInfo = new ProcessStartInfo
                            {
                                UseShellExecute = false,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                FileName = "psexec.exe",
                                Arguments = string.Format("\\\\{0} mpclaim -s -d {1}", serverName, driveNum)
                            };

                        process.StartInfo = startInfo;
                        process.Start();
                        process.WaitForExit();

                        var stdOutput = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                        trace.AppendLine("Full Standard Output:");
                        trace.AppendLine("----------------");
                        trace.AppendLine(stdOutput);
                        trace.AppendLine("");
                        trace.AppendLine("----------------");
                        trace.AppendLine("Trimmed Standard Output");
                        trace.AppendLine( "----------------" );

                        var outputLines = new List<string>();
                        using (var reader = new StringReader(stdOutput))
                        {
                            String line;
                            do
                            {
                                line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line))
                                {
                                    trace.AppendLine(line);
                                    if ( line.StartsWith( "PsExec", true, CultureInfo.CurrentCulture ) ) 
                                        break;

                                    outputLines.Add( line );
                                }
                                    
                            } while ( line != null );
                        }

                        if (outputLines.Count >= 8)
                        {
                            const int startLine = 0;
                            var result = new CheckStatus
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
                            var result = new CheckStatus
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
                    MessageBox.Show(ex.Message);

                    var result = new CheckStatus
                    {
                        ServerName = serverName,
                        DriveNumber = currentDrvNum,
                        CommandExecutedSuccessfully = false,
                        FailureReason = ex.Message
                    };

                    results.Add( result );
                }
            }

            File.WriteAllText( "Output.txt", trace.ToString() );
            MessageBox.Show("Done");
            dataGridView1.DataSource = results;
            label4.Text = results.Count.ToString(CultureInfo.InvariantCulture);
        }

        private void dataGridView1_RowHeaderMouseClick( object sender, DataGridViewCellMouseEventArgs e )
        {
            if (e.RowIndex > 0 && e.RowIndex < results.Count)
            {
                var currentStatus = results[e.RowIndex];
                dataGridView2.DataSource = currentStatus.PathInfos;
            }
        }
    }
}
