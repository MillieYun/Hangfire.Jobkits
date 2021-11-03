﻿using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Hangfire.JobKits.Worker
{
    internal sealed class StandbyJob
    {
        public string Id { get; }
        public string CategoryName { get; }
        public string Name { get; }
        public string Description { get; }
        public bool UseQueue { get; }
        public string RecurringJobId { get; }
        public string RecurringJobCron { get; }
        public MethodInfo Method { get; }
        public string ActionName { get; }
        public bool IsAppendMachineNameToJobId { get; }
        public string MachineName { get => Environment.MachineName; }

        public StandbyJob(
            JobLauncherAttribute launcherAttribute, 
            JobMethodAttribute methodAttribute, 
            MethodInfo method)
        {
            Id = GenerateId(method);
            CategoryName = launcherAttribute.CategoryName ?? method.DeclaringType?.Name ?? "Default";

            Name = methodAttribute.Name;
            Description = methodAttribute.Description;
            UseQueue = methodAttribute.UseQueue;
            IsAppendMachineNameToJobId = methodAttribute.IsAppendMachineNameToJobId;

            RecurringJobId = methodAttribute.RecurringJobId;
            RecurringJobCron = methodAttribute.RecurringJobCron ?? "0/30 * * * *";
            
            Method = method;

            ActionName = $"{method.DeclaringType.Name}.{method.Name}";
        }

        private static string GenerateId(MethodInfo method)
        {
            var id = GenerateSignature(method);

            using (var crypt = new SHA1Managed())
            {
                var hashStringBuilder = new StringBuilder();
                var hash = crypt.ComputeHash(Encoding.ASCII.GetBytes(id));
                foreach (var @byte in hash)
                {
                    hashStringBuilder.Append(@byte.ToString("x2"));
                }
                return hashStringBuilder.ToString();
            }
        }

        private static string GenerateSignature(MethodInfo method)
        {
            var declaringType = method.DeclaringType?.FullName ?? "Unknown";
            var methodName = method.Name;
            var parameterList = string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType.FullName}"));

            return $"{declaringType}.{methodName}({parameterList})";
        }
    }
}
