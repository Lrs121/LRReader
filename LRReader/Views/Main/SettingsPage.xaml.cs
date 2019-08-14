﻿using LRReader.Internal;
using LRReader.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views.Main
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SettingsPage : Page
	{
		private SettingsPageViewModel Data;

		public SettingsPage()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			ProfileError.Text = "";
			ServerProfileDialog.PrimaryButtonText = "Add";
			var result = await ServerProfileDialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.AddProfile(ProfileName.Text, ProfileServerAddress.Text, ProfileServerApiKey.Password);
			}
			ProfileName.Text = "";
			ProfileServerAddress.Text = "";
			ProfileServerApiKey.Password = "";
		}

		private async void ButtonEdit_Click(object sender, RoutedEventArgs e)
		{
			ProfileError.Text = "";
			ServerProfileDialog.PrimaryButtonText = "Save";
			ServerProfile profile = Data.SettingsManager.Profile;
			ProfileName.Text = profile.Name;
			ProfileServerAddress.Text = profile.ServerAddress;
			ProfileServerApiKey.Password = profile.ServerApiKey;

			var result = await ServerProfileDialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.ModifyProfile(profile.UID, ProfileName.Text, ProfileServerAddress.Text, ProfileServerApiKey.Password);
				Global.LRRApi.RefreshSettings();
			}
			ProfileName.Text = "";
			ProfileServerAddress.Text = "";
			ProfileServerApiKey.Password = "";
		}

		private async void ButtonRemove_Click(object sender, RoutedEventArgs e)
		{
			ContentDialog dialog = new ContentDialog { Title = "Remove Profile?", PrimaryButtonText = "Yes", CloseButtonText = "No" };
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				var sm = Data.SettingsManager;
				sm.Profiles.Remove(sm.Profile);
				sm.Profile = null;
				sm.Profile = sm.Profiles.First();
			}
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Data.SettingsManager.Profile != null)
				Global.LRRApi.RefreshSettings();
		}

		private void ProfileName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			ProfileError.Text = "";
			if (string.IsNullOrEmpty(ProfileName.Text))
			{
				ProfileError.Text = "Empty Profile Name";
				allow = false;
			}
			ServerProfileDialog.IsPrimaryButtonEnabled = allow && ValidateServerAddress();
		}

		private void ProfileServerAddress_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			ProfileError.Text = "";
			if (string.IsNullOrEmpty(ProfileServerAddress.Text))
			{
				ProfileError.Text = "Empty Server Address";
				allow = false;
			}
			if (!Uri.IsWellFormedUriString(ProfileServerAddress.Text, UriKind.Absolute))
			{
				ProfileError.Text = "Invalid Server Address";
				allow = false;
			}
			ServerProfileDialog.IsPrimaryButtonEnabled = allow && ValidateProfileName();
		}

		private bool ValidateProfileName()
		{
			return !string.IsNullOrEmpty(ProfileName.Text);
		}

		private bool ValidateServerAddress()
		{
			return !string.IsNullOrEmpty(ProfileServerAddress.Text) && Uri.IsWellFormedUriString(ProfileServerAddress.Text, UriKind.Absolute);
		}
	}
}
