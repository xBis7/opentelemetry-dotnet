// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using OpenTelemetry.Logs;

namespace OpenTelemetry;

/// <summary>
/// Implements processor that batches <see cref="Activity"/> objects before calling exporter.
/// </summary>
public class PartialActivityExportProcessor : PartialExportProcessor<Activity>
{

    BaseExportProcessor<LogRecord> logProcessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialActivityExportProcessor"/> class.
    /// </summary>
    /// <param name="exporter"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='exporter']"/></param>
    /// <param name="logProcessor"><inheritdoc cref="SimpleLogRecordExportProcessor" path="/param[@name='exporter']"/></param>
    /// <param name="maxQueueSize"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='maxQueueSize']"/></param>
    /// <param name="scheduledDelayMilliseconds"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='scheduledDelayMilliseconds']"/></param>
    /// <param name="exporterTimeoutMilliseconds"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='exporterTimeoutMilliseconds']"/></param>
    /// <param name="maxExportBatchSize"><inheritdoc cref="BatchExportProcessor{T}" path="/param[@name='maxExportBatchSize']"/></param>
    public PartialActivityExportProcessor(
        BaseExporter<Activity> exporter,
        BaseExportProcessor<LogRecord> logProcessor,
        int maxQueueSize = DefaultMaxQueueSize,
        int scheduledDelayMilliseconds = DefaultScheduledDelayMilliseconds,
        int exporterTimeoutMilliseconds = DefaultExporterTimeoutMilliseconds,
        int maxExportBatchSize = DefaultMaxExportBatchSize)
        : base(
            exporter,
            logProcessor,
            maxQueueSize,
            scheduledDelayMilliseconds,
            exporterTimeoutMilliseconds,
            maxExportBatchSize)
    {
        this.logProcessor = logProcessor;
    }

    /// <inheritdoc />
    public override void OnStart(Activity data)
    {

        var logRecord = new LogRecord
        {
            Timestamp = data.StartTimeUtc,
            TraceId = data.TraceId,
            SpanId = data.SpanId,
            TraceFlags = data.ActivityTraceFlags,
            Severity = LogRecordSeverity.Info,
            // Body = data.,
            Attributes = new List<KeyValuePair<string, object?>>
            {
                new KeyValuePair<string, object?>("partial.event", "heartbeat"),
                new KeyValuePair<string, object?>("partial.frequency", DefaultScheduledDelayMilliseconds + "ms"),
                new KeyValuePair<string, object?>("telemetry.logs.cluster", "partial"),
                new KeyValuePair<string, object?>("telemetry.logs.project", "span"),
            },
        };
        this.logProcessor.Exporter.Export(new Batch<LogRecord>(logRecord));
    }

    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        if (!data.Recorded)
        {
            return;
        }

        this.OnExport(data);
    }
}
