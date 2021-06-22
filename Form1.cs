using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FacepunchCommitsMonitor
{
	public partial class Form1 : Form
	{
		static bool cleanupOnShutDown = false;
		static Dictionary<string, bool> repositories = new()
		{
			["Garry's Mod"] = false,
			["Rust"] = false,
			["Sandbox"] = false
		};

		/// <summary>
		/// Initialize the form and all its components.
		/// </summary>
		public Form1()
		{
			InitializeComponent();
			FormClosing += Form1_Close;
		}

		/// <summary>
		/// Opens a URL in the system default browser.
		/// </summary>
		static private void OpenURL(string url)
		{
			try
			{
				Process.Start("explorer.exe", url);
			}
			catch (Exception error)
			{
				MessageBox.Show(error.Message);
			}
		}

		/// <summary>
		/// Create a Toast notification (Windows) with custom settings.
		/// </summary>
		static public void CreateToastNotification(string id, string category, string repository, string branch, string author, string url)
		{
			// Supports the opening of the commit link when the button is pressed.
			ToastNotificationManagerCompat.OnActivated += toastArgs =>
			{
				var args = ToastArguments.Parse(toastArgs.Argument);
				OpenURL(args["url"]);
			};

			// Builds the notification with the filled parameters.
			new ToastContentBuilder()
				.AddArgument("conversationId", 9813)

				.AddHeader("6289", category, "")
				.AddText("Repository: " + repository)
				.AddText("Branch: " + branch)
				.AddAttributionText("By " + author)

				.AddButton(new ToastButton()
					.SetContent("Click here to reach the commit link")
					.AddArgument("url", "https://commits.facepunch.com/" + id)
				)

				.AddAppLogoOverride(new Uri(url), ToastGenericAppLogoCrop.Circle)
				.AddAudio(new Uri("ms-winsoundevent:Notification.Mail"))

				.Show(toast =>
				{
					// Automatic deletion after 12 hours.
					toast.ExpirationTime = DateTime.Now.AddHours(12);
				});
		}

		/// <summary>
		/// Test the notification system.
		/// </summary>
		private void Form1_Load(object sender, EventArgs e)
		{
			CreateToastNotification("385699", "Garry's Mod", "rust_reboot", "main", "Rubat", "https://files.facepunch.com/s/43d4ef6a46eb.jpg");
		}

		/// <summary>
		/// Executes the uninstallation system when the program closes.
		/// </summary>
		private void Form1_Close(object sender, FormClosingEventArgs e)
		{
			if (cleanupOnShutDown)
				ToastNotificationManagerCompat.Uninstall();
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
		private void CheckedListBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedID = checkedListBox1.SelectedIndex;
			var selectedState = checkedListBox1.GetItemChecked(selectedID);

			switch (selectedID)
			{
				case 0:
					repositories["Garry's Mod"] = selectedState;
					break;
				case 1:
					repositories["Rust"] = selectedState;
					break;
				case 2:
					repositories["Sandbox"] = selectedState;
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
				MessageBox.Show("Note: When the program is running, notifications are automatically deleted 12 hours" +
					" after their creation to free up space in the control center.");
		}
	}
}