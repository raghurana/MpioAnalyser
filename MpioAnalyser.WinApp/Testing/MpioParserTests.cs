using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace MpioAnalyser.WinApp.Testing
{
    [TestClass]
    public class MpioParserTests
    {
         [TestMethod]
         public void GetPathsTest()
         {
             const string testLine = "MPIO Disk0: 02 Paths, Round Robin, ALUA Not Supported";
             var output = MpioParser.GetPaths(testLine);
             Assert.AreEqual( "02 Paths", output );
         }

        [TestMethod]
        public void GetControllingDsmTest()
        {
            const string testLine = "    Controlling DSM: Microsoft DSM";
            var output = MpioParser.GetControllingDsm( testLine );
            Assert.AreEqual( "Microsoft DSM", output );
        }

        [TestMethod]
        public void GetSerialNummberTest()
        {
            const string testLine = "SN: 606E806D192000D192001042                                        ";
            var output = MpioParser.GetSerialNumber( testLine );
            Assert.AreEqual( "606E806D192000D192001042", output );
        }

        [TestMethod]
        public void GetPathInfoTest()
        {
            const string testLine = "0000000077080000 Active/Optimized   008|000|000|000   0";
            var output = MpioParser.GetPathInfo( testLine);
            Assert.AreEqual( "0000000077080000", output.PathId );
            Assert.AreEqual( "Active/Optimized", output.State );
            Assert.AreEqual( "008|000|000|000", output.ScsiAddress );
            Assert.AreEqual( "0", output.Weight );
        }
    }
}