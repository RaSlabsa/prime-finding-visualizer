using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace PrimeVisualizer.Services.Algorithms
{
    public class EratosthenesSieve : IPrimeAlgorithm
    {
        public string Name => "Sieve of Eratosthenes";

        public async Task<List<int>> Calculate(int limit, IProgress<int> progress, CancellationToken token)
        {
            var result = new List<int>();

            await Task.Run(() =>
            {
                bool[] prime = new bool[limit + 1];
                Array.Fill(prime, true);

                prime[0] = prime[1] = false;

                var stopwatch = Stopwatch.StartNew();
                long lastReportTime = 0;
                int lastReportedPercent = 0;

                for (int p = 2; p * p <= limit; p++)
                {
                    if (prime[p])
                    {
                        for (int i = p * p; i <= limit; i+=p)
                        {
                            prime[i] = false;
                        }
                    }
                }

                for (int i = 2; i <= limit; i++)
                {
                    token.ThrowIfCancellationRequested();

                    if (prime[i])
                    {
                        result.Add(i);

                        int currentPercent = (int)((double)i / limit * 100);

                        if (currentPercent > lastReportedPercent &&
                        stopwatch.ElapsedMilliseconds - lastReportTime > 50)
                        {
                            progress?.Report(currentPercent);
                            lastReportedPercent = currentPercent;
                            lastReportTime = stopwatch.ElapsedMilliseconds;
                        }
                    }
                }

                progress?.Report(100);

            }, token);

            return result;
        }
    }
}
