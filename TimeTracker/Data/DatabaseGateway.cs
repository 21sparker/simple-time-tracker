using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dapper;

namespace TimeTracker
{
    public class DatabaseGateway
    {
        private readonly System.Data.SQLite.SQLiteConnection _DBConnection;

        public DatabaseGateway(string databasePath)
        {
            _DBConnection = new System.Data.SQLite.SQLiteConnection(databasePath);
        }

        public void Close()
        {
            _DBConnection.Close();
        }
    }
}
