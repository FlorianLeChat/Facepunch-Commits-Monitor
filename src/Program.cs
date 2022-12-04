using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
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

		public Commit(string identifier, string category, string repository, string branch, string author, string avatar)
		{
			// Incremented number from the beginning assigned to the commit (generated on the Facepunch side).
			this.identifier = identifier;

			// "Nice" game name associated with the commit (e.g. "Garry's Mod", "Rust", "S&Box"). 
			this.category = category;

			// GitHub repository name attached with the commit (e.g. "rust_reboot").
			this.repository = repository;

			// Branch involved in the GitHub repository of the commit (e.g. "x86-64").
			this.branch = branch;

			// Author of the GitHub commit (e.g. "Garry Newman").
			this.author = author;

			// URL to the avatar of the commit author on GitHub (e.g. "https://files.facepunch.com/web/avatar/151-51815457.png").
			this.avatar = avatar;
		}
	}

	internal class Program
	{
		public static DateTime StartTime { get; set; }
		public static Timer CheckTimer { get; set; } = new();

		private static uint firstIdentifier;
		private static readonly HttpClient client = new();
		private static readonly List<string> readedIds = new();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static async Task Main()
		{
			// Note: we run the check once to store the identifier of the first commit.
			await CheckForNewCommits(true);

			// Initializes the timer continually checking the new commits.
			CheckTimer.Elapsed += async (sender, e) =>
			{
				await CheckForNewCommits(false);
			};
			CheckTimer.Interval = Form.IntervalTime;
			CheckTimer.Enabled = true;

			// Default generated code to create the form.
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form());
		}

		/// <summary>
		/// Gives the name of the category associated with the commit repository.
		/// This also serves as a check to see if the game needs to be monitored.
		/// </summary>
		private static string SelectGameCategory(string repository)
		{
			if (repository.Contains("Garrys") && Form.Repositories["Garry's Mod"])
				return "Garry's Mod";

			if (repository.Contains("rust") && Form.Repositories["Rust"])
				return "Rust";

			if (repository.Contains("sbox") && Form.Repositories["Sandbox"])
				return "Sandbox";

			return "N/A";
		}

		/// <summary>
		/// Periodically check the Facepunch API for new commits.
		/// </summary>
		private static async Task CheckForNewCommits(bool isFirstTime)
		{
			// Let's record the execution start time of the check code
			StartTime = DateTime.Now;

			// Then, we continue to run the check.
			var request = await client.GetAsync("https://commits.facepunch.com/?format=json");

			if (request.IsSuccessStatusCode && request.Content != null)
			{
				var document = await JsonDocument.ParseAsync(await request.Content.ReadAsStreamAsync());

				if (document.RootElement.TryGetProperty("results", out var items))
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

						// Check if the commit is one of the games to monitor.
						var gameRepository = item.GetProperty("repo").ToString();
						var gameCategory = SelectGameCategory(gameRepository);

						if (gameCategory == "N/A")
							continue;

						// Checks if the identifier has not already been "read".
						if (!readedIds.Contains(stringIdentifier) && numberIdentifier > firstIdentifier)
						{
							var userData = item.GetProperty("user");

							await Form.CreateToastNotification(
								new Commit(
									stringIdentifier,
									gameCategory,
									gameRepository,
									item.GetProperty("branch").ToString(),
									userData.GetProperty("name").ToString(),
									userData.GetProperty("avatar").ToString()
								)
							);

							readedIds.Add(stringIdentifier);
						}
					}
				}
			}
		}
	}
}