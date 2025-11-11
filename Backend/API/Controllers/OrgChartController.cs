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
    public class OrgChartController : ControllerBase
    {
        private readonly IOrgChartService _orgChartService;

        public OrgChartController(IOrgChartService orgChartService)
        {
            _orgChartService = orgChartService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Org Chart Visualization
        [HttpGet("company")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<OrgChartDto>> GetCompanyOrgChart()
        {
            try
            {
                var orgChart = await _orgChartService.GetCompanyOrgChartAsync(GetUserId());
                return Ok(orgChart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("department/{departmentId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<OrgChartDto>> GetDepartmentOrgChart(int departmentId)
        {
            try
            {
                var orgChart = await _orgChartService.GetDepartmentOrgChartAsync(departmentId, GetUserId());
                return Ok(orgChart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeHierarchyDto>> GetEmployeeOrgChart(int employeeId, [FromQuery] int depth = 10)
        {
            try
            {
                var orgChart = await _orgChartService.GetEmployeeOrgChartAsync(employeeId, depth, GetUserId());
                return Ok(orgChart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Communication Level Operations
        [HttpGet("employee/{employeeId}/communication-level")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<int>> GetEmployeeCommunicationLevel(int employeeId)
        {
            try
            {
                var level = await _orgChartService.GetEmployeeCommunicationLevelAsync(employeeId, GetUserId());
                return Ok(level);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("level/{level}/employees")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetEmployeesAtLevel(int level)
        {
            var employees = await _orgChartService.GetEmployeesAtLevelAsync(level, GetUserId());
            return Ok(employees);
        }

        [HttpGet("level/{level}/employees/above")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetEmployeesAboveLevel(int level)
        {
            var employees = await _orgChartService.GetEmployeesAboveLevelAsync(level, GetUserId());
            return Ok(employees);
        }

        [HttpGet("level/{level}/employees/below")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetEmployeesBelowLevel(int level)
        {
            var employees = await _orgChartService.GetEmployeesBelowLevelAsync(level, GetUserId());
            return Ok(employees);
        }

        // Communication Routing
        [HttpGet("can-communicate")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<bool>> CanEmployeeCommunicate([FromQuery] int fromEmployeeId, [FromQuery] int toEmployeeId)
        {
            var canCommunicate = await _orgChartService.CanEmployeeCommunicateAsync(fromEmployeeId, toEmployeeId, GetUserId());
            return Ok(canCommunicate);
        }

        [HttpGet("employee/{employeeId}/authorized-partners")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetAuthorizedCommunicationPartners(int employeeId)
        {
            var partners = await _orgChartService.GetAuthorizedCommunicationPartnersAsync(employeeId, GetUserId());
            return Ok(partners);
        }

        [HttpGet("communication-path")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<CommunicationPathDto>> GetCommunicationPath([FromQuery] int fromEmployeeId, [FromQuery] int toEmployeeId)
        {
            try
            {
                var path = await _orgChartService.GetCommunicationPathAsync(fromEmployeeId, toEmployeeId, GetUserId());
                return Ok(path);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Manager Approval Workflow
        [HttpGet("employee/{employeeId}/requires-approval")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<bool>> RequiresApproval(int employeeId, [FromQuery] string action)
        {
            var requires = await _orgChartService.RequiresApprovalAsync(employeeId, action, GetUserId());
            return Ok(requires);
        }

        [HttpGet("employee/{employeeId}/approver")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<EmployeeDto>> GetApprover(int employeeId, [FromQuery] string action)
        {
            var approver = await _orgChartService.GetApproverAsync(employeeId, action, GetUserId());
            if (approver == null) return NotFound("Approver not found");
            return Ok(approver);
        }

        [HttpGet("employee/{employeeId}/approval-chain")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetApprovalChain(int employeeId)
        {
            var chain = await _orgChartService.GetApprovalChainAsync(employeeId, GetUserId());
            return Ok(chain);
        }

        // Hierarchy Analysis
        [HttpGet("employee/{employeeId}/hierarchy-depth")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<int>> GetHierarchyDepth(int employeeId)
        {
            var depth = await _orgChartService.GetHierarchyDepthAsync(employeeId, GetUserId());
            return Ok(depth);
        }

        [HttpGet("employee/{managerId}/span-of-control")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<int>> GetSpanOfControl(int managerId)
        {
            var span = await _orgChartService.GetSpanOfControlAsync(managerId, GetUserId());
            return Ok(span);
        }

        [HttpGet("employee/{managerId}/total-subordinates")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<int>> GetTotalSubordinates(int managerId)
        {
            var total = await _orgChartService.GetTotalSubordinatesAsync(managerId, GetUserId());
            return Ok(total);
        }

        [HttpGet("employee/{employeeId}/peers")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetPeers(int employeeId)
        {
            var peers = await _orgChartService.GetPeersAsync(employeeId, GetUserId());
            return Ok(peers);
        }

        // Department Communication
        [HttpGet("department/{departmentId}/communication-list")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetDepartmentCommunicationList(int departmentId)
        {
            var employees = await _orgChartService.GetDepartmentCommunicationListAsync(departmentId, GetUserId());
            return Ok(employees);
        }

        [HttpGet("employee/{employeeId}/can-communicate-with-department/{departmentId}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<bool>> CanCommunicateWithDepartment(int employeeId, int departmentId)
        {
            var canCommunicate = await _orgChartService.CanCommunicateWithDepartmentAsync(employeeId, departmentId, GetUserId());
            return Ok(canCommunicate);
        }

        // Statistics
        [HttpGet("statistics")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<OrgChartStatisticsDto>> GetStatistics()
        {
            var stats = await _orgChartService.GetOrgChartStatisticsAsync(GetUserId());
            return Ok(stats);
        }
    }
}
