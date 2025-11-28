using Microsoft.OpenApi.MicrosoftExtensions;
using System.Diagnostics;

namespace PrimeVisualizer.Services.Algorithms
{
    public class TrialDivisionOptimized : IPrimeAlgorithm
    {
        public string Name => "Trial Division (optimized)";

        public async Task<List<int>> Calculate(int limit, IProgress<int> progress, CancellationToken token)
        {
            var primes = new List<int>();

            await Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                long lastReportTime = 0;
                int lastReportedPercent = 0;

                for (int i = 2; i < limit; i++)
                {
                    token.ThrowIfCancellationRequested();

                    bool isPrime = true;
                    int k = (int)Math.Ceiling(Math.Sqrt(i));

                    for (int j = 2; j <= k; j++)
                    {
                        if (i % j == 0) 
                        {
                            isPrime = false;
                            break;
                        }
                    }

                    if (isPrime) primes.Add(i);

                    int currentPercent = (int)((double)i / limit * 100);

                    if (currentPercent > lastReportedPercent &&
                        stopwatch.ElapsedMilliseconds - lastReportTime > 50)
                    {
                        progress?.Report(currentPercent);
                        lastReportedPercent = currentPercent;
                        lastReportTime = stopwatch.ElapsedMilliseconds;
                    }
                }

                progress?.Report(100);

            }, token);

            return primes;
        }
    }
}
