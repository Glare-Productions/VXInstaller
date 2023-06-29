using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VXInstaller
{
	internal enum PluginType
	{
		Folder,
		ZIP
	}

	/// <summary>
	/// Info for a plugin
	/// </summary>
	internal class PluginInfo : INotifyPropertyChanged
	{
		/// <summary>
		/// Type of plugin container
		/// </summary>
		public PluginType PluginType { get; set; }

		/// <summary>
		/// The full path for the plugin's ZIP file.
		/// </summary>
		public string Path;

		/// <summary>
		/// The plugin's name (usually the filename withoout extension)
		/// </summary>
		public string Name;

		/// <summary>
		/// If true this will be installed.
		/// </summary>
		private bool _Selected;
		public bool Selected
		{
			get => _Selected;
			set { _Selected = value; OnPropertyChanged(); }
		}

		public override string ToString() => Name;

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}
}
