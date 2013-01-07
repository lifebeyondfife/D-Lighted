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
using Npgsql;

namespace LbF.Database
{
	public class DatabaseCreatorPostgreSQL : DatabaseCreator
	{
		internal DatabaseManagerPostgreSQL DatabaseManager { get; private set; }

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
				"Server={0};Database={1};Integrated Security=true;" :
				"Server={0};Database={1};User Id={2};Password={3};";

			if (port > 0)
				connectionString += "Port={4};";

			this.DatabaseManager = new DatabaseManagerPostgreSQL();
			this.DatabaseManager.DatabaseConnection = 
				new NpgsqlConnection(string.Format(connectionString, server, databaseName, userID, password, port));
			this.DatabaseManager.DatabaseConnection.Open();

			return this.DatabaseManager;
		}
	}

	public class DatabaseManagerPostgreSQL : DatabaseManager
	{
		public override IList<string> GetTableList()
		{
			IDbCommand getTableListCommand = new NpgsqlCommand("SELECT tablename FROM pg_tables where schemaname = 'public';");

			return GetTableListBase(getTableListCommand);
		}

		public override DataView GetTable(ref string tableName, int topMostRowCount, out int totalRows)
		{
			this.DataTable = new DataTable();
			this.DataAdapter = new NpgsqlDataAdapter(string.Format("SELECT * FROM \"{0}\"", tableName),
				this.DatabaseConnection as NpgsqlConnection);
			this.CommandBuilder = new NpgsqlCommandBuilder(this.DataAdapter as NpgsqlDataAdapter);

			return GetTableBase(ref tableName, topMostRowCount, out totalRows);
		}
	}
}