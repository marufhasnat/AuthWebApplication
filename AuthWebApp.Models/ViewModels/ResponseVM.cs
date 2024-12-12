using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWebApp.Models.ViewModels
{
    public class ResponseVM
    {
        public string Message { get; set; } = default!;
        public string StatusCode { get; set; } = default!;
    }
}
