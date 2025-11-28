using Microsoft.AspNetCore.Mvc;
using PrimeVisualizer.Models;
using PrimeVisualizer.Services;
using PrimeVisualizer.Services.Algorithms;

namespace PrimeVisualizer.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class RaceController : ControllerBase
    {
        private readonly RaceService _raceService;
        private readonly IEnumerable<IPrimeAlgorithm> _algorithms;

        public RaceController(RaceService raceService, IEnumerable<IPrimeAlgorithm> algorithms)
        {
            _raceService = raceService;
            _algorithms = algorithms;
        }

        [HttpGet("algorithms")]
        public IActionResult GetAlgorithms()
        {
            var list = _algorithms.Select(algo => new
            {
                key = algo.Name,
                label = algo.Name

            }).ToList();

            return Ok(list);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartRace([FromBody] StartRaceRequest request, CancellationToken token)
        {
            try
            {
                await _raceService.StartRace(request.Limit, request.selectedAlgorithms, token);
            }
            catch (OperationCanceledException)
            {
                return Ok(new { message = "Race canceled" });
            }

            return Ok(new { message = "RaceFinished" });
        }
    }
}
