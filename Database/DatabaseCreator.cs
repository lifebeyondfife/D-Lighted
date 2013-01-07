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

namespace LbF.Database
{
	public interface IDatabaseCreator
	{
		IDatabaseManager DatabaseFactory(string serverName, string databaseName, string userID, string password, out string error);
	}

	public abstract class DatabaseCreator : IDatabaseCreator
	{
		public abstract IDatabaseManager DatabaseFactory(string serverName, string databaseName, string userID, string password, out string error);

		protected static bool GetServerPort(string serverName, out string server, out int port)
		{
			server = serverName;
			port = 0;
			var serverPort = serverName.Split(':');

			if (serverPort.Length != 2)
				return true;

			if (!Int32.TryParse(serverPort[1], out port))
				return false;

			server = serverPort[0];
			return true;
		}
	}
}
