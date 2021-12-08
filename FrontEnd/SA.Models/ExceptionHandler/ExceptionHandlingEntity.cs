using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SA.Models.ExceptionHandler
{
    public class ExceptionHandlingEntity
    {
        public string Message;
        public Exception Exception;
        public string Title;
        public string Source;
        public TraceEventType eventType;

    }
}
