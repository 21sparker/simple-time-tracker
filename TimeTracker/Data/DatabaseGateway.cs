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

        public List<TaskItem> TaskItems { get; set; }
        public List<WBS> WBSs { get; set; }

        public DatabaseGateway(string databasePath)
        {
            _DBConnection = new System.Data.SQLite.SQLiteConnection(databasePath);
        }

        public void Close()
        {
            _DBConnection.Close();
        }
        
        public void LoadAllData()
        {
            TaskItems = new List<TaskItem>();
            WBSs = LoadWBSs();

            List<TaskItem> taskItems = LoadTasks();

            foreach (TaskItem taskItem in taskItems)
            {
                foreach (WBS wbs in WBSs)
                {
                    if (taskItem.WBSId == wbs.WBSId)
                    {
                        taskItem.WBSCode = wbs;
                        break;
                    }
                }
                TaskItems.Add(taskItem);
            }
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

            //List<TaskItem> taskItems = _DBConnection.Query<TaskItem, WBS, TaskItem>(
            //    sql,
            //    (taskItem, wbs) =>
            //    {
            //        taskItem.WBSCode = wbs;
            //        return taskItem;
            //    },
            //    splitOn: "WBSId").AsList();

            return _DBConnection.Query<TaskItem>(sql).AsList();
        }

        //public List<TaskItem> LoadTasks(bool activeTasksOnly = true)
        //{
        //    string sql =
        //        "SELECT * FROM Task LEFT JOIN " +
        //        "(SELECT DateTracked, TaskId, SUM(SecondsTracked) AS SecondsTracked FROM TaskHistory " +
        //        "GROUP BY DateTracked, TaskId) AS TH " +
        //        "ON Task.TaskId = TH.TaskId " +
        //        "LEFT JOIN WBS ON Task.WBSId = WBS.WBSId";

        //    if (activeTasksOnly)
        //    {
        //        sql += " WHERE Task.DeletedDateTime IS NULL";
        //    }

        //    List<TaskItem> taskItems = _DBConnection.Query<TaskItem, WBS, TaskItem>(
        //        sql,
        //        (taskItem, wbs) =>
        //        {
        //            taskItem.WBSCode = wbs;
        //            return taskItem;
        //        },
        //        splitOn: "WBSId").AsList();

        //    return taskItems;
        //}

        public List<WBS> LoadWBSs(bool activeWBSsOnly = true)
        {
            string sql = "SELECT * FROM WBS";

            if (activeWBSsOnly)
            {
                sql += " WHERE DeletedDateTime IS NULL";
            }

            return _DBConnection.Query<WBS>(sql).AsList();
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

        public WBS InsertNewWBS(string name, string code, long createdDateTime)
        {
            string sql = "INSERT INTO WBS (Name, Code, CreatedDateTime) VALUES (@Name, @Code, @CreatedDateTime)";
            _DBConnection.Execute(sql, new { Name = name, Code = code, CreatedDateTime = createdDateTime });

            sql =
                "SELECT * FROM WBS " +
                "WHERE WBS.Name = @Name AND WBS.Code = @Code AND CreatedDateTime = @CreatedDateTime";

            return _DBConnection.QueryFirst<WBS>(sql, new { Name = name, Code = code, CreatedDateTime = createdDateTime });
        }

        public void DeleteTask(TaskItem task)
        {
            string sql = "UPDATE Task SET DeletedDateTime = @DeletedDateTime WHERE TaskId = @TaskId";
            long deletedDateTime = Utilities.ConvertToUnixTime(DateTime.Now);
            _DBConnection.Execute(sql, new { DeletedDateTime=deletedDateTime, TaskId=task.TaskId });
        }

        public void DeleteWBS(WBS wbs)
        {
            string sql = "UPDATE WBS SET DeletedDateTime = @DeletedDateTime WHERE WBSId = @WBSId";
            long deletedDateTime = Utilities.ConvertToUnixTime(DateTime.Now);
            _DBConnection.Execute(sql, new { DeletedDateTime = deletedDateTime, WBSId = wbs.WBSId });
        }

        /// <summary>
        /// Updates a tasks attributes.
        /// Note, that the task parameter should have the updated properties already.
        /// </summary>
        /// <param name="task"></param>
        public void UpdateTask(TaskItem task)
        {
            string sql = "UPDATE Task SET Description = @Description, WBSId = @WBSId WHERE TaskId = @TaskId";
            _DBConnection.Execute(sql, new { Description = task.Description, WBSId = (int?)task.WBSId, 
                TaskId = task.TaskId });
        }

        public void UpdateWBS(WBS wbs)
        {
            string sql = "UPDATE WBS SET Name = @Name, Code = @Code WHERE WBSId = @WBSId";
            _DBConnection.Execute(sql, new { Name = wbs.Name, Code = wbs.Code, WBSId = wbs.WBSId });
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
