using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AspireAPI.AdminWeb
{
    /// <summary>
    /// In-memory ring buffer of recent tool invocations. Operator-facing only —
    /// useful for catching auth failures and runaway 5xx loops without trawling
    /// stderr. Capped at <see cref="Capacity"/> entries to bound memory.
    /// </summary>
    public sealed class CallTailBuffer
    {
        public const int Capacity = 200;

        private readonly ConcurrentQueue<CallEntry> _entries = new();

        public void Record(CallEntry entry)
        {
            _entries.Enqueue(entry);
            while (_entries.Count > Capacity && _entries.TryDequeue(out _)) { /* trim */ }
        }

        public IReadOnlyList<CallEntry> Recent(int max = Capacity)
        {
            var arr = _entries.ToArray();
            // Newest first.
            Array.Reverse(arr);
            return arr.Length <= max ? arr : arr.AsSpan(0, max).ToArray();
        }

        public void Clear()
        {
            while (_entries.TryDequeue(out _)) { }
        }
    }

    public sealed record CallEntry(
        DateTime AtUtc,
        string ToolName,
        bool Ok,
        long DurationMs,
        int? StatusCode,
        bool DryRun,
        string? Error,
        string Source);
}
