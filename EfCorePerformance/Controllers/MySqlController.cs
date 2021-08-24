using System;
using System.Threading.Tasks;
using EFTestApp.Data;
using EFTestApp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EFTestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MySqlController : ControllerBase
    {
        private readonly MySqlDbContext _dbContext;
        private readonly ILogger<MySqlController> _logger;
        
        public MySqlController(MySqlDbContext dbContext, ILogger<MySqlController> logger)
        {
	        _dbContext = dbContext;
            _logger = logger;
        }
        
        [HttpPost("AutoGenId")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateWithAutoGenId([FromBody] AutoGenId request)
		{
			try
			{
				_logger.LogInformation("Executing Create on AutoGenId");
				var  entry = await _dbContext.AddAsync(request);
				await _dbContext.SaveChangesAsync();
				
				return Created("Entry Created", entry.Entity);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERROR");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}
    }
}