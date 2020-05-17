using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dapper;
using Dapper.Contrib;

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
                "SELECT * FROM Task LEFT JOIN " +
                "(SELECT DateTracked, TaskId, SUM(SecondsTracked) AS SecondsTracked FROM TaskHistory " +
                "GROUP BY DateTracked, TaskId) AS TH " +
                "ON Task.TaskId = TH.TaskId";

            if (activeTasksOnly)
            {
                sql += " WHERE Task.DeletedDateTime IS NULL";
            }
                
            return _DBConnection.Query<Task>(sql).AsList();
        }

        public Task InsertNewTask(string description, long createdDateTime)
        {
            string sql = "INSERT INTO Task (Description, CreatedDateTime) VALUES (@Description, @CreatedDateTime)";
            _DBConnection.Execute(sql, new { Description = description, CreatedDateTime = createdDateTime });

            sql =
                "SELECT * FROM Task LEFT JOIN " +
                "(SELECT DateTracked, TaskId, SUM(SecondsTracked) AS SecondsTracked FROM TaskHistory " +
                "GROUP BY DateTracked, TaskId) AS TH " +
                "ON Task.TaskId = TH.TaskId " +
                "WHERE Task.Description = @Description AND CreatedDateTime = @CreatedDateTime";

            return _DBConnection.QueryFirst<Task>(sql, new { Description = description, CreatedDateTime = createdDateTime });
        }
    }
}
