using System;
using System.Collections.Generic;
using System.Linq;

namespace MpioAnalyser.WinApp
{
    public class InputValidator
    {
        public static bool Validate(IEnumerable<string> inputLines, out List<ServerDevicePair> serverDevicePairs, out string validationError)
        {
            var parsed  = new List<ServerDevicePair>();

            if (!inputLines.Any())
            {
                serverDevicePairs = null;
                validationError = "No input given";
                return false;
            }

            foreach (var inputLine in inputLines)
            {
                var splits = inputLine.Split( '~' );
                if (splits.Length < 2)
                {
                    serverDevicePairs = null;
                    validationError = string.Format("Bad Input: {0}", inputLine);
                    return false;
                }

                var serverName = splits[0];
                int numberOfDisks;

                if (!Int32.TryParse(splits[1], out numberOfDisks))
                {
                    serverDevicePairs = null;
                    validationError = string.Format("Bad Input, Interger expected afer server name: {0}", serverName);
                    return false;
                }

                parsed.Add(new ServerDevicePair { ServerName = serverName, NoOfDisks = numberOfDisks});
            }

            serverDevicePairs = parsed;
            validationError = "";
            return true;
        }
    }

    public class ServerDevicePair
    {
        public string ServerName;

        public int NoOfDisks;
    }
}