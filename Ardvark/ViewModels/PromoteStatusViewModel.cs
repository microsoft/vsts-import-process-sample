using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aardvark.ViewModels
{
    public class PromoteStatusViewModel
    {
        public string id { get; set; }
        public int complete { get; set; }
        public int pending { get; set; }
        public int remainingRetries { get; set; }
        public bool successful { get; set; }
        public string message { get; set; }
    }
}
