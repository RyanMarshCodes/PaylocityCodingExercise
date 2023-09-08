using System.Collections.Immutable;
using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EmployeesController : ControllerBase
{
    private ISQLiteRepository _repository;
    private IPayService _payService;

    public EmployeesController(ISQLiteRepository repository, IPayService payService)
    {
        this._repository = repository;
        this._payService = payService;
    }

    [SwaggerOperation(Summary = "Get employee by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetEmployeeDto>>> Get(int id)
    {
        var employee = await _repository.GetEmployeeAsync(id);
        if (employee != null)
        {
            IEnumerable<Dependent> dependents = await _repository.GetDependentsByEmployeeIdAsync(
                id
            );

            var employeeDto = new GetEmployeeDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Salary = employee.Salary,
                DateOfBirth = employee.DateOfBirth,
                Dependents = dependents
                    .Select(
                        (dep) =>
                            new GetDependentDto
                            {
                                Id = dep.Id,
                                FirstName = dep.FirstName,
                                LastName = dep.LastName,
                                Relationship = dep.Relationship,
                                DateOfBirth = dep.DateOfBirth
                            }
                    )
                    .ToImmutableList()
            };

            return Ok(new ApiResponse<GetEmployeeDto> { Data = employeeDto, Success = true });
        }
        else
            return NotFound(new ApiResponse<GetEmployeeDto>
            {
                Success = false,
                Error = "No employee found with that ID"
            });
    }

    [SwaggerOperation(Summary = "Get all employees")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetEmployeeDto>>>> GetAll()
    {
        var employees = await _repository.GetEmployeesAsync();
        var dependents = await _repository.GetDependentsAsync();

        var employeeDtos = employees
            .Select(
                (emp) =>
                    new GetEmployeeDto
                    {
                        Id = emp.Id,
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        DateOfBirth = emp.DateOfBirth,
                        Salary = emp.Salary,
                        Dependents = dependents
                            .Where((dep) => dep.EmployeeId == emp.Id)
                            .Select(
                                (dep) =>
                                    new GetDependentDto
                                    {
                                        Id = dep.Id,
                                        FirstName = dep.FirstName,
                                        LastName = dep.LastName,
                                        Relationship = dep.Relationship,
                                        DateOfBirth = dep.DateOfBirth
                                    }
                            )
                            .ToList()
                    }
            )
            .ToList();

        var result = new ApiResponse<List<GetEmployeeDto>> { Data = employeeDtos, Success = true };

        return Ok(result);
    }

    [HttpGet("{id}/annual-summary")]
    public async Task<ActionResult<ApiResponse<AnnualPayStatement>>> GetAnnualPayStatementByEmployeeId(int id)
    {
        var annualStatement = await _payService.GetAnnualPayStatementByEmployeeIdAsync(id);

        if (annualStatement != null)
            return Ok(new ApiResponse<AnnualPayStatement> { Data = annualStatement, Success = true });
        else
            return BadRequest(new ApiResponse<AnnualPayStatement> { Success = false, Error = "No statement found for that Employee ID." });
    }

    [SwaggerOperation(Summary = "Just seeding the database with employees/dependents")]
    [HttpGet("/seed")]
    public async Task<ActionResult<ApiResponse<bool>>> SeedDatabase()
    {
        if (await _repository.CreateTablesAsync())
        {
            var employees = new List<GetEmployeeDto>
            {
                new()
                {
                    Id = 1,
                    FirstName = "LeBron",
                    LastName = "James",
                    Salary = 75420.99m,
                    DateOfBirth = new DateTime(1984, 12, 30)
                },
                new()
                {
                    Id = 2,
                    FirstName = "Ja",
                    LastName = "Morant",
                    Salary = 92365.22m,
                    DateOfBirth = new DateTime(1999, 8, 10),
                    Dependents = new List<GetDependentDto>
                    {
                        new()
                        {
                            Id = 1,
                            FirstName = "Spouse",
                            LastName = "Morant",
                            Relationship = Relationship.Spouse,
                            DateOfBirth = new DateTime(1998, 3, 3)
                        },
                        new()
                        {
                            Id = 2,
                            FirstName = "Child1",
                            LastName = "Morant",
                            Relationship = Relationship.Child,
                            DateOfBirth = new DateTime(2020, 6, 23)
                        },
                        new()
                        {
                            Id = 3,
                            FirstName = "Child2",
                            LastName = "Morant",
                            Relationship = Relationship.Child,
                            DateOfBirth = new DateTime(2021, 5, 18)
                        }
                    }
                },
                new()
                {
                    Id = 3,
                    FirstName = "Michael",
                    LastName = "Jordan",
                    Salary = 143211.12m,
                    DateOfBirth = new DateTime(1963, 2, 17),
                    Dependents = new List<GetDependentDto>
                    {
                        new()
                        {
                            Id = 4,
                            FirstName = "DP",
                            LastName = "Jordan",
                            Relationship = Relationship.DomesticPartner,
                            DateOfBirth = new DateTime(1974, 1, 2)
                        }
                    }
                }
            };

            foreach (var employee in employees)
            {
                if (!await _repository.AddEmployeeAsync(employee))
                    return BadRequest(new ApiResponse<bool>
                    {
                        Data = false,
                        Success = false,
                        Message = $"Failed to add {employee.FirstName}"
                    });
            }

            return Ok(new ApiResponse<bool> { Data = true });
        }
        else
            return BadRequest(new ApiResponse<bool>
            {
                Data = false,
                Success = false,
                Error = "Error creating database"
            });
    }
}
