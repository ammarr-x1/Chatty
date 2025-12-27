using System;
using System.Collections.Generic;
//using System.Linq;
using System.Threading.Tasks;

namespace Chatty.Models
{
    public class UserMessage
    {
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool CurrentUser { get; set; }
        public DateTime DateSent { get; set; }
    }
}
