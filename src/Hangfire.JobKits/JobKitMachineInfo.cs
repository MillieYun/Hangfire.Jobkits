using System;
using Hangfire.Annotations;

namespace Hangfire.JobKits
{
    [PublicAPI]
    public static class JobKitMachineInfo
    {
        /// <summary>
        /// Machine Name
        /// </summary>
        public static string Name { get => Environment.MachineName.Replace("-", "_").Replace(".", "_").ToLower(); }

    }
}
