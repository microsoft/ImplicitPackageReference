using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Build.ImplicitPackageReference
{
    public class TestLogger : Logger
    {
        string ErrorMessage = "";
        string WarningMessage = "";
        string standardMessage = "";

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

        public string GetError()
        {
            return ErrorMessage;
        }
    }
}
