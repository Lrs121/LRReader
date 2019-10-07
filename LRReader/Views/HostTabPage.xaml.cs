﻿using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.ViewModels;
using LRReader.Views.Items;
using LRReader.Views.Tabs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class HostTabPage : Page
	{

		private HostTabPageViewModel Data;

		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		public HostTabPage()
		{
			this.InitializeComponent();

			Data = DataContext as HostTabPageViewModel;

			CoreView = CoreApplication.GetCurrentView();
			AppView = ApplicationView.GetForCurrentView();

			CoreApplicationViewTitleBar coreTitleBar = CoreView.TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;
			//TitleBar.Height = coreTitleBar.Height;
			coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

			ApplicationViewTitleBar titleBar = AppView.TitleBar;
			titleBar.ButtonBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];
			AppView.VisibleBoundsChanged += AppView_VisibleBoundsChanged;

			Window.Current.SetTitleBar(TitleBar);

			Global.EventManager.ShowErrorEvent += ShowError;
			Global.EventManager.AddTabEvent += AddTab;
			Global.EventManager.CloseAllTabsEvent += CloseAllTabs;
			Global.EventManager.CloseTabWithHeaderEvent += CloseTabWithHeader;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await DispatcherHelper.RunAsync(() =>
			{
				bool firstRun = Global.SettingsManager.Profile == null;
				if (firstRun)
				{
					Global.EventManager.AddTab(new FirstRunTab());
				}
				else
				{
					Global.LRRApi.RefreshSettings();
					Global.EventManager.AddTab(new ArchivesTab());
				}
			});
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			//TitleBar.Height = coreTitleBar.Height;
			//TitleBarLeft.Margin = new Thickness(coreTitleBar.SystemOverlayLeftInset, 0, 0, 0);
			TabViewEndHeader.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
		}

		private void ShowError(string title, string content)
		{
			Notifications.Show(new NotificationItem(title, content), 5000);
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new SettingsTab());
		}

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e)
		{
			AppView.TryEnterFullScreenMode();
		}

		private void AppView_VisibleBoundsChanged(ApplicationView sender, object args)
		{
			Data.FullScreen = AppView.IsFullScreenMode;
		}

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			if (args.Tab is ArchiveTab unTab)
				unTab.UnloadInternal();
			TabViewControl.TabItems.Remove(args.Tab);
		}

		public async void AddTab(TabViewItem tab, bool switchToTab)
		{
			var current = GetTabFromHeader(tab.Header);
			if (current != null)
			{
				if (switchToTab)
					Data.CurrentTab = current;
			}
			else
			{
				TabViewControl.TabItems.Add(tab);
				if (switchToTab)
					await DispatcherHelper.RunAsync(() => Data.CurrentTab = tab);
			}
		}

		public void CloseAllTabs()
		{
			TabViewControl.TabItems.Clear();
		}

		public void CloseTabWithHeader(string header)
		{
			var tab = GetTabFromHeader(header);
			if (tab != null)
			{
				TabViewControl.TabItems.Remove(tab);
			}
		}

		private TabViewItem GetTabFromHeader(object header) => TabViewControl.TabItems.FirstOrDefault(t => (t as TabViewItem).Header.Equals(header)) as TabViewItem;
	}
}
