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
using Oracle.DataAccess.Client;

namespace LbF.Database
{
	public class DatabaseCreatorOracle : DatabaseCreator
	{
		internal DatabaseManagerOracle DatabaseManager { get; private set; }

		public override IDatabaseManager DatabaseFactory(string serverName, string databaseName, string userID, string password, out string error)
		{
			error = string.Empty;
			this.DatabaseManager = new DatabaseManagerOracle();
			this.DatabaseManager.DatabaseConnection = new OracleConnection(string.Format
				("User Id={0};Password={1};Data Source={2}", userID, password, databaseName));

			this.DatabaseManager.DatabaseConnection.Open();

			return this.DatabaseManager;
		}
	}

	internal class DatabaseManagerOracle : DatabaseManager
	{
		public override IList<string> GetTableList()
		{
			IDbCommand getTableListCommand = new OracleCommand
				("SELECT OBJECT_NAME FROM ALL_OBJECTS WHERE OBJECT_TYPE = 'TABLE' ORDER BY OBJECT_NAME");

			return GetTableListBase(getTableListCommand);
		}

		public override DataView GetTable(ref string tableName, int topMostRowCount, out int totalRows)
		{
			this.DataTable = new DataTable();
			this.DataAdapter = new OracleDataAdapter(string.Format("SELECT * FROM {0}", tableName),
				this.DatabaseConnection as OracleConnection);
			this.CommandBuilder = new OracleCommandBuilder(this.DataAdapter as OracleDataAdapter);

			return GetTableBase(ref tableName, topMostRowCount, out totalRows);
		}
	}
}
