using System;
using System.Collections.Generic;
using System.IO;

namespace VXInstaller
{
	internal class FileSystem
	{
		/// <summary>
		/// Check a given directory for a plugin DLL
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		private static bool CheckDirForPlugin(string dir)
		{
			try
			{
				var files = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);

				foreach (var file in files)
				{
					if (file.EndsWith("vx.dll")) return true;
				}
			}
			catch
			{
			}

            return false;
		}

		/// <summary>
		/// Find all plugins in the plugin directory next to the Application
		/// </summary>
		/// <returns>A list of plugins, might be empty. Each plugin is represented as a tuple 
		/// of the plugin name and the full path</returns>
		internal static List<PluginInfo> GetPlugins()
		{
			var plugins = new List<PluginInfo>();

			string pd = Path.GetFullPath(".");

			if (Directory.Exists(pd))
			{
				// list all zips
				var zips = Directory.GetFiles(pd, "*.zip");
                foreach (var zip in zips)
                {
					var plugin = new PluginInfo
					{
						PluginType = PluginType.ZIP,
						Name = Path.GetFileNameWithoutExtension(zip),
						Path = zip,
					};
					plugins.Add(plugin);
                }

				// list all plugin dirs
				var dirs = Directory.GetDirectories(pd);
				foreach (var dir in dirs)
				{
					if (CheckDirForPlugin(dir))
					{
						var plugin = new PluginInfo
						{
							PluginType = PluginType.Folder,
							Name = Path.GetFileNameWithoutExtension(dir),
							Path = dir,
						};
						plugins.Add(plugin);
					}
				}
            }

			return plugins;
		}

		/// <summary>
		/// Get or create the VX directory in the current user's
		/// documents folder.
		/// </summary>
		/// <returns>The VX directory or string.Empty in case of an error</returns>
		internal static string GetVXDir()
		{
			try
			{
				string doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				if (Directory.Exists(doc))
				{
					string vx = Path.Combine(doc, "Ventuz6", "Vx");
					if (!Directory.Exists(doc))
					{
						Directory.CreateDirectory(vx);
					}

					return vx;
				}
			}
			catch
			{
			}

			return string.Empty;
		}

		internal static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
		{
			// Get information about the source directory
			var dir = new DirectoryInfo(sourceDir);

			// Check if the source directory exists
			if (!dir.Exists)
				throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

			// Cache directories before we start copying
			DirectoryInfo[] dirs = dir.GetDirectories();

			// Create the destination directory
			Directory.CreateDirectory(destinationDir);

			// Get the files in the source directory and copy to the destination directory
			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(destinationDir, file.Name);
				file.CopyTo(targetFilePath);
			}

			// If recursive and copying subdirectories, recursively call this method
			if (recursive)
			{
				foreach (DirectoryInfo subDir in dirs)
				{
					string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
					CopyDirectory(subDir.FullName, newDestinationDir, true);
				}
			}
		}
	}
}
