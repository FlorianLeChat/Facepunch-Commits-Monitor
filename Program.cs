using System;
using System.Timers;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using Timer = System.Timers.Timer;

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
		static uint firstIdentifier;
		static bool isFirstTime = true;
		static List<string> readedIDs = new();
		static public Timer actionTimer = new();
		static readonly HttpClient client = new();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Initializes the timer continually checking the new commits.
			actionTimer.Elapsed += async (sender, e) => await CheckForNewCommits(sender, e);
			actionTimer.Interval = Form1.intervalTime * 1000;
			actionTimer.Enabled = true;

			// Default code to create the form.
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		/// <summary>
		/// Periodically check the Facepunch API for new commits.
		/// </summary>
		static private async Task CheckForNewCommits(object sender, ElapsedEventArgs e)
		{
			var request = await client.GetAsync("https://commits.facepunch.com/?format=json");

			if (request.IsSuccessStatusCode && request.Content != null)
			{
				var document = await JsonDocument.ParseAsync(await request.Content.ReadAsStreamAsync());
				var details = document.RootElement;

				if (details.TryGetProperty("results", out var items))
				{
					foreach (var item in items.EnumerateArray())
					{
						var stringIdentifier = item.GetProperty("id").ToString();
						var numberIdentifier = uint.Parse(stringIdentifier);

						if (isFirstTime)
						{
							firstIdentifier = numberIdentifier;
							isFirstTime = false;
							break;
						}

						if (!readedIDs.Contains(stringIdentifier) && numberIdentifier > firstIdentifier)
						{
							var data = new Commit
							{
								category = "Garry's Mod",
								identifier = stringIdentifier,
								repository = item.GetProperty("repo").ToString(),
								branch = item.GetProperty("branch").ToString(),
								author = "Rubat",
								avatar = "https://files.facepunch.com/s/b8ec968c721a.jpg"
							};

							readedIDs.Add(stringIdentifier);

							Form1.CreateToastNotification(data);
						}
					}
				}
			}
		}
	}
}