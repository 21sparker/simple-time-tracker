﻿using Dapper.Contrib.Extensions;

namespace TimeTracker
{
    public class TaskItem
    {
        [Key]
        public int TaskId { get; set; }

        public string Description { get; set; }

        public long CreatedDateTime { get; set; }

        public long? DeletedDateTime { get; set; }

        public int? WBSId { get; set; }

        [Computed]
        public WBS WBSCode { get; set; }

        [Computed]
        public long? DateTracked { get; set; }

        [Computed]
        public long? SecondsTracked { get; set; }

    }
}
