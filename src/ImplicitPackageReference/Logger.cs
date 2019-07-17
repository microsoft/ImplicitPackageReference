// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Build.ImplicitPackageReference
{
    public interface ILogger
    {
        void LogError(string message);
        void LogMessage(string message);
        void LogWarning(string message);
    }
}
