using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BruteforceApp1.Core;

namespace BruteforceApp1.Logging
{
    // Records timing results and compares single vs multi-thread speed
    public class PerformanceLogger
    {
        private readonly List<string> _entries = new List<string>();
        private readonly string _logFile;

        public PerformanceLogger(string logFile = "performance_log.txt")
        {
            _logFile = logFile;
        }

        public void LogRun(BruteForceResult result)
        {
            string mode = result.IsMultiThreaded
                ? $"MULTI-THREAD ({result.ThreadsUsed} threads)"
                : "SINGLE-THREAD (1 thread)";

            string line =
                $"[{DateTime.Now:HH:mm:ss}] {mode} | " +
                $"Time: {result.ElapsedTime.TotalSeconds:F3}s | " +
                $"Attempts: {result.TotalAttempts:N0} | " +
                $"Password: '{result.FoundPassword ?? "NOT FOUND"}'";

            _entries.Add(line);
            WriteToFile(line);
        }

        // Speedup = single time / multi time
        // e.g. single=15s, multi=5s → speedup=3.0x faster
        public string BuildComparisonReport(BruteForceResult single, BruteForceResult multi)
        {
            double speedup = single.ElapsedTime.TotalMilliseconds > 0
                ? single.ElapsedTime.TotalMilliseconds / multi.ElapsedTime.TotalMilliseconds
                : 1.0;

            var report = new StringBuilder();
            report.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            report.AppendLine("       PERFORMANCE COMPARISON REPORT      ");
            report.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            report.AppendLine($"  Single-thread : {single.ElapsedTime.TotalSeconds:F3} seconds");
            report.AppendLine($"  Multi-thread  : {multi.ElapsedTime.TotalSeconds:F3} seconds");
            report.AppendLine($"  Threads used  : {multi.ThreadsUsed}");
            report.AppendLine($"  Speedup       : {speedup:F2}x faster");
            report.AppendLine($"  Password      : '{multi.FoundPassword ?? single.FoundPassword ?? "NOT FOUND"}'");
            report.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            string text = report.ToString();
            _entries.Add(text);
            WriteToFile(text);
            return text;
        }

        private void WriteToFile(string text)
        {
            try { File.AppendAllText(_logFile, text + Environment.NewLine); }
            catch { }
        }
    }
}