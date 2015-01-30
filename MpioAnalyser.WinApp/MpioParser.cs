using System.Linq;
using System.Text;

namespace MpioAnalyser.WinApp
{
    public class MpioParser
    {
        public static string GetPaths( string line )
        {
            var colonIndex = line.IndexOf( ':' );
            var commaIndex = line.IndexOf( ',' );
            return line.Substring( colonIndex + 1, (commaIndex - colonIndex) - 1).Trim();
        }

        public static string GetControllingDsm( string line )
        {
            var colonIndex = line.IndexOf( ':' );
            return line.Substring( colonIndex + 1, (line.Length - colonIndex) - 1).Trim();
        }

        public static string GetSerialNumber(string line)
        {
            return GetControllingDsm(line);
        }

        public static PathInfo GetPathInfo(string line, StringBuilder trace)
        {
            var columns = line.Split(' ').Where(c => !string.IsNullOrEmpty(c)).Select( c => c.Trim() ).ToArray();
            trace.AppendLine("Total Path Info Columns = " + columns.Length);

            if (columns.Length >= 4)
                return new PathInfo
                    {
                        PathId = columns[0],
                        State = columns[1],
                        ScsiAddress = columns[2],
                        Weight = columns[3]
                    };

            return new PathInfo();
        }
    }
}