using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aardvark.ViewModels
{
    public class ImportViewModel
    {
        public ImportResponseViewModel ImportResponseViewModel { get; set; } = null;
        public string Message { get; set; }
        public bool Success { get; set; } = false;
        public string PromoteJobId { get; set; } = "0";
        public Validationresult[] validationResults { get; set; }
    }
}
