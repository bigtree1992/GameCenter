using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGameCenter.Data
{
   public class ProcessInfo
    {

        public string ProcessID { get; set; }

        public string ProcessName { get; set; }
        public string ProcessCPU { get; set; }
        public string WorkingSet { get; set; }
        public string ProcessPath { get; set; }
        public string ProcessorTime { get; set; }

    }


    public class AppInfo
    {
        public string AppName { get; set; }
        public string AppState { get; set; }
    }
}
