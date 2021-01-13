﻿using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ArchiveImage : UserControl
	{
		private string _oldUrl = "";
		private Container Data = new Container();

		public ArchiveImage()
		{
			this.InitializeComponent();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			string n = args.NewValue as string;
			if (!_oldUrl.Equals(n))
			{
				Image.Opacity = 0;
				Ring.IsActive = true;
				Data.MissingImage = false;
				var image = new BitmapImage();
				image.DecodePixelType = DecodePixelType.Logical;
				image.DecodePixelHeight = 275;
				image = await Util.ByteToBitmap(await SharedGlobal.ImagesManager.GetImageCached(n), image);
				Ring.IsActive = false;
				if (image != null)
				{
					Image.Source = image;
					Image.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
				}
				else
				{
					Image.Source = null;
					Data.MissingImage = true;
				}
				_oldUrl = n;
			}
		}

		private class Container : ObservableObject
		{
			private bool _missingImage;
			public bool MissingImage
			{
				get => _missingImage;
				set
				{
					_missingImage = value;
					RaisePropertyChanged("MissingImage");
				}
			}
		}

	}

}
