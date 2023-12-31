﻿using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace VXInstaller
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private InstallManager InstallMan = new InstallManager();

		public MainWindow()
		{
			DataContext = InstallMan;
			InitializeComponent();

			InstallMan.Init(DetermineVersion());
			ListPlugins.Focus();
		}

		private int DetermineVersion()
		{
			// default to Ventuz 6
			int version = 6;

			string pattern = @"\d+";
			var match = Regex.Match(System.AppDomain.CurrentDomain.FriendlyName, pattern);
			if (match.Success && Int32.TryParse(match.Value, out int num)) 
			{
				version = num;
			}

			return version;
		}

		#region Event handlers

		/// <summary>
		/// Select destination folder.
		/// </summary>
		private void ChoseDirClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonOpenFileDialog();
			dlg.IsFolderPicker = true;
			
			dlg.InitialDirectory = BoxInstallDirectory.Text;

			if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
			{
				BoxInstallDirectory.Text = dlg.FileName;
			}
		}

		/// <summary>
		/// Click on exit.
		/// </summary>
		private void ExitClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#endregion

		private void InstallClick(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			(bool res, string msg) = InstallMan.Install();
			Mouse.OverrideCursor = null;
			MessageBox.Show(msg, "Install plugins", MessageBoxButton.OK, res ? MessageBoxImage.Information : MessageBoxImage.Warning);
			Close();
		}
	}
}
