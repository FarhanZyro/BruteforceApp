using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BruteforceApp1.Core
{
    public class BruteForceResult
    {
        public string FoundPassword { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public long TotalAttempts { get; set; }
        public bool IsMultiThreaded { get; set; }
        public int ThreadsUsed { get; set; }
        public bool Success => FoundPassword != null;
    }

    public class BruteForceEngine
    {
        private readonly PasswordValidator _validator;
        private readonly CombinationGenerator _generator;
        private volatile string _foundPassword = null;
        private CancellationTokenSource _stopSignal;
        private long _totalAttempts = 0;
        private readonly long _totalCombinations;

        public event Action<long, long> OnProgressUpdate;
        public event Action<string, TimeSpan> OnPasswordFound;
        public event Action<TimeSpan> OnAttackFinished;

        public BruteForceEngine(PasswordValidator validator)
        {
            _validator = validator;
            _generator = new CombinationGenerator(PasswordGenerator.Charset);
            _totalCombinations = _generator.CountAllCombinations(PasswordGenerator.MaxLength);
        }

        public Task<BruteForceResult> StartMultiThreadedAsync()
        {
            return Task.Run(() => RunMultiThreaded());
        }

        private BruteForceResult RunMultiThreaded()
        {
            _foundPassword = null;
            Interlocked.Exchange(ref _totalAttempts, 0);
            _stopSignal = new CancellationTokenSource();

            int threadCount = Math.Max(1, Environment.ProcessorCount - 1);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int length = 1; length <= PasswordGenerator.MaxLength; length++)
            {
                if (_stopSignal.IsCancellationRequested || _foundPassword != null)
                    break;

                int charsPerThread = (int)Math.Ceiling(
                    (double)PasswordGenerator.Charset.Length / threadCount);

                // Build only the tasks that have valid work
                var taskList = new List<Task>();

                for (int t = 0; t < threadCount; t++)
                {
                    int startIndex = t * charsPerThread;
                    int endIndex = Math.Min(
                        startIndex + charsPerThread,
                        PasswordGenerator.Charset.Length);

                    if (startIndex >= PasswordGenerator.Charset.Length)
                        break; // no more work to assign

                    int capturedStart = startIndex;
                    int capturedEnd = endIndex;
                    int capturedLength = length;

                    var task = Task.Factory.StartNew(
                        () => SearchSlice(capturedLength, capturedStart, capturedEnd),
                        _stopSignal.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);

                    taskList.Add(task);
                }

                // Wait only on the tasks we actually created
                try { Task.WaitAll(taskList.ToArray(), _stopSignal.Token); }
                catch (OperationCanceledException) { break; }
                catch (AggregateException) { }

                if (_foundPassword != null) break;
            }

            stopwatch.Stop();

            if (_foundPassword != null)
                OnPasswordFound?.Invoke(_foundPassword, stopwatch.Elapsed);
            else
                OnAttackFinished?.Invoke(stopwatch.Elapsed);

            return new BruteForceResult
            {
                FoundPassword = _foundPassword,
                ElapsedTime = stopwatch.Elapsed,
                TotalAttempts = Interlocked.Read(ref _totalAttempts),
                IsMultiThreaded = true,
                ThreadsUsed = threadCount
            };
        }

        private void SearchSlice(int length, int startIndex, int endIndex)
        {
            CancellationToken token = _stopSignal.Token;

            foreach (string candidate in _generator.GenerateCombinations(
                length, startIndex, endIndex, token))
            {
                if (_foundPassword != null || token.IsCancellationRequested)
                    return;

                long attempts = Interlocked.Increment(ref _totalAttempts);

                if (_validator.IsMatch(candidate))
                {
                    _foundPassword = candidate;
                    _stopSignal.Cancel();
                    OnProgressUpdate?.Invoke(attempts, _totalCombinations);
                    return;
                }

                if (attempts % 50_000 == 0)
                    OnProgressUpdate?.Invoke(attempts, _totalCombinations);
            }
        }

        public Task<BruteForceResult> StartSingleThreadedAsync()
        {
            return Task.Run(() => RunSingleThreaded());
        }

        private BruteForceResult RunSingleThreaded()
        {
            _foundPassword = null;
            Interlocked.Exchange(ref _totalAttempts, 0);
            _stopSignal = new CancellationTokenSource();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            CancellationToken token = _stopSignal.Token;

            for (int length = 1; length <= PasswordGenerator.MaxLength; length++)
            {
                if (token.IsCancellationRequested) break;

                foreach (string candidate in _generator.GenerateCombinations(
                    length, 0, PasswordGenerator.Charset.Length, token))
                {
                    long attempts = Interlocked.Increment(ref _totalAttempts);

                    if (_validator.IsMatch(candidate))
                    {
                        _foundPassword = candidate;
                        stopwatch.Stop();
                        OnPasswordFound?.Invoke(_foundPassword, stopwatch.Elapsed);
                        return new BruteForceResult
                        {
                            FoundPassword = _foundPassword,
                            ElapsedTime = stopwatch.Elapsed,
                            TotalAttempts = attempts,
                            IsMultiThreaded = false,
                            ThreadsUsed = 1
                        };
                    }

                    if (attempts % 50_000 == 0)
                        OnProgressUpdate?.Invoke(attempts, _totalCombinations);

                    if (token.IsCancellationRequested) break;
                }
                if (_foundPassword != null) break;
            }

            stopwatch.Stop();
            OnAttackFinished?.Invoke(stopwatch.Elapsed);

            return new BruteForceResult
            {
                FoundPassword = _foundPassword,
                ElapsedTime = stopwatch.Elapsed,
                TotalAttempts = Interlocked.Read(ref _totalAttempts),
                IsMultiThreaded = false,
                ThreadsUsed = 1
            };
        }

        public void Stop() => _stopSignal?.Cancel();
        public long TotalCombinations => _totalCombinations;
    }
}