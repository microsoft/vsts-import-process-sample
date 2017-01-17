using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aardvark.ViewModels
{

    public class ImportResponseViewModel
    {
        public string id { get; set; }
        public string promoteJobId { get; set; }
        public string helpUrl { get; set; }
        public Validationresult[] validationResults { get; set; }
    }

    public class Validationresult
    {
        public string issueType { get; set; }
        public string error { get; set; }
        public string description { get; set; }
        public string file { get; set; }
        public string line { get; set; }
    }



}
