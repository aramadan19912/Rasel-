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
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // CRUD Operations
        [HttpGet("{id}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeDetailDto>> GetById(int id)
        {
            try
            {
                var employee = await _employeeService.GetByIdAsync(id, GetUserId());
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("employee-number/{employeeNumber}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeDetailDto>> GetByEmployeeNumber(string employeeNumber)
        {
            try
            {
                var employee = await _employeeService.GetByEmployeeNumberAsync(employeeNumber, GetUserId());
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeDetailDto>> GetByUserId(string userId)
        {
            try
            {
                var employee = await _employeeService.GetByUserIdAsync(userId, GetUserId());
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var employees = await _employeeService.GetAllAsync(GetUserId(), pageNumber, pageSize);
            return Ok(employees);
        }

        [HttpGet("search")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> Search([FromQuery] string searchTerm)
        {
            var employees = await _employeeService.SearchAsync(searchTerm, GetUserId());
            return Ok(employees);
        }

        [HttpPost]
        [Permission(SystemPermission.OrganizationCreate)]
        public async Task<ActionResult<EmployeeDetailDto>> Create([FromBody] CreateEmployeeDto dto)
        {
            var employee = await _employeeService.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<ActionResult<EmployeeDetailDto>> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.UpdateAsync(id, dto, GetUserId());
                return Ok(employee);
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
                var result = await _employeeService.DeleteAsync(id, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Hierarchy Operations
        [HttpGet("{id}/manager")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeDto>> GetManager(int id)
        {
            var manager = await _employeeService.GetManagerAsync(id, GetUserId());
            if (manager == null) return NotFound("Manager not found");
            return Ok(manager);
        }

        [HttpGet("{id}/direct-reports")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetDirectReports(int id)
        {
            var reports = await _employeeService.GetDirectReportsAsync(id, GetUserId());
            return Ok(reports);
        }

        [HttpGet("{id}/all-reports")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetAllReports(int id)
        {
            var reports = await _employeeService.GetAllReportsAsync(id, GetUserId());
            return Ok(reports);
        }

        [HttpGet("{id}/manager-chain")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetManagerChain(int id)
        {
            var chain = await _employeeService.GetManagerChainAsync(id, GetUserId());
            return Ok(chain);
        }

        [HttpPut("{id}/manager")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<IActionResult> UpdateManager(int id, [FromBody] UpdateManagerRequest request)
        {
            var result = await _employeeService.UpdateManagerAsync(id, request.NewManagerId, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        // Department & Position Operations
        [HttpGet("department/{departmentId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetByDepartment(int departmentId)
        {
            var employees = await _employeeService.GetByDepartmentAsync(departmentId, GetUserId());
            return Ok(employees);
        }

        [HttpGet("position/{positionId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetByPosition(int positionId)
        {
            var employees = await _employeeService.GetByPositionAsync(positionId, GetUserId());
            return Ok(employees);
        }

        [HttpPut("{id}/transfer")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<IActionResult> TransferDepartment(int id, [FromBody] TransferRequest request)
        {
            var result = await _employeeService.TransferDepartmentAsync(id, request.NewDepartmentId, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/promote")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<IActionResult> Promote(int id, [FromBody] PromoteRequest request)
        {
            var result = await _employeeService.PromoteAsync(id, request.NewPositionId, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        // Search and Filter
        [HttpGet("status/{status}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetByStatus(string status)
        {
            var employees = await _employeeService.GetByEmploymentStatusAsync(status, GetUserId());
            return Ok(employees);
        }

        [HttpGet("type/{type}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetByType(string type)
        {
            var employees = await _employeeService.GetByEmploymentTypeAsync(type, GetUserId());
            return Ok(employees);
        }

        [HttpGet("location/{location}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetByLocation(string location)
        {
            var employees = await _employeeService.GetByLocationAsync(location, GetUserId());
            return Ok(employees);
        }

        [HttpGet("communication-level/{level}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetByCommunicationLevel(int level)
        {
            var employees = await _employeeService.GetByCommunicationLevelAsync(level, GetUserId());
            return Ok(employees);
        }

        [HttpGet("remote")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetRemoteEmployees()
        {
            var employees = await _employeeService.GetRemoteEmployeesAsync(GetUserId());
            return Ok(employees);
        }

        // Communication Permissions
        [HttpGet("{id}/can-communicate/{targetId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<bool>> CanCommunicateWith(int id, int targetId)
        {
            var canCommunicate = await _employeeService.CanCommunicateWithAsync(id, targetId, GetUserId());
            return Ok(canCommunicate);
        }

        [HttpGet("{id}/communication-peers")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetCommunicationPeers(int id)
        {
            var peers = await _employeeService.GetCommunicationPeersAsync(id, GetUserId());
            return Ok(peers);
        }

        [HttpGet("{id}/requires-approval")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<bool>> RequiresManagerApproval(int id)
        {
            var requires = await _employeeService.RequiresManagerApprovalAsync(id, GetUserId());
            return Ok(requires);
        }

        // Onboarding & Offboarding
        [HttpPost("{id}/onboard")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<ActionResult<EmployeeDetailDto>> Onboard(int id)
        {
            try
            {
                var employee = await _employeeService.OnboardEmployeeAsync(id, GetUserId());
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{id}/terminate")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<IActionResult> Terminate(int id, [FromBody] TerminateRequest request)
        {
            var result = await _employeeService.TerminateEmployeeAsync(id, request.TerminationDate, request.Reason, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        // Statistics
        [HttpGet("statistics")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeStatisticsDto>> GetStatistics()
        {
            var stats = await _employeeService.GetStatisticsAsync(GetUserId());
            return Ok(stats);
        }

        [HttpGet("statistics/department/{departmentId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeStatisticsDto>> GetDepartmentStatistics(int departmentId)
        {
            var stats = await _employeeService.GetDepartmentStatisticsAsync(departmentId, GetUserId());
            return Ok(stats);
        }

        // Request DTOs
        public record UpdateManagerRequest(string NewManagerId);
        public record TransferRequest(int NewDepartmentId);
        public record PromoteRequest(int NewPositionId);
        public record TerminateRequest(DateTime TerminationDate, string Reason);
    }
}
