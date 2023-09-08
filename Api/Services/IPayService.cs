using Api.Models;

namespace Api.Services
{
    public interface IPayService
    {
        public Task<AnnualPayStatement?> GetAnnualPayStatementByEmployeeIdAsync(int employeeId);
        public Task<AnnualPayStatement> GetAnnualPayStatementByEmployeeAsync(Employee employee);
    }
}
