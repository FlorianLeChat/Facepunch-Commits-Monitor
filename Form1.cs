using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Uwp.Notifications;

using Timer = System.Timers.Timer;

namespace FacepunchCommitsMonitor
{
	public partial class Form1 : Form
	{
		public static double IntervalTime { get; set; } = 180000;
		public static Dictionary<string, bool> Repositories { get; set; } = new()
		{
			["Garry's Mod"] = false,
			["Rust"] = false,
			["Sandbox"] = false
		};

		private bool cleanupOnShutDown;
		private static readonly HttpClient client = new();
		private readonly Timer interfaceUpdateTimer = new();

		private const string directory = "images";
		private const string fallbackAvatar = "https://files.facepunch.com/garry/f549bfc2-2a49-4eb8-a701-3efd7ae046ac.png";

		/// <summary>
		/// Initialize the form and all its components.
		/// </summary>
		public Form1()
		{
			InitializeComponent();
			FormClosing += Form1_Close;

			// Supports the opening of the commit link when the button is pressed.
			ToastNotificationManagerCompat.OnActivated += toastArgs =>
			{
				var args = ToastArguments.Parse(toastArgs.Argument);

				if (args.Contains("url"))
				{
					OpenURL(args["url"]);
				}
			};
		}

		/// <summary>
		/// Opens a URL in the system default browser.
		/// </summary>
		private static void OpenURL(string url)
		{
			try
			{
				_ = Process.Start("explorer.exe", url);
			}
			catch (Exception error)
			{
				_ = MessageBox.Show(error.Message);
			}
		}

		/// <summary>
		/// Download images from the Internet to save them in the software folder.
		/// There is an easier way, but Microsoft seems to restrict some of its features for UWP applications.
		/// </summary>
		private static async Task DownloadImage(string fileName, Uri uri)
		{
			// We check if the file already exists to avoid re-downloading it.
			if (File.Exists(directory + "/" + fileName + ".jpg"))
				return;

			// Otherwise, we download it normally.
			var path = Path.Combine(directory, $"{fileName}.jpg");

			_ = Directory.CreateDirectory(directory);

			await File.WriteAllBytesAsync(path, await client.GetByteArrayAsync(uri));
		}

		/// <summary>
		/// Create a Toast notification (Windows) with custom settings.
		/// </summary>
		public static async Task CreateToastNotification(Commit data)
		{
			await DownloadImage(data.author, new Uri(string.IsNullOrWhiteSpace(data.avatar) ? fallbackAvatar : data.avatar));

			new ToastContentBuilder()
				.AddHeader(data.category, data.category, "")
				.AddText("Repository: " + data.repository)
				.AddText("Branch: " + data.branch)
				.AddAttributionText("By " + data.author)

				.AddButton(new ToastButton()
					.SetContent("Click here to reach the commit link")
					.AddArgument("url", "https://commits.facepunch.com/" + data.identifier)
				)

				.AddAppLogoOverride(new Uri(Directory.GetCurrentDirectory() + "/images/" + data.author + ".jpg"), ToastGenericAppLogoCrop.Circle)
				.AddAudio(new Uri("ms-winsoundevent:Notification.Mail"))

				.Show(toast =>
				{
					// Automatic deletion after 6 hours.
					toast.ExpirationTime = DateTime.Now.AddHours(6);
				});
		}

		/// <summary>
		/// Updates UI elements with values from the internal logic.
		/// </summary>
		private void SafeInvoke(Control element, Action callback)
		{
			if (element.InvokeRequired)
			{
				_ = element.BeginInvoke((Action)delegate { SafeInvoke(element, callback); });
			}
			else
			{
				if (!element.IsDisposed)
					callback();
			}
		}

		private void ActionTimer_Tick(object sender, EventArgs e)
		{
			// Remaining time text
			var remainingTime = Math.Round((TimeSpan.FromMilliseconds(IntervalTime) - (DateTime.Now - Program.StartTime)).TotalSeconds);

			SafeInvoke(label6, new Action(() =>
			{
				label6.Text = Regex.Replace(label6.Text, "[0-9]+", Math.Max(remainingTime, 0).ToString());
			}));

			// Remaining time progress bar
			SafeInvoke(progressBar1, new Action(() =>
			{
				progressBar1.Value = Math.Clamp((int)(100 * remainingTime / (IntervalTime / 1000)), 0, 100);
			}));
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			interfaceUpdateTimer.Elapsed += ActionTimer_Tick;
			interfaceUpdateTimer.Interval = 100;
			interfaceUpdateTimer.Enabled = true;
		}

		/// <summary>
		/// Executes the uninstallation system when the program closes.
		/// </summary>
		private void Form1_Close(object sender, FormClosingEventArgs e)
		{
			// Firstly, the notifications and data associated to the program.
			if (cleanupOnShutDown)
				ToastNotificationManagerCompat.Uninstall();

			// Then, all saved avatars.
			var files = Directory.GetFiles("images");

			foreach (var file in files)
			{
				File.Delete(file);
			}
		}

		/// <summary>
		/// Opens the GitHub page of the program author.
		/// </summary>
		private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenURL("https://github.com/FlorianLeChat");
		}

		/// <summary>
		/// Enable or disable repositories tracking.
		/// </summary>
		private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs args)
		{
			var state = args.NewValue == CheckState.Checked;

			switch (args.Index)
			{
				case 0:
					Repositories["Garry's Mod"] = state;
					break;
				case 1:
					Repositories["Rust"] = state;
					break;
				case 2:
					Repositories["Sandbox"] = state;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Changes the state of the program's uninstallation system when it is closed.
		/// </summary>
		private void CheckBox1_CheckedChanged(object sender, EventArgs e)
		{
			cleanupOnShutDown = checkBox1.Checked;

			if (cleanupOnShutDown)
				_ = MessageBox.Show("Note: When the program is running, notifications are automatically deleted 6 hours" +
					" after their creation to free up space in the control center.");
		}

		/// <summary>
		/// Changes the time interval between each check.
		/// </summary>
		private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			IntervalTime = (double)numericUpDown1.Value * 1000;

			Program.StartTime = DateTime.Now;
			Program.CheckTimer.Interval = IntervalTime;
		}
	}
}