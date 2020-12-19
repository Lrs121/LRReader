﻿using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.UWP.ViewModels.Items;
using LRReader.UWP.Views.Tabs;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ArchiveItem : UserControl
	{

		private ArchiveItemViewModel ViewModel;

		private string _oldID = "";

		private bool _open;

		public ArchiveItem()
		{
			this.InitializeComponent();
			ViewModel = new ArchiveItemViewModel();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Archive = args.NewValue as Archive;

			if (!_oldID.Equals(ViewModel.Archive.arcid))
			{
				Overlay.Opacity = 0;
				Title.Opacity = 0;
				TagsGrid.Opacity = 0;
				Thumbnail.Source = null;
				Ring.IsActive = true;
				ViewModel.MissingImage = false;

				byte[] bytes = await ArchivesProvider.GetThumbnail(ViewModel.Archive.arcid);
				if (bytes?.Length > 0)
				{
					var image = await Util.ByteToBitmap(bytes);
					image.DecodePixelType = DecodePixelType.Logical;
					image.DecodePixelHeight = 275;
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
				}
				else
				{
					ViewModel.MissingImage = true;
				}
				Ring.IsActive = false;
				Overlay.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
				Title.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
				TagsGrid.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
				_oldID = ViewModel.Archive.arcid;
			}
		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new ArchiveTab(ViewModel.Archive), false);
		}

		private void EditMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new ArchiveEditTab(ViewModel.Archive));
		}

		private async void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Downloading = true;
			var download = await ViewModel.DownloadArchive();
			if (download == null)
			{
				ViewModel.Downloading = false;
				return;
			}

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
			savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
			savePicker.SuggestedFileName = download.Name;

			StorageFile file = await savePicker.PickSaveFileAsync();
			ViewModel.Downloading = false;
			if (file != null)
			{
				CachedFileManager.DeferUpdates(file);
				await FileIO.WriteBytesAsync(file, download.Data);
				FileUpdateStatus status =
					await CachedFileManager.CompleteUpdatesAsync(file);
				if (status == FileUpdateStatus.Complete)
				{
					//save
				}
				else
				{
					// not saved
				}
			}
			else
			{
				//cancel
			}
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Bookmarked = true;
		}

		private void Remove_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Bookmarked = false;
		}

		private async void TagsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_open = true;
			await Task.Delay(TimeSpan.FromMilliseconds(300));
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = true;
				ShowPopup.Begin();
			}
		}

		private void TagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = false;
			}
			if (TagsPopup.IsOpen)
				HidePopup.Begin();
		}

		private void HidePopup_Completed(object sender, object e)
		{
			TagsPopup.IsOpen = false;
		}

		private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Global.EventManager.AddTab(new ArchiveTab(ViewModel.Archive), false);
				}
			}
		}
	}
}
