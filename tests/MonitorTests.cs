using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacepunchCommitsMonitorTests;

[TestClass]
public class MonitorTests
{
    [TestMethod]
    public void SelectGameCategoryTest()
    {
		var gmod = Monitor.SelectGameCategory("GarrysMod2");
		var rust = Monitor.SelectGameCategory("Rustify");
		var sbox = Monitor.SelectGameCategory("SboxUnreal");

		Assert.AreEqual("Garry's Mod", gmod);
		Assert.AreEqual("Rust", rust);
		Assert.AreEqual("S&Box", sbox);
    }
}