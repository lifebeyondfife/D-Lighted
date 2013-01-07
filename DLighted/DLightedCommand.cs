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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LbF.Database;
using Microsoft.Win32;

namespace LbF.DLighted
{
	public abstract class DLightedCommand : ICommand
	{
		protected readonly DLightedViewModel viewModel;

		public DLightedCommand(DependencyObject viewModel)
		{
			this.viewModel = (DLightedViewModel) viewModel;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public virtual bool CanExecute(object parameter)
		{
			return true;
		}

		protected async void ShowConnectDialog(Connect connect, DatabaseCreator databaseCreator)
		{
			connect.Icon = this.viewModel.View.Icon;
			connect.Owner = this.viewModel.View;
			var result = connect.ShowDialog();

			if (!result ?? true)
				return;

			this.viewModel.UpdateConfigFile();
			await Task.Factory.StartNew(() => this.viewModel.Connect(databaseCreator));
			await Task.Factory.StartNew(() => this.viewModel.GetTableList());
		}

		protected async void ShowOpenFileDialog(DatabaseCreator databaseCreator, string filter, string title)
		{
			OpenFileDialog fileOpen = new OpenFileDialog();
			fileOpen.Filter = filter;
			fileOpen.Title = title;

			var result = fileOpen.ShowDialog();

			if (!result ?? true)
				return;

			this.viewModel.ServerName = string.Empty;
			this.viewModel.DatabaseName = fileOpen.FileName;
			await Task.Factory.StartNew(() => this.viewModel.Connect(databaseCreator));
			await Task.Factory.StartNew(() => this.viewModel.GetTableList());
		}

		public abstract void Execute(object parameter);
	}

	public class DLightedCommitCommand : DLightedCommand
	{
		public DLightedCommitCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return !(this.viewModel.DatabaseManager == null || string.IsNullOrEmpty(this.viewModel.TableName) ||
				this.viewModel.DatabaseTable == null);
		}

		public override void Execute(object parameter)
		{
			this.viewModel.DatabaseTable = this.viewModel.DatabaseManager.CommitChanges
				(this.viewModel.DatabaseTable, this.viewModel.StartIndex, this.viewModel.RowsToShow);
		}
	}

	public class DLightedNextRecordsCommand : DLightedCommand
	{
		public DLightedNextRecordsCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return this.viewModel.StartIndex < this.viewModel.RowCountTotal;
		}

		public override void Execute(object parameter)
		{
			this.viewModel.NextRecords();
		}
	}

	public class DLightedPreviousRecordsCommand : DLightedCommand
	{
		public DLightedPreviousRecordsCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return this.viewModel.StartIndex > 0;
		}

		public override void Execute(object parameter)
		{
			this.viewModel.PreviousRecords();
		}
	}

	public class DLightedMSSQLConnectCommand : DLightedCommand
	{
		public DLightedMSSQLConnectCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return !this.viewModel.Locked;
		}

		public override void Execute(object parameter)
		{
			ShowConnectDialog(new Connect(this.viewModel, "Connect to a Microsoft SQL Database"), new DatabaseCreatorMSSQL());
		}
	}

	public class DLightedMySQLConnectCommand : DLightedCommand
	{
		public DLightedMySQLConnectCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return !this.viewModel.Locked;
		}

		public override void Execute(object parameter)
		{
			ShowConnectDialog(new Connect(this.viewModel, "Connect to a MySQL Database"), new DatabaseCreatorMySQL());
		}
	}

	public class DLightedOracleConnectCommand : DLightedCommand
	{
		public DLightedOracleConnectCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return !this.viewModel.Locked;
		}

		public override void Execute(object parameter)
		{
			var connect = new Connect(this.viewModel,  "Connect to an Oracle Database");
			connect.ServerLabel.Visibility = System.Windows.Visibility.Hidden;
			connect.ServerTextBox.Visibility = System.Windows.Visibility.Hidden;
			connect.DatabaseLabel.Content = "TNS";
			ShowConnectDialog(connect, new DatabaseCreatorOracle());
		}
	}

	public class DLightedPostgreSQLConnectCommand : DLightedCommand
	{
		public DLightedPostgreSQLConnectCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return !this.viewModel.Locked;
		}

		public override void Execute(object parameter)
		{
			ShowConnectDialog(new Connect(this.viewModel, "Connect to a PostgreSQL Database"), new DatabaseCreatorPostgreSQL());
		}
	}
	
	public class DLightedSQLiteConnectCommand : DLightedCommand
	{
		public DLightedSQLiteConnectCommand(DLightedViewModel viewModel)
			: base(viewModel)
		{
		}

		public override bool CanExecute(object parameter)
		{
			return !this.viewModel.Locked;
		}

		public override void Execute(object parameter)
		{
			var filter = "SQLite3 (*.db,*.sqlite) |*.db;*.db3;*.sqlite;*.sqlite3|All files (*.*)|*.*";
			var title = "Select SQLite3 Database to Open";

			ShowOpenFileDialog(new DatabaseCreatorSQLite(), filter, title);
		}
	}
}
