using System.Collections.Generic;
using System.Linq;
using Hangfire.JobKits.Worker;

namespace Hangfire.JobKits.Dashboard
{
    internal partial class StandbyPage
    {
        /// <summary>
        /// Selected category.
        /// </summary>
        public string SelectedCategory { get; }
        /// <summary>
        /// Standby job map.
        /// </summary>
        public StandbyMap Map { get; }
        /// <summary>
        /// Standby options.
        /// </summary>
        public JobKitOptions Options { get; }
        /// <summary>
        /// Queues
        /// </summary>
        public List<string> Queues { get; }

        public StandbyPage(string selectedCategory, StandbyMap map, JobKitOptions options)
        {
            SelectedCategory = selectedCategory;
            Map = map;
            Options = options;
            Queues = GetQueues();
        }

        private List<string> GetQueues()
        {
            var servers = JobStorage.Current.GetMonitoringApi().Servers();
            var queues = new List<string>();
            foreach (var server in servers)
            {
                queues.AddRange(server.Queues);
            }

            return queues.Distinct().ToList();
        }
    }
}
