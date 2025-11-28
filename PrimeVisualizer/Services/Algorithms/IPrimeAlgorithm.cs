using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeVisualizer.Services.Algorithms
{
    public interface IPrimeAlgorithm
    {
        string Name { get; }

        Task<List<int>> Calculate(int limit, IProgress<int> progress, CancellationToken token);
    }
}
