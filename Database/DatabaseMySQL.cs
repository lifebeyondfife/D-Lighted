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
using MySql.Data.MySqlClient;

namespace LbF.Database
{
	public class DatabaseCreatorMySQL : DatabaseCreator
	{
		internal DatabaseManagerMySQL DatabaseManager { get; private set; }

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

			var connectionString = "Server={0};Database={1};Uid={2};Pwd={3};";
			if (port > 0)
				connectionString += "Port={4};";

			this.DatabaseManager = new DatabaseManagerMySQL();
			this.DatabaseManager.DatabaseConnection = new MySqlConnection(
				string.Format(connectionString, server, databaseName, userID, password, port));
			
			this.DatabaseManager.DatabaseConnection.Open();

			return this.DatabaseManager;
		}
	}

	internal class DatabaseManagerMySQL : DatabaseManager
	{
		public override IList<string> GetTableList()
		{
			IDbCommand getTableListCommand = new MySqlCommand(string.Format
				("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = '{0}'", this.DatabaseConnection.Database));

			return GetTableListBase(getTableListCommand);
		}

		public override DataView GetTable(ref string tableName, int topMostRowCount, out int totalRows)
		{
			this.DataTable = new DataTable();
			this.DataAdapter = new MySqlDataAdapter(string.Format("SELECT * FROM {0}.{1}", this.DatabaseConnection.Database, tableName),
				this.DatabaseConnection as MySqlConnection);
			this.CommandBuilder = new MySqlCommandBuilder(this.DataAdapter as MySqlDataAdapter);

			return GetTableBase(ref tableName, topMostRowCount, out totalRows);
		}
	}
}