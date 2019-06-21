using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Build.ImplicitPackageReference
{
    public class StandardLogger : Logger
    {
        private TaskLoggingHelper myLogger;

        public StandardLogger(TaskLoggingHelper buildEngine)
        {
            myLogger = buildEngine;
        }

        public void LogError(string message)
        {
            myLogger.LogError(message, null);
        }

        public void LogMessage(string message)
        {
            myLogger.LogMessage(message, null);
        }

        public void LogWarning(string message)
        {
            myLogger.LogWarning(message, null);
        }
    }
}
