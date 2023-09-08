using Api.Dtos.Employee;
using Api.Models;

namespace Api.Repositories;

public interface ISQLiteRepository
{
    public Task<Employee?> GetEmployeeAsync(int id);
    public Task<IEnumerable<Employee>> GetEmployeesAsync();
    public Task<Dependent?> GetDependentAsync(int id);
    public Task<IEnumerable<Dependent>> GetDependentsAsync();
    public Task<IEnumerable<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId);
    public Task<bool> AddEmployeeAsync(GetEmployeeDto employee);
    public Task<bool> CreateTablesAsync();
}
