using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VXInstaller
{

	internal class InstallManager : INotifyPropertyChanged
	{
		#region Properties with notification

		private string _TextInstallDir;
		public string TextInstallDir
		{
			get { return _TextInstallDir; }
			set { _TextInstallDir = value; OnPropertyChanged(); }
		}

		private string _TextPlugins;
		public string TextPlugins
		{
			get { return _TextPlugins; }
			set { _TextPlugins = value; OnPropertyChanged(); }
		}

		private string _InstallDir;
		public string InstallDir
		{
			get { return _InstallDir; }
			set { _InstallDir = value; OnPropertyChanged(); }
		}

		private ObservableCollection<PluginInfo> _Plugins = new ObservableCollection<PluginInfo>();
		public ObservableCollection<PluginInfo> Plugins
		{
			get { return _Plugins; }
			set { _Plugins = value; OnPropertyChanged(); }
		}


		#endregion

		/// <summary>
		/// Collect plugins and paths and whatever we need.
		/// </summary>
		internal void Init()
		{
			// retrieve all plugins in the plugin dir
			var plugins = FileSystem.GetPlugins();

			// text for the plugin area depends on the result
			TextPlugins = (plugins.Count > 0)
				? Properties.Resources.PluginsFound
				: Properties.Resources.PluginsError;

			// add plugins to list that is bound to the list box
			foreach (var plugin in plugins)
			{
				plugin.Selected = true;
				Plugins.Add(plugin);
			}

			// also set text for the VX dir
			var dst = FileSystem.GetVXDir();
			TextInstallDir = string.IsNullOrEmpty(dst)
				? Properties.Resources.InstallDirTextError
				: Properties.Resources.InstallDirTextFound;

			InstallDir = dst;
		}

		/// <summary>
		/// Install a given ZIP plugin
		/// </summary>
		/// <param name="plugin">The plugin</param>
		/// <param name="tempRoot">The temp dir</param>
		/// <returns>True if the plugin could be installed.</returns>
		private bool InstallZip(PluginInfo plugin, string tempRoot)
		{
			// we need a temp dir
			string temp = Path.Combine(tempRoot, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

			// unzip to temp
			try
			{
				ZipFile.ExtractToDirectory(plugin.Path, temp);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error installing {plugin.Name}: {ex.Message}");
				return false;
			}

			// check for a single sub dir in the temp (the plugin must be a folder)
			var subs = Directory.GetDirectories(temp);
			if (subs.Length != 1)
			{
				Console.WriteLine($"Error installing {plugin.Name}: The ZIP must contain exactly one directory with the plugin files");
				return false;
			}

			var sub = Path.GetFileName(subs[0]);
			var dstSub = Path.Combine(InstallDir, sub);

			// clean install: remove old plugin first
			if (Directory.Exists(dstSub))
			{
				try
				{
					Directory.Delete(dstSub, true);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error installing {plugin.Name}: {ex.Message}");
					return false;
				}
			}

			// now move files
			try
			{
				Directory.Move(subs[0], Path.Combine(InstallDir, sub));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error installing {plugin.Name}: {ex.Message}");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Install a given folder plugin
		/// </summary>
		/// <param name="plugin">The plugin</param>
		/// <returns>True if the plugin could be installed.</returns>
		private bool InstallFolder(PluginInfo plugin)
		{
			var sub = Path.GetFileName(plugin.Path);
			var dstSub = Path.Combine(InstallDir, sub);

			// clean install: remove old plugin first
			if (Directory.Exists(dstSub))
			{
				try
				{
					Directory.Delete(dstSub, true);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error installing {plugin.Name}: {ex.Message}");
					return false;
				}
			}

			// now copy files
			try
			{
				FileSystem.CopyDirectory(plugin.Path, dstSub, true);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error installing {plugin.Name}: {ex.Message}");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Install all selected plugins
		/// </summary>
		/// <returns>The result and a message to show.</returns>
		internal (bool res, string msg) Install()
		{
			// we need a temp dir. This is where we unpack
			// ZIPs to if there are any. We'll delete it in
			// the end. Make it in the VX dir (same drive) so
			// we are sure we can move unzipped files later.
			string temp = Path.Combine(InstallDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
			try
			{
				Directory.CreateDirectory(temp);
			}
			catch (Exception ex)
			{
                Console.WriteLine($"Could not create a temp directory: {ex.Message}");
				return (false, Properties.Resources.InstallNone);
            }

			// we count/store the installed plugins so we can report
			// on (partial) succes later.
			int expectedInstalls = 0;
			List<string> installedNames = new List<string>();

			// try to install all the plugins that have
			// been selected
			foreach (var plugin in Plugins)
			{
				if (plugin.Selected)
				{
					++expectedInstalls;
					bool installed = false;
					switch (plugin.PluginType)
					{
						case PluginType.ZIP:
							installed = InstallZip(plugin, temp);
							break;

						case PluginType.Folder:
							installed = InstallFolder(plugin);
							break;
					}

					if (installed)
					{
						installedNames.Add(plugin.Name);
					}
				}
            }

			// get rid of the temp dir
			CleanupDir(temp);

			// report on success or failure or partial success

			if (expectedInstalls == installedNames.Count)
			{
				return (true, Properties.Resources.InstallSuccess);
			}
			else if (installedNames.Count > 0)
			{
				return (false, string.Format(Properties.Resources.InstallPartial, string.Join(", ", installedNames.ToArray())));
			}
			else
			{
				return (false, Properties.Resources.InstallNone);
			}
		}

		/// <summary>
		/// Safe-delete a given dir
		/// </summary>
		/// <param name="path">Dir to remove</param>
		private void CleanupDir(string path)
		{
			try
			{
				Directory.Delete(path, true);
			}
			catch
			{
			}
		}

		
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}
}
