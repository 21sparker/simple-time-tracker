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

        public List<Task> LoadTasks(bool activeTasksOnly = true)
        {
            string sql =
                "SELECT * FROM Task JOIN " +
                "(SELECT DateTracked, TaskId, SUM(SecondsTracked) AS SecondsTracked FROM TaskHistory " +
                "GROUP BY DateTracked, TaskId) AS TH " +
                "ON Task.TaskId = TH.TaskId";

            if (activeTasksOnly)
            {
                sql += " WHERE Task.DeletedDateTime IS NULL";
            }
                
            return _DBConnection.Query<Task>(sql).AsList();
        }
    }
}
