using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    public class WBSViewModel : ObservableObject
    {
        private DatabaseGateway _dbGateway;

        public WBSViewModel(WBS wbs, DatabaseGateway dbGateway)
        {
            WBSItem = wbs;
            _dbGateway = dbGateway;
        }

        public WBS WBSItem { get; private set; }
        public string Name
        {
            get 
            {
                Trace.WriteLine("Name was retrived for " + WBSItem.Name);
                return WBSItem.Name; 
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    WBSItem.Name = value;
                    _dbGateway.UpdateWBS(WBSItem);
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Code
        {
            get { return WBSItem.Code; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    WBSItem.Code = value;
                    _dbGateway.UpdateWBS(WBSItem);
                    OnPropertyChanged("Code");
                }
            }
        }

    }
}
