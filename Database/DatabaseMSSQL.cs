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
using System.Data.Linq;
using System.Data.SqlClient;

namespace LbF.Database
{
	public class DatabaseCreatorMSSQL : DatabaseCreator
	{
		internal DatabaseManagerMSSQL DatabaseManager { get; private set; }

		public override IDatabaseManager DatabaseFactory(string serverName, string databaseName, string userID, string password, out string error)
		{
			error = string.Empty;
			string server;
			int port;

			if (!DatabaseCreator.GetServerPort(serverName, out server, out port))
			{
				error = "Cannot parse Server Name and Port Number";
				return null;
			}

			var connectionString = (string.IsNullOrEmpty(userID) && string.IsNullOrEmpty(password)) ?
				"Database={1};Trusted_Connection=Yes;" :
				"Database={1};User ID={2};Password={3};";

			if (port > 0)
				connectionString = "Data Source={0},{4};" + connectionString;
			else
				connectionString = "Server={0};" + connectionString;

			this.DatabaseManager = new DatabaseManagerMSSQL();
			DataContext database = new DataContext(string.Format(connectionString, server, databaseName, userID, password, port));

			this.DatabaseManager.DatabaseConnection = database.Connection;
			this.DatabaseManager.DatabaseConnection.Open();

			return this.DatabaseManager;
		}
	}

	internal class DatabaseManagerMSSQL : DatabaseManager
	{
		public override IList<string> GetTableList()
		{
			IDbCommand getTableListCommand = new SqlCommand("SELECT table_schema, table_name FROM INFORMATION_SCHEMA.TABLES ORDER BY table_name");

			return GetTableListBase(getTableListCommand);
		}

		public override DataView GetTable(ref string tableName, int topMostRowCount, out int totalRows)
		{
			this.DataTable = new DataTable();
			this.DataAdapter = new SqlDataAdapter("SELECT * FROM " + tableName, this.DatabaseConnection as SqlConnection);
			this.CommandBuilder = new SqlCommandBuilder(this.DataAdapter as SqlDataAdapter);
			this.CommandBuilder.QuotePrefix = "[";
			this.CommandBuilder.QuoteSuffix = "]";

			return GetTableBase(ref tableName, topMostRowCount, out totalRows);
		}
	}
}