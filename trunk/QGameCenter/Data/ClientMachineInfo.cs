using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace QGameCenter.Data
{
   public class ClientMachineInfo
    {
        public int ID { get; set; }

        public string Machine { get; set; }

        public string IP { get; set; }

        public string Info { get; set; }

        public string NetInfo { get; set; }

        public List<string> DiskInfo  = new List<string>();

    }
}
