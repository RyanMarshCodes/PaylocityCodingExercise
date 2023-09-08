using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DependentsController : ControllerBase
{
    private ISQLiteRepository _repository;

    public DependentsController(ISQLiteRepository repository)
    {
        this._repository = repository;
    }

    [SwaggerOperation(Summary = "Get dependent by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetDependentDto>>> Get(int id)
    {
        var dependent = await _repository.GetDependentAsync(id);

        if (dependent != null)
        {
            var dependentDto = new GetDependentDto
            {
                Id = dependent.Id,
                FirstName = dependent.FirstName,
                LastName = dependent.LastName,
                DateOfBirth = dependent.DateOfBirth,
                Relationship = dependent.Relationship,
            };

            return Ok(new ApiResponse<GetDependentDto> { Data = dependentDto, Success = true });
        }
        else return NotFound(new ApiResponse<GetDependentDto> { Success = false, Error = "Could not find dependent with that ID" });

    }

    [SwaggerOperation(Summary = "Get all dependents")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetDependentDto>>>> GetAll()
    {
        var dependents = await _repository.GetDependentsAsync();

        var dependentDtos = dependents.Select((dep) => new GetDependentDto
        {
            Id = dep.Id, FirstName = dep.FirstName, LastName = dep.LastName, DateOfBirth = dep.DateOfBirth, Relationship = dep.Relationship
        }).ToList();

        if (dependents.Count() > 0)
            return Ok(new ApiResponse<List<GetDependentDto>> { Data = dependentDtos, Success = true });
        else
            return NotFound(new ApiResponse<List<GetDependentDto>> { Success = false, Error = "No dependents found " });

    }
}
