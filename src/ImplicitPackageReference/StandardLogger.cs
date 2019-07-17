// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Build.ImplicitPackageReference
{
    using Microsoft.Build.Utilities;

    public class StandardLogger : ILogger
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
