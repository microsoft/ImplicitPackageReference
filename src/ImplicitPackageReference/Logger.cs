using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Build.ImplicitPackageReference
{
    public interface Logger
    {
        void LogError(string message);
        void LogMessage(string message);
        void LogWarning(string message);
    }
}
