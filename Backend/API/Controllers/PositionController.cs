using Application.DTOs.Organization;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("{id}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<PositionDto>> GetById(int id)
        {
            try
            {
                var position = await _positionService.GetByIdAsync(id, GetUserId());
                return Ok(position);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var positions = await _positionService.GetAllAsync(GetUserId(), pageNumber, pageSize);
            return Ok(positions);
        }

        [HttpGet("active")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetActive()
        {
            var positions = await _positionService.GetActivePositionsAsync(GetUserId());
            return Ok(positions);
        }

        [HttpGet("management")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetManagementPositions()
        {
            var positions = await _positionService.GetManagementPositionsAsync(GetUserId());
            return Ok(positions);
        }

        [HttpGet("search")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> Search([FromQuery] string searchTerm)
        {
            var positions = await _positionService.SearchAsync(searchTerm, GetUserId());
            return Ok(positions);
        }

        [HttpGet("department/{departmentId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetByDepartment(int departmentId)
        {
            var positions = await _positionService.GetByDepartmentAsync(departmentId, GetUserId());
            return Ok(positions);
        }

        [HttpGet("level/{level}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetByLevel(int level)
        {
            var positions = await _positionService.GetByLevelAsync(level, GetUserId());
            return Ok(positions);
        }

        [HttpGet("employment-type/{type}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetByEmploymentType(string type)
        {
            var positions = await _positionService.GetByEmploymentTypeAsync(type, GetUserId());
            return Ok(positions);
        }

        [HttpGet("salary-range")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetBySalaryRange([FromQuery] decimal minSalary, [FromQuery] decimal maxSalary)
        {
            var positions = await _positionService.GetBySalaryRangeAsync(minSalary, maxSalary, GetUserId());
            return Ok(positions);
        }

        [HttpPost]
        [Permission(SystemPermission.OrganizationCreate)]
        public async Task<ActionResult<PositionDto>> Create([FromBody] CreatePositionDto dto)
        {
            var position = await _positionService.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = position.Id }, position);
        }

        [HttpPut("{id}")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<ActionResult<PositionDto>> Update(int id, [FromBody] UpdatePositionDto dto)
        {
            try
            {
                var position = await _positionService.UpdateAsync(id, dto, GetUserId());
                return Ok(position);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Permission(SystemPermission.OrganizationDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _positionService.DeleteAsync(id, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Hierarchy Operations
        [HttpGet("{id}/reports-to")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<PositionDto>> GetReportsTo(int id)
        {
            var reportsTo = await _positionService.GetReportsToPositionAsync(id, GetUserId());
            if (reportsTo == null) return NotFound("Reports-to position not found");
            return Ok(reportsTo);
        }

        [HttpGet("{id}/subordinates")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<PositionDto>>> GetSubordinates(int id)
        {
            var subordinates = await _positionService.GetSubordinatePositionsAsync(id, GetUserId());
            return Ok(subordinates);
        }

        // Statistics
        [HttpGet("statistics")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<PositionStatisticsDto>> GetStatistics()
        {
            var stats = await _positionService.GetStatisticsAsync(GetUserId());
            return Ok(stats);
        }
    }
}
