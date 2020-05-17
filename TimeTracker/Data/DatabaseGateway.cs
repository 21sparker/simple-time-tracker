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

        public List<TaskItem> LoadTasks(bool activeTasksOnly = true)
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
                
            return _DBConnection.Query<TaskItem>(sql).AsList();
        }

        public TaskItem InsertNewTask(string description, long createdDateTime)
        {
            string sql = "INSERT INTO Task (Description, CreatedDateTime) VALUES (@Description, @CreatedDateTime)";
            _DBConnection.Execute(sql, new { Description = description, CreatedDateTime = createdDateTime });

            sql =
                "SELECT * FROM Task LEFT JOIN " +
                "(SELECT DateTracked, TaskId, SUM(SecondsTracked) AS SecondsTracked FROM TaskHistory " +
                "GROUP BY DateTracked, TaskId) AS TH " +
                "ON Task.TaskId = TH.TaskId " +
                "WHERE Task.Description = @Description AND CreatedDateTime = @CreatedDateTime";

            return _DBConnection.QueryFirst<TaskItem>(sql, new { Description = description, CreatedDateTime = createdDateTime });
        }

        public void DeleteTask(TaskItem task)
        {
            string sql = "UPDATE Task SET DeletedDateTime = @DeletedDateTime WHERE TaskId = @TaskId";
            long deletedDateTime = Utilities.ConvertToUnixTime(DateTime.Now);
            _DBConnection.Execute(sql, new { DeletedDateTime=deletedDateTime, TaskId=task.TaskId });
        }

        /// <summary>
        /// Updates a tasks attributes.
        /// Note, that the task parameter should have the updated properties already.
        /// </summary>
        /// <param name="task"></param>
        public void UpdateTask(TaskItem task)
        {
            string sql = "UPDATE Task SET Description = @Description WHERE TaskId = @TaskId";
            _DBConnection.Execute(sql, new { Description = task.Description, TaskId = task.TaskId });
        }

        
        public void InsertNewTaskHistoryItem(int taskId, long dateTracked, long secondsTracked)
        {
            string sql = "INSERT INTO TaskHistory (DateTracked, TaskId, SecondsTracked)" +
                         " VALUES (@DateTracked, @TaskId, @SecondsTracked)";
            _DBConnection.Execute(sql, new
            {
                DateTracked = dateTracked,
                TaskId = taskId,
                SecondsTracked = secondsTracked
            });
        }
    }
}
