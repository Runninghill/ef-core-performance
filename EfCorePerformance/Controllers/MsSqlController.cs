using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFTestApp.Data;
using EFTestApp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Guid = EFTestApp.Model.Guid;

namespace EFTestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MsSqlController : ControllerBase
    {
        private readonly MsSqlDbContext _dbContext;
        private readonly ILogger<MsSqlController> _logger;
        
        public MsSqlController(MsSqlDbContext dbContext, ILogger<MsSqlController> logger)
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
				_logger.LogInformation("Executing Create with AutoGenId");
				var entry = await _dbContext.AddAsync(request);
				await _dbContext.SaveChangesAsync();
				
				return Created("Entry Created", entry.Entity);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,"ERROR");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}
		
		[HttpPost("Guid")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateWithGuid([FromBody] Guid request)
		{
			try
			{
				_logger.LogInformation("Executing Create with Guid");
				var entry = await _dbContext.AddAsync(request);
				await _dbContext.SaveChangesAsync();
				
				return Created("Entry Created", entry.Entity);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,"ERROR");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}
		
		[HttpPost("SP")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateWithSp([FromBody] AutoGenId request)
		{
			try
			{
				_logger.LogInformation("Executing Create with Stored Procedure");
				var sql = "EXECUTE dbo.[sp_InsertTestModel] @FirstName, @LastName, @IdNumber";
				var parameters = new List<SqlParameter> 
				{
					new() { ParameterName = "@FirstName", Value = request.FirstName },
					new() { ParameterName = "@LastName", Value = request.Surname },
					new() { ParameterName = "@IdNumber", Value = request.IdNumber }
				};

				await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
				return Created("Entry Created", request);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,"ERROR");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}
    }
}