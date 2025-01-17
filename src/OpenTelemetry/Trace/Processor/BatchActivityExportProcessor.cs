// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace OpenTelemetry;

/// <summary>
/// Implements processor that batches <see cref="Activity"/> objects before calling exporter.
/// </summary>
public class BatchActivityExportProcessor : BatchExportProcessor<Activity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchActivityExportProcessor"/> class.
    /// </summary>
    /// <param name="exporter"><inheritdoc cref="BatchExportProcessor{T}.BatchExportProcessor" path="/param[@name='exporter']"/></param>
    /// <param name="maxQueueSize"><inheritdoc cref="BatchExportProcessor{T}.BatchExportProcessor" path="/param[@name='maxQueueSize']"/></param>
    /// <param name="scheduledDelayMilliseconds"><inheritdoc cref="BatchExportProcessor{T}.BatchExportProcessor" path="/param[@name='scheduledDelayMilliseconds']"/></param>
    /// <param name="exporterTimeoutMilliseconds"><inheritdoc cref="BatchExportProcessor{T}.BatchExportProcessor" path="/param[@name='exporterTimeoutMilliseconds']"/></param>
    /// <param name="maxExportBatchSize"><inheritdoc cref="BatchExportProcessor{T}.BatchExportProcessor" path="/param[@name='maxExportBatchSize']"/></param>
    /// <param name="partialSpansEnabled"><inheritdoc cref="BatchExportProcessor{T}.BatchExportProcessor" path="/param[@name='partialSpansEnabled']"/></param>
    public BatchActivityExportProcessor(
        BaseExporter<Activity> exporter,
        int maxQueueSize = DefaultMaxQueueSize,
        int scheduledDelayMilliseconds = DefaultScheduledDelayMilliseconds,
        int exporterTimeoutMilliseconds = DefaultExporterTimeoutMilliseconds,
        int maxExportBatchSize = DefaultMaxExportBatchSize,
        bool partialSpansEnabled = DefaultPartialSpansEnabled)
        : base(
            exporter,
            maxQueueSize,
            scheduledDelayMilliseconds,
            exporterTimeoutMilliseconds,
            maxExportBatchSize,
            partialSpansEnabled)
    {
    }

    /// <inheritdoc />
    public override void OnStart(Activity data)
    {
        if (this.PartialSpansEnabled)
        {
            this.runningSpans.TryAdd(data.Context.SpanId.ToString(), data);
        }
    }

    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        if (!data.Recorded)
        {
            return;
        }

        if (this.PartialSpansEnabled)
        {
            Console.WriteLine($"End span: Current stored duration is {data.Duration.TotalMilliseconds} ms");
            this.runningSpans.TryRemove(data.Context.SpanId.ToString(), out Activity removedData);

            // There is already an EndTime due to the periodic export.
            // Set EndTime to the current time.
            var time = DateTimeOffset.UtcNow;
            data.SetEndTime(time.UtcDateTime);
        }

        this.OnExport(data);
    }
}
