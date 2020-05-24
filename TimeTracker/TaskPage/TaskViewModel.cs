﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    public class TaskViewModel : ObservableObject
    {
        private DatabaseGateway _databaseGateway;

        public TaskViewModel(TaskItem task, DatabaseGateway databaseGateway)
        {
            MainTask = task;
            _databaseGateway = databaseGateway;
            _secondsTracked = task.SecondsTracked;
        }

        public TaskItem MainTask { get; private set; }

        public string Description
        {
            get { return MainTask.Description; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    MainTask.Description = value;
                    _databaseGateway.UpdateTask(MainTask);
                    OnPropertyChanged("Description");
                }
            }
        }

        private long? _secondsTracked;
        public long? SecondsTracked
        {
            get { return _secondsTracked ?? 0; }
            set
            {
                _secondsTracked = value;
                OnPropertyChanged("SecondsTracked");
            }
        }

        private bool _isTracking;
        public bool IsTracking
        {
            get { return _isTracking; }
            set
            {
                _isTracking = value;
                OnPropertyChanged("IsTracking");
            }
        }

        public void AddTrackedTime(long seconds)
        {
            long dateTracked = Utilities.ConvertToUnixTime(DateTime.Today);
            _databaseGateway.InsertNewTaskHistoryItem(MainTask.TaskId, dateTracked, seconds);
        }

        private WBSViewModel _wbsVM;
        public WBSViewModel WBSVM
        {
            get { return _wbsVM; }
            set
            {
                if (value != null)
                {
                    _wbsVM = value;
                    MainTask.WBSId = _wbsVM.WBSItem.WBSId;
                    _databaseGateway.UpdateTask(MainTask);
                    OnPropertyChanged("WBSVM");
                }
            }
        }
    }
}
