﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.States;

namespace Hangfire.JobKits.Worker
{
    internal sealed class RecurringJobDispatcher : IDashboardDispatcher
    {
        public StandbyMap Map { get; }

        private JobKitOptions Options { get; }

        public RecurringJobDispatcher(StandbyMap map, JobKitOptions options)
        {
            Map = map;
            Options = options;
        }

        public async Task Dispatch(DashboardContext context)
        {
            if (!"POST".Equals(context.Request.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = 405;
                return;
            }

            try
            {
                var key = context.Request.GetQuery(StandbyKey.IdField);

                var standbyJob = Map.JobCollection[key];

                var queuedState = new EnqueuedState();
                if (standbyJob.UseQueue)
                {
                    var queueString = (await context.Request.GetFormValuesAsync("equeued_state")).LastOrDefault();
                    queuedState.Queue = string.IsNullOrEmpty(queueString) ? queuedState.Queue : queueString;
                }

                var jobId = standbyJob.RecurringJobId;
                if (standbyJob.IsAppendMachineNameToJobId)
                {
                    jobId = $"{standbyJob.RecurringJobId}_{standbyJob.MachineName}";
                }

                var cron = (await context.Request.GetFormValuesAsync("recurring_cron")).LastOrDefault();
                var timeZone = Options.RecurringTimeZone ?? TimeZoneInfo.Local;

                var parameters = await StandbyHelper.CreateParameters(context, standbyJob.Method);
                var jobOptions = new RecurringJobOptions()
                {
                    TimeZone = timeZone,
                    QueueName = queuedState.Queue
                };
                
                context.GetRecurringJobManager().AddOrUpdate(jobId, new Job(standbyJob.Method, parameters), cron, jobOptions) ;
                context.Response.StatusCode = 200;

            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        }
    }
}
