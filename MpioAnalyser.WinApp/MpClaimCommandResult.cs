using System.Collections.Generic;

namespace MpioAnalyser.WinApp
{
    public class MpClaimCommandResult
    {
        public string ServerName { get; set; }

        public int DriveNumber { get; set; }

        public string Paths { get; set; }

        public string ControllingDSM { get; set; }

        public string SerialNumber { get; set; }

        public bool CommandExecutedSuccessfully { get; set; }

        public string FailureReason { get; set; }

        public List<PathInfo> PathInfos = new List<PathInfo>();
    }

    public class PathInfo
    {
        public string PathId { get; set; }

        public string State { get; set; }

        public string ScsiAddress { get; set; }

        public string Weight { get; set; }
    }
}