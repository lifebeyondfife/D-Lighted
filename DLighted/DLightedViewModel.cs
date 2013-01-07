/*
  Copyright © Iain McDonald 2011
  
  This file is part of D-Lighted.

	D-Lighted is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	D-Lighted is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with D-Lighted.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Input;

using LbF.Database;
using System.IO;
using System.Windows.Threading;

namespace LbF.DLighted
{
	public class DLightedViewModel : DependencyObject, IDisposable
	{
		#region Dependency Properties

		public static readonly DependencyProperty ListItemsProperty =
			DependencyProperty.Register("ListItems", typeof(IList<string>), typeof(DLightedViewModel), new UIPropertyMetadata());
		public IList<string> ListItems
		{
			get { return (IList<string>) GetUIValue(ListItemsProperty); }
			set { SetUIValue(ListItemsProperty, value); }
		}

		public static readonly DependencyProperty DatabaseTableProperty =
			DependencyProperty.Register("DatabaseTable", typeof(DataView), typeof(DLightedViewModel), new UIPropertyMetadata());
		public DataView DatabaseTable
		{
			get { return (DataView) GetUIValue(DatabaseTableProperty); }
			set { SetUIValue(DatabaseTableProperty, value); }
		}

		public static readonly DependencyProperty TableNameProperty =
			DependencyProperty.Register("TableName", typeof(string), typeof(DLightedViewModel), new UIPropertyMetadata());
		public string TableName
		{
			get { return (string) GetUIValue(TableNameProperty); }
			set { SetUIValue(TableNameProperty, value); }
		}

		public static readonly DependencyProperty ConnectionNameProperty =
			DependencyProperty.Register("ConnectionName", typeof(string), typeof(DLightedViewModel), new UIPropertyMetadata());
		public string ConnectionName
		{
			get { return (string) GetUIValue(ConnectionNameProperty); }
			set { SetUIValue(ConnectionNameProperty, value); }
		}

		public static readonly DependencyProperty ServerNameProperty =
			DependencyProperty.Register("ServerName", typeof(string), typeof(DLightedViewModel), new UIPropertyMetadata());
		public string ServerName
		{
			get { return (string) GetUIValue(ServerNameProperty); }
			set { SetUIValue(ServerNameProperty, value); }
		}

		public static readonly DependencyProperty DatabaseNameProperty =
			DependencyProperty.Register("DatabaseName", typeof(string), typeof(DLightedViewModel), new UIPropertyMetadata());
		public string DatabaseName
		{
			get { return (string) GetUIValue(DatabaseNameProperty); }
			set { SetUIValue(DatabaseNameProperty, value); }
		}

		public static readonly DependencyProperty UserIDProperty =
			DependencyProperty.Register("UserID", typeof(string), typeof(DLightedViewModel), new UIPropertyMetadata());
		public string UserID
		{
			get { return (string) GetUIValue(UserIDProperty); }
			set { SetUIValue(UserIDProperty, value); }
		}

		private delegate object GetValueDelegate(DependencyProperty property);
		private object GetUIValue(DependencyProperty property)
		{
			return this.UIDispatcher.Invoke(new GetValueDelegate(GetValue), property);
		}

		private delegate void SetValueDelegate(DependencyProperty property, object value);
		private void SetUIValue(DependencyProperty property, object value)
		{
			this.UIDispatcher.Invoke(new SetValueDelegate(SetValue), property, value);
		}

		#endregion

		#region ViewModel - Model Interaction

		internal string Password { get; set; }
		public int RowsToShow { get; set; }
	
		public ICommand CommitCommand { get; set; }
		public ICommand NextRecordsCommand { get; set; }
		public ICommand PreviousRecordsCommand { get; set; }
		public ICommand MSSQLCommand { get; set; }
		public ICommand MySQLCommand { get; set; }
		public ICommand SQLiteCommand { get; set; }
		public ICommand OracleCommand { get; set; }
		public ICommand PostgreSQLCommand { get; set; }
	
		internal IDatabaseManager DatabaseManager { get; set; }
		internal DataColumn[] PrimaryKey { get; set; }
		internal int RowCountTotal { get; set; }
		internal int StartIndex { get; set; }
		public Dispatcher UIDispatcher { get; set; }
		public DLightedView View { get; set; }
		private static object ConnectionObject = new object();
		public bool Locked { get; set; }

		internal void UpdateTable(string tableName)
		{
			lock (DLightedViewModel.ConnectionObject)
			{
				this.Locked = true;
				
				int totalRows;
				var databaseTable = this.DatabaseManager.GetTable(ref tableName, this.RowsToShow, out totalRows);

				this.TableName = string.Format("{0} [{1} rows]", tableName, totalRows);
				this.RowCountTotal = totalRows;
				this.StartIndex = 0;

				if (totalRows != 0)
					this.PrimaryKey = databaseTable.Table.PrimaryKey;

				this.DatabaseTable = databaseTable;
			
				this.Locked = false;
			}
		}

		internal void GetTableList()
		{
			lock (DLightedViewModel.ConnectionObject)
			{
				this.Locked = true;

				if (this.DatabaseManager == null)
				{
					this.Locked = false;
					return;
				}

				try
				{
					this.ListItems = this.DatabaseManager.GetTableList();
				}
				catch (DbException exception)
				{
					this.ConnectionName = exception.Message;
				}
				finally
				{
					this.Locked = false;
				}
			}
		}

		internal void Connect(IDatabaseCreator databaseCreator)
		{
			lock (DLightedViewModel.ConnectionObject)
			{
				this.Locked = true;

				ClearExistingConnection();

				try
				{
					string error;
					this.ConnectionName = "Attempting connection...";
					this.DatabaseManager = databaseCreator.DatabaseFactory(this.ServerName, this.DatabaseName, this.UserID, this.Password, out error);
					this.Password = string.Empty;

					this.ConnectionName = string.IsNullOrWhiteSpace(error) ?
						string.Format("{0} - {1}", this.ServerName, this.DatabaseName) : error;
				}
				catch (DbException exception)
				{
					this.ConnectionName = exception.Message;
				}
				finally
				{
					this.Locked = false;
				}
			}
		}

		private void ClearExistingConnection()
		{
			if (this.DatabaseManager == null)
				return;

			this.DatabaseManager.Close();
			this.StartIndex = 0;
			this.RowCountTotal = 0;
			this.DatabaseManager = null;
			this.ListItems = null;
			this.ConnectionName = DLightedViewModel.NotConnectedString;
			this.TableName = string.Empty;
			this.DatabaseTable = null;
		}

		internal void PreviousRecords()
		{
			this.StartIndex = Math.Max(0, this.StartIndex - this.RowsToShow);
			this.DatabaseTable = this.DatabaseManager.CommitChanges(this.DatabaseTable, this.StartIndex, this.RowsToShow);
		}

		internal void NextRecords()
		{
			this.StartIndex = Math.Min(this.RowCountTotal - 1, this.StartIndex + this.RowsToShow);

			this.DatabaseTable = this.DatabaseManager.CommitChanges(this.DatabaseTable, this.StartIndex, this.RowsToShow);

			if (this.StartIndex + this.RowsToShow >= this.RowCountTotal)
				this.StartIndex = this.RowCountTotal;
		}

		#endregion

		#region Dispose of Database Connection

		public void Dispose()
		{
			ClearExistingConnection();
		}

		#endregion

		#region Config File Reading / Writing

		private Configuration GetConfigurationManager()
		{
			var applicationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			if (!File.Exists(applicationDirectory + "\\D-Lighted\\DLighted.exe.config"))
				return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			var exeConfigMapping = new ExeConfigurationFileMap();
			exeConfigMapping.ExeConfigFilename = applicationDirectory + "\\D-Lighted\\DLighted.exe.config";
			return ConfigurationManager.OpenMappedExeConfiguration(exeConfigMapping, ConfigurationUserLevel.None);
		}

		private void ReadConfigFile()
		{
			var config = GetConfigurationManager();

			if (config == null)
				return;

			this.ServerName = GetSetting(config, "ServerName");
			this.DatabaseName = GetSetting(config, "DatabaseName");
			this.UserID = GetSetting(config, "UserID");
		}

		internal void UpdateConfigFile()
		{
			var config = GetConfigurationManager();

			if (config == null)
				return;

			SetOrUpdateSetting(config, "ServerName", this.ServerName);
			SetOrUpdateSetting(config, "DatabaseName", this.DatabaseName);
			SetOrUpdateSetting(config, "UserID", this.UserID);

			try
			{
				config.Save(ConfigurationSaveMode.Modified, true);
				ConfigurationManager.RefreshSection("appSettings");
			}
			catch (ConfigurationErrorsException)
			{ }
		}

		private static string GetSetting(Configuration config, string propertyName)
		{
			var propertySetting = config.AppSettings.Settings[propertyName];
			return propertySetting == null ? string.Empty : propertySetting.Value;
		}

		private static void SetOrUpdateSetting(Configuration config, string propertyName, string property)
		{
			if (config.AppSettings.Settings[propertyName] != null)
				config.AppSettings.Settings[propertyName].Value = property;
			else
				config.AppSettings.Settings.Add(propertyName, property);
		}

		#endregion

		private readonly static string NotConnectedString = "Not Connected";

		public DLightedViewModel()
		{
			this.StartIndex = 0;
			this.RowsToShow = 1000;
			this.Locked = false;
			this.UIDispatcher = this.Dispatcher;
			this.ConnectionName = DLightedViewModel.NotConnectedString;
			var table = new DataTable();
			table.Columns.Add("<-- Database Table List Here");
			table.LoadDataRow(new string[] { string.Empty }, true);
			this.PrimaryKey = new DataColumn[0];
			this.DatabaseTable = new DataView(table);

			ReadConfigFile();

			this.CommitCommand = new DLightedCommitCommand(this);
			this.PreviousRecordsCommand = new DLightedPreviousRecordsCommand(this);
			this.NextRecordsCommand = new DLightedNextRecordsCommand(this);

			this.MSSQLCommand = new DLightedMSSQLConnectCommand(this);
			this.MySQLCommand = new DLightedMySQLConnectCommand(this);
			this.SQLiteCommand = new DLightedSQLiteConnectCommand(this);
			this.OracleCommand = new DLightedOracleConnectCommand(this);
			this.PostgreSQLCommand = new DLightedPostgreSQLConnectCommand(this);
		}
	}
}
