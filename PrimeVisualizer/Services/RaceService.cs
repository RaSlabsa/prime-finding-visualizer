using Microsoft.AspNetCore.SignalR;
using PrimeVisualizer.Hubs;
using PrimeVisualizer.Services.Algorithms;

namespace PrimeVisualizer.Services
{
    public class RaceService
    {
        private readonly IEnumerable<IPrimeAlgorithm> _algorithms;
        private readonly IHubContext<RaceHub> _hubContext;

        public RaceService(IEnumerable<IPrimeAlgorithm> algorithms, IHubContext<RaceHub> hubContext)
        {
            _algorithms = algorithms;
            _hubContext = hubContext;
        }

        public async Task StartRace(int limit, List<string> selectedNames ,CancellationToken token)
        {
            var tasks = new List<Task>();

            var algorithmsToRun = _algorithms.Where(a => selectedNames.Contains(a.Name)).ToList();

            foreach (var algorithm in algorithmsToRun)
            {
                var progress = new Progress<int>(async percent =>
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveProgress",
                        new 
                        { 
                            algorithm = algorithm.Name, 
                            progress = percent 

                    });
                });

                var task = algorithm.Calculate(limit, progress, token).ContinueWith(async t =>
                {
                    if (t.IsCanceled || token.IsCancellationRequested)
                    {
                        return;
                    }

                    await _hubContext.Clients.All.SendAsync("AlgorithmFinished",
                        new
                        {
                            algorithm = algorithm.Name

                        });
                });

                tasks.Add(task);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Global error: {ex.Message}");
            }

            await Task.WhenAll(tasks);
        }


    }
}
