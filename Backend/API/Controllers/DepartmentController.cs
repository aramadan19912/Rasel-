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
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("{id}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<DepartmentDto>> GetById(int id)
        {
            try
            {
                var department = await _departmentService.GetByIdAsync(id, GetUserId());
                return Ok(department);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<DepartmentDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var departments = await _departmentService.GetAllAsync(GetUserId(), pageNumber, pageSize);
            return Ok(departments);
        }

        [HttpGet("active")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<DepartmentDto>>> GetActive()
        {
            var departments = await _departmentService.GetActiveDepartmentsAsync(GetUserId());
            return Ok(departments);
        }

        [HttpGet("search")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<DepartmentDto>>> Search([FromQuery] string searchTerm)
        {
            var departments = await _departmentService.SearchAsync(searchTerm, GetUserId());
            return Ok(departments);
        }

        [HttpGet("location/{location}")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<DepartmentDto>>> GetByLocation(string location)
        {
            var departments = await _departmentService.GetByLocationAsync(location, GetUserId());
            return Ok(departments);
        }

        [HttpPost]
        [Permission(SystemPermission.OrganizationCreate)]
        public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto dto)
        {
            var department = await _departmentService.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }

        [HttpPut("{id}")]
        [Permission(SystemPermission.OrganizationUpdate)]
        public async Task<ActionResult<DepartmentDto>> Update(int id, [FromBody] UpdateDepartmentDto dto)
        {
            try
            {
                var department = await _departmentService.UpdateAsync(id, dto, GetUserId());
                return Ok(department);
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
                var result = await _departmentService.DeleteAsync(id, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Hierarchy Operations
        [HttpGet("{id}/hierarchy")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<DepartmentHierarchyDto>> GetHierarchy(int id)
        {
            try
            {
                var hierarchy = await _departmentService.GetDepartmentHierarchyAsync(id, GetUserId());
                return Ok(hierarchy);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("tree")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<DepartmentHierarchyDto>> GetFullTree()
        {
            try
            {
                var tree = await _departmentService.GetFullDepartmentTreeAsync(GetUserId());
                return Ok(tree);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/subdepartments")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<DepartmentDto>>> GetSubDepartments(int id)
        {
            var subDepartments = await _departmentService.GetSubDepartmentsAsync(id, GetUserId());
            return Ok(subDepartments);
        }

        [HttpGet("{id}/parent")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<DepartmentDto>> GetParent(int id)
        {
            var parent = await _departmentService.GetParentDepartmentAsync(id, GetUserId());
            if (parent == null) return NotFound("Parent department not found");
            return Ok(parent);
        }

        // Employee Operations
        [HttpGet("{id}/employees")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<List<EmployeeDto>>> GetEmployees(int id)
        {
            var employees = await _departmentService.GetDepartmentEmployeesAsync(id, GetUserId());
            return Ok(employees);
        }

        [HttpGet("{id}/employee-count")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<int>> GetEmployeeCount(int id)
        {
            var count = await _departmentService.GetEmployeeCountAsync(id, GetUserId());
            return Ok(count);
        }

        // Statistics
        [HttpGet("statistics")]
        [Permission(SystemPermission.OrganizationRead)]
        public async Task<ActionResult<DepartmentStatisticsDto>> GetStatistics()
        {
            var stats = await _departmentService.GetStatisticsAsync(GetUserId());
            return Ok(stats);
        }
    }
}
