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
		static List<string> readedIDs = new();
		static public Timer actionTimer = new();
		static readonly HttpClient client = new();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static async Task Main()
		{
			// Note: we run the check once to store the identifier of the first commit.
			await CheckForNewCommits(true);

			// Initializes the timer continually checking the new commits.
			actionTimer.Elapsed += async (sender, e) => await CheckForNewCommits(false);
			actionTimer.Interval = Form1.intervalTime * 1000;
			actionTimer.Enabled = true;

			// Default generated code to create the form.
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		/// <summary>
		/// Gives the name of the category associated with the commit repository.
		/// </summary>
		static string SelectGameCategory(string repository)
		{
			if (repository.Contains("Garrys"))
				return "Garry's Mod";

			if (repository.Contains("rust"))
				return "Rust";

			if (repository.Contains("sbox"))
				return "Sandbox";

			return "N/A";
		}

		/// <summary>
		/// Periodically check the Facepunch API for new commits.
		/// </summary>
		static async Task CheckForNewCommits(bool isFirstTime)
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

						// Store the first start identifier (as stated above).
						if (isFirstTime)
						{
							firstIdentifier = numberIdentifier;
							break;
						}

						// Checks if the identifier has not already been "read".
						if (!readedIDs.Contains(stringIdentifier) && numberIdentifier > firstIdentifier)
						{
							var userData = item.GetProperty("user");
							var gameRepository = item.GetProperty("repo").ToString();

							var commitData = new Commit
							{
								category = SelectGameCategory(gameRepository),
								identifier = stringIdentifier,
								repository = gameRepository,
								branch = item.GetProperty("branch").ToString(),
								author = userData.GetProperty("name").ToString(),
								avatar = userData.GetProperty("avatar").ToString()
							};

							readedIDs.Add(stringIdentifier);

							Form1.CreateToastNotification(commitData);
						}
					}
				}
			}
		}
	}
}