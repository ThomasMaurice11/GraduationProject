using GP.Services;
using Microsoft.AspNetCore.Mvc;

namespace GP.Controllers
{
    // DiseasePredictionController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class DiseasePredictionController : ControllerBase
    {
        private readonly DiseasePredictionService _predictionService;

        public DiseasePredictionController(DiseasePredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromBody] Dictionary<string, int> symptoms)
        {
            var predictions = await _predictionService.PredictDiseaseAsync(symptoms);
            return Ok(predictions);
        }
    }
}
