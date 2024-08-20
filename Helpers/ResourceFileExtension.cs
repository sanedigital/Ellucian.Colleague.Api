// Copyright 2023-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Logging;
using Ellucian.Web.Resource;
using System.Collections.Generic;
using System.Text;

namespace Ellucian.Colleague.Api.Helpers
{
    /// <summary>
    /// Extensions for <see cref="ResourceFile"/>.
    /// </summary>
    public static class ResourceFileExtension
    {
        /// <summary>
        /// Audit log configuration changes for a <see cref="ResourceFile"/>.
        /// </summary>
        /// <param name="resourceFile">Current <see cref="ResourceFile"/>.</param>
        /// <param name="oldResourceFile">Old <see cref="ResourceFile"/>.</param>
        /// <param name="props">Audit log information properties.</param>
        /// <param name="auditLog">The audit log instance.</param>
        public static void AuditLogConfigurationChanges(this ResourceFile resourceFile, ResourceFile oldResourceFile, AuditLogProperties props, AuditLoggingAdapter auditLog)
        {
            using (Serilog.Context.LogContext.Push(props.GetEnricherList().ToArray()))
            {
                if (ResourceFileEntriesHeadersHaveChanged(resourceFile.ResourceFileEntries, oldResourceFile.ResourceFileEntries, out string fileEntries, out string oldFileEntries))
                {
                    auditLog.Info($"Web API Admin API Resource Editor File, {resourceFile.ResourceFileName}: Property {nameof(resourceFile.ResourceFileEntries)} changed from [{oldFileEntries}] to [{fileEntries}].");
                }
            }
        }

        private static bool ResourceFileEntriesHeadersHaveChanged(IEnumerable<ResourceFileEntry> currentEntries, IEnumerable<ResourceFileEntry> oldEntries, out string currentFileEntryValues, out string oldFileEntryValues)
        {
            oldFileEntryValues = null;
            currentFileEntryValues = null;
            var oldEntryBuilder = new StringBuilder();
            var currentEntryBuilder = new StringBuilder();

            foreach (var entry in oldEntries)
            {
                var updatedEntry = currentEntries.FirstOrDefault(x => x.Key.Equals(entry.Key));

                if (!(updatedEntry is null) && !updatedEntry.Value.Equals(entry.Value))
                {
                    if (oldEntryBuilder.Length > 0)
                    {
                        oldEntryBuilder.Append(", ");
                        currentEntryBuilder.Append(", ");
                    }

                    oldEntryBuilder.Append($"{{{entry.Key}: '{entry.Value}'}}");
                    currentEntryBuilder.Append($"{{{updatedEntry.Key}: '{updatedEntry.Value}'}}");
                }
            }

            oldFileEntryValues = oldEntryBuilder.ToString();
            currentFileEntryValues = currentEntryBuilder.ToString();

            return !string.IsNullOrEmpty(oldFileEntryValues);
        }
    }
}
