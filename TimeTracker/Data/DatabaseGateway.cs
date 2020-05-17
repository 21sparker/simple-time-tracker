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

        public List<Task> LoadTasks()
        {
            return LoadTasks(ConvertToUnixTime(DateTime.Today));
        }

        public List<Task> LoadTasks(long dateTracked)
        {
            string sql =
                "SELECT * FROM Task JOIN " +
                "(SELECT DateTracked, TaskId, SUM(SecondsTracked) AS SecondsTracked FROM TaskHistory " +
                "GROUP BY DateTracked, TaskId) AS TH " +
                "ON Task.TaskId = TH.TaskId " +
                "WHERE Task.DeletedDateTime IS NULL AND TH.DateTracked = @DateTracked;";
            return _DBConnection.Query<Task>(sql, new { DateTracked = dateTracked }).AsList();
        }


        private static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)(datetime - sTime).TotalSeconds;
        }
    }
}
