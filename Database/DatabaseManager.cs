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
using System.Data;
using System.Data.Common;
using System.Linq;
using MySql.Data.Types;

namespace LbF.Database
{
	public interface IDatabaseManager
	{
		void Close();
		void Connect(IDbConnection databaseConnection);
		DataView CommitChanges(DataView updatedView, int startIndex, int topMostRowCount);
		DataView GetTable(ref string tableName, int topMostRowCount, out int totalRows);
		IList<string> GetTableList();
	}

	public abstract class DatabaseManager : IDatabaseManager
	{
		internal IDbConnection DatabaseConnection { get; set; }
		protected DbDataAdapter DataAdapter { get; set; }
		protected DbCommandBuilder CommandBuilder { get; set; }
		protected DataTable DataTable { get; set; }

		public abstract DataView GetTable(ref string tableName, int topMostRowCount, out int totalRows);
		public abstract IList<string> GetTableList();

		public void Connect(IDbConnection databaseConnection)
		{
			this.DatabaseConnection = databaseConnection;
		}

		public void Close()
		{
			this.DatabaseConnection.Close();
			this.DatabaseConnection.Dispose();
		}

		protected DataView GetDataView(int startIndex, int topMostRowCount)
		{
			if (this.DataTable.Rows.Count <= topMostRowCount)
			{
				var dataView = this.DataTable.Rows.Cast<DataRow>().OrderBy(row => row[0]).CopyToDataTable().AsDataView();
				this.DataAdapter.FillSchema(dataView.Table, SchemaType.Source);
				this.DataTable.Rows.Clear();
				return dataView;
			}

			//	Bookmark top 'x' rows, and create a table of the remainding rows left unshown
			var topRows = this.DataTable.Rows.Cast<DataRow>().OrderBy(row => row[0]).Skip(startIndex).Take(topMostRowCount);
			var otherRowsTable = this.DataTable.Rows.Cast<DataRow>().Except(topRows).CopyToDataTable();
			this.DataAdapter.FillSchema(otherRowsTable, SchemaType.Source);

			//	Create table from bookmarked top 'x'
			var topRowsTable = topRows.CopyToDataTable();
			this.DataAdapter.FillSchema(topRowsTable, SchemaType.Source);

			//	Table stored in class contains just the remainding data
			this.DataTable = otherRowsTable;

			return topRowsTable.AsDataView();
		}

		public DataView CommitChanges(DataView updatedView, int startIndex, int topMostRowCount)
		{
			foreach (DataRow row in updatedView.Table.Rows)
				this.DataTable.ImportRow(row);

			try
			{
				this.DataAdapter.Update(this.DataTable);
			}
			catch (DBConcurrencyException)
			{ }

			foreach (DataRow row in this.DataTable.Rows)
				row.AcceptChanges();

			return GetDataView(startIndex, topMostRowCount);
		}

		protected DataView GetTableBase(ref string tableName, int topMostRowCount, out int totalRows)
		{
			try
			{
				this.DataAdapter.UpdateCommand = this.CommandBuilder.GetUpdateCommand();
				this.DataAdapter.InsertCommand = this.CommandBuilder.GetInsertCommand();
				this.DataAdapter.DeleteCommand = this.CommandBuilder.GetDeleteCommand();
			}
			catch (InvalidOperationException ioe)
			{
				totalRows = 0;
				tableName = ioe.Message;
				return null;
			}

			this.DataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
			this.DataAdapter.FillSchema(this.DataTable, SchemaType.Source);

			try
			{
				totalRows = this.DataAdapter.Fill(this.DataTable);
			}
			catch (ConstraintException ce)
			{
				totalRows = 0;
				tableName = ce.Message;
				return null;
			}
			catch (MySqlConversionException ce)
			{
				totalRows = 0;
				tableName = ce.Message;
				return null;
			}

			if (totalRows == 0)
				return null;

			return GetDataView(0, topMostRowCount);
		}

		protected IList<string> GetTableListBase(IDbCommand getTableListCommand)
		{
			getTableListCommand.Connection = this.DatabaseConnection;
			getTableListCommand.CommandType = CommandType.Text;

			IDataReader reader = getTableListCommand.ExecuteReader();

			var list = new List<string>();
			try
			{
				while (reader.Read())
					list.Add(reader.FieldCount == 1 ?
						reader.GetString(0) :
						reader.GetString(0) + "." + reader.GetString(1)
					);
			}
			finally
			{
				reader.Close();
			}
			
			return list;
		}
	}
}
