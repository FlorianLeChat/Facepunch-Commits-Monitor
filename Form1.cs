using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FacepunchCommitsMonitor
{
	public partial class Form1 : Form
	{
		static bool cleanupOnShutDown = false;

		/// <summary>
		/// Initialize the form and all its components.
		/// </summary>
		public Form1()
		{
			InitializeComponent();
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
		/// Opens the GitHub page of the program author.
		/// </summary>
		private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenURL("https://github.com/FlorianLeChat");
		}
	}
}