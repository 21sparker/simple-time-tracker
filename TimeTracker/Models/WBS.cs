using Dapper.Contrib.Extensions;

namespace TimeTracker
{
    public class WBS
    {
        [Key]
        public int WBSId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public long CreatedDateTime { get; set; }

        public long? DeletedDateTime { get; set; }
    }
}
