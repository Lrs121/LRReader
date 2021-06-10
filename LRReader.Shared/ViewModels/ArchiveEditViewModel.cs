﻿using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LRReader.Shared.ViewModels
{
	public class ArchiveEditViewModel : ObservableObject
	{
		private readonly EventsService Events;

		public AsyncRelayCommand SaveCommand { get; }
		public AsyncRelayCommand UsePluginCommand { get; }
		public AsyncRelayCommand ReloadCommand { get; }

		private RelayCommand<EditableTag> TagCommand { get; }

		public Archive Archive;

		public string Title { get; set; }
		private string _tags;
		public string Tags
		{
			get => _tags;
			set => SetProperty(ref _tags, value);
		}

		private bool _saving;
		public bool Saving
		{
			get => _saving;
			set
			{
				SetProperty(ref _saving, value);
				TagCommand.NotifyCanExecuteChanged();
				SaveCommand.NotifyCanExecuteChanged();
				UsePluginCommand.NotifyCanExecuteChanged();
				ReloadCommand.NotifyCanExecuteChanged();
			}
		}

		public ObservableCollection<Plugin> Plugins = new ObservableCollection<Plugin>();
		public ObservableCollection<EditableTag> TagsList = new ObservableCollection<EditableTag>();

		private Plugin _currentPlugin;
		public Plugin CurrentPlugin
		{
			get => _currentPlugin;
			set => SetProperty(ref _currentPlugin, value);
		}

		public string Arg = "";

		public ArchiveEditViewModel(EventsService events)
		{
			Events = events;
			SaveCommand = new AsyncRelayCommand(SaveArchive, () => !Saving);
			UsePluginCommand = new AsyncRelayCommand(UsePlugin, () => !Saving);
			ReloadCommand = new AsyncRelayCommand(ReloadArchive, () => !Saving);

			TagCommand = new RelayCommand<EditableTag>(HandleTagCommand, (_) => !Saving);
		}

		public async Task LoadArchive(Archive archive)
		{
			Archive = archive;
			Title = archive.title;
			ReloadTagsList(archive.tags);

			OnPropertyChanged("Title");
			OnPropertyChanged("Archive");
			var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
			Plugins.Clear();
			plugins.ForEach(p => Plugins.Add(p));
			CurrentPlugin = Plugins.ElementAt(0);
		}

		private async Task ReloadArchive()
		{
			var result = await ArchivesProvider.GetArchive(Archive.arcid);
			if (result != null)
			{
				Title = result.title;
				ReloadTagsList(result.tags);
				OnPropertyChanged("Title");
			}
			var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
			Plugins.Clear();
			plugins.ForEach(p => Plugins.Add(p));
			CurrentPlugin = Plugins.ElementAt(0);
		}

		private async Task SaveArchive()
		{
			Saving = true;
			var tags = BuildTags();
			var result = await ArchivesProvider.UpdateArchive(Archive.arcid, Title, tags);
			if (result)
			{
				Archive.title = Title;
				Archive.tags = tags;
				Tags = BuildTags();
				OnPropertyChanged("Archive");
			}
			Saving = false;
		}

		private async Task UsePlugin()
		{
			await SaveArchive();
			Saving = true;
			var result = await ServerProvider.UsePlugin(CurrentPlugin.@namespace, Archive.arcid, Arg);
			if (result != null)
			{
				if (result.success)
				{
					if (!string.IsNullOrEmpty(result.data.new_tags))
					{
						foreach (var t in result.data.new_tags.Split(','))
							TagsList.Insert(TagsList.Count - 1, new EditableTag { Tag = t.Trim(), Command = TagCommand });
						Tags = BuildTags();
					}
				}
				else
				{
					Events.ShowNotification("Error while fetching tags", result.error, 0);
				}
			}
			Saving = false;
		}

		private string BuildTags()
		{
			var result = "";
			foreach (var t in TagsList)
				if (!(t is AddTag))
					result += t.Tag + ", ";
			return result.Trim().TrimEnd(',');
		}

		private void HandleTagCommand(EditableTag tag)
		{
			if (tag is AddTag)
				TagsList.Insert(TagsList.Count - 1, new EditableTag { Tag = "", Command = TagCommand });
			else
				TagsList.Remove(tag);
		}

		private void ReloadTagsList(string tags)
		{
			TagsList.Clear();
			foreach (var t in tags.Split(','))
				TagsList.Add(new EditableTag { Tag = t.Trim(), Command = TagCommand });
			TagsList.Add(new AddTag { Command = TagCommand });
			Tags = BuildTags();
		}

	}

	public class EditableTag
	{
		public string Tag { get; set; }
		public RelayCommand<EditableTag> Command { get; internal set; }
	}

	public class AddTag : EditableTag
	{

	}
}
