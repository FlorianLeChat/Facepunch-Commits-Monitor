using FacepunchCommitsMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Windows.Forms.AxHost;

namespace FacepunchCommitsMonitorTests
{
	[TestClass]
	public class MonitorTests
	{
		[TestMethod]
		public void SelectGameCategoryTest()
		{
			Form.GetRepositories()["Garry's Mod"] = true;
			Form.GetRepositories()["Rust"] = true;
			Form.GetRepositories()["Sandbox"] = true;

			var gmod = Monitor.SelectGameCategory("GarrysMod2");
			var rust = Monitor.SelectGameCategory("Rustify");
			var sbox = Monitor.SelectGameCategory("SboxUnreal");

			Assert.AreEqual("Garry's Mod", gmod);
			Assert.AreEqual("Rust", rust);
			Assert.AreEqual("S&Box", sbox);
		}
	}
}