using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PrimeVisualizer.Services.Algorithms
{
    public class TrialDivisionWithPrecomputedPrimes : IPrimeAlgorithm
    {
        public string Name => "Trial Division (With Precomputed Primes)";

        public async Task<List<int>> Calculate(int limit, IProgress<int> progress, CancellationToken token)
        {
            var result = new List<int>();
            int sqrtLimit = (int)Math.Ceiling(Math.Sqrt(limit));
            await Task.Run(async () =>
            {
                var basePrimes = GetSmallPrimes(sqrtLimit);
                result.AddRange(basePrimes);

                int start = sqrtLimit++;

                if (start % 2 == 0) start++;

                var stopwatch = Stopwatch.StartNew();
                long lastReportTime = 0;
                int lastReportedPercent = 0;

                for (var current = start; current <= limit; current+=2)
                {
                    token.ThrowIfCancellationRequested();

                    var isPrime = true;

                    var currentLimit = (int)Math.Sqrt(current);

                    for (int j = 0; j < basePrimes.Count; j++)
                    {
                        int p = basePrimes[j];

                        if (p > currentLimit) break;

                        if (current % p == 0)
                        {
                            isPrime = false;
                            break;
                        }
                    }

                    if (isPrime)
                    {
                        result.Add(current);
                    }

                    int currentPercent = (int)((double)current / limit * 100);

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

            return result;
        }

        public List<int> GetSmallPrimes(int limit)
        {
            var primes = new List<int>();
            bool[] isPrime = new bool[limit + 1];

            isPrime[0] = isPrime[1] = false;

            for (int p = 2; p * p <= limit; p++)
            {
                if (isPrime[p])
                {
                    for (int i = p * p; i <= limit; i += p)
                    {
                        isPrime[i] = false;
                    }
                }
            }

            for (int i = 2; i <= limit; i++)
            {

                if (isPrime[i])
                {
                    primes.Add(i);

                }
            }

            return primes;
        }
    }
}
