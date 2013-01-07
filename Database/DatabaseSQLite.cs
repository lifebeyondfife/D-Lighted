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
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using LbF.Database;

namespace LbF.DLighted
{
	public class DatabaseCreatorSQLite : DatabaseCreator
	{
		internal DatabaseManagerSQLite DatabaseManager { get; private set; }

		public override IDatabaseManager DatabaseFactory(string serverName, string databaseName, string userID, string password, out string error)
		{
			error = string.Empty;
			this.DatabaseManager = new DatabaseManagerSQLite();
			this.DatabaseManager.DatabaseConnection = new SQLiteConnection(string.Format("Data Source=\"{0}\";Version=3;", databaseName));
			this.DatabaseManager.DatabaseConnection.Open();
			
			return this.DatabaseManager;
		}
	}

	internal class DatabaseManagerSQLite : DatabaseManager
	{
		public override IList<string> GetTableList()
		{
			IDbCommand getTableListCommand = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;");

			return GetTableListBase(getTableListCommand);
		}

		public override DataView GetTable(ref string tableName, int topMostRowCount, out int totalRows)
		{
			this.DataTable = new DataTable();
			this.DataAdapter = new SQLiteDataAdapter(string.Format("SELECT * FROM {0}", tableName),
				this.DatabaseConnection as SQLiteConnection);
			this.CommandBuilder = new SQLiteCommandBuilder(this.DataAdapter as SQLiteDataAdapter);

			return GetTableBase(ref tableName, topMostRowCount, out totalRows);
		}
	}
}