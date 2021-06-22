using System;
using System.Windows.Forms;

namespace FacepunchCommitsMonitor
{
	public class Commit
	{
		public string identifier;
		public string category;
		public string repository;
		public string branch;
		public string author;
		public string avatar;
	}

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}