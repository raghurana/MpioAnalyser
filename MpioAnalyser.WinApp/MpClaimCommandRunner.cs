using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace MpioAnalyser.WinApp
{
    public class MpClaimCommandRunner
    {
        public static string RunCommand( string serverName, int driveNum )
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "psexec.exe",
                Arguments = string.Format( "\\\\{0} mpclaim -s -d {1}", serverName, driveNum )
            };

            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            return process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        }

        public static List<string> GetStandardOutputLines( string stdOutput )
        {
            var outputLines = new List<string>();

            using ( var reader = new StringReader( stdOutput ) )
            {
                String line;
                do
                {
                    line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line)) continue;
                    if ( line.StartsWith( "PsExec", true, CultureInfo.CurrentCulture ) )
                        break;

                    outputLines.Add( line );
                } while ( line != null );
            }
            return outputLines;
        }
    }
}