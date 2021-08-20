﻿using LRReader.UWP.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class WebTabContent : UserControl
	{
		public WebTabViewModel ViewModel;

		public WebTabContent()
		{
			this.InitializeComponent();
			ViewModel = new WebTabViewModel();
		}

		public void LoadPage(string page, string tabId)
		{
			ViewModel.TabId = tabId;
			WebContent.Navigate(ViewModel.Page = new Uri(page));
		}

		public void RefreshPage() => WebContent.Refresh();

		private void WebContent_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) => ViewModel.NavigationStarting(sender, args);

		private void WebContent_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) => ViewModel.NavigationCompleted(sender, args);

		private async void WebContent_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
		{
			string func = "var style = document.createElement('style'); style.innerHTML = 'body, html { background: transparent !important; } p.ip { display: none !important; }'; document.head.appendChild(style);";
			await sender.InvokeScriptAsync("eval", new string[] { func });
		}
	}
}
