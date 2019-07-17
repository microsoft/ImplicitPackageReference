// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Build.ImplicitPackageReference
{
    public class TestLogger : ILogger
    {
        public string ErrorMessage = "";
        public string WarningMessage = "";
        public string standardMessage = "";

        public void LogError(string message)
        {
            ErrorMessage = message;
        }

        public void LogMessage(string message)
        {
            standardMessage = message;
        }

        public void LogWarning(string message)
        {
            WarningMessage = message;
        }
    }
}
