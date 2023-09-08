using Api.Models;
using Api.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace Api.Services
{
    public class PayService : IPayService
    {
        private ISQLiteRepository _repository;
        private const decimal BASE_DEDUCTION = 1000.00m;
        private const decimal DEPENDENT_DEDUCTION = 600.00m;
        private const decimal OVER_80K_DEDUCATION_RATE = 0.02m;
        private const decimal DEPENDENT_OVER_50_DEDUCTION = 200.00m;


        public PayService(ISQLiteRepository repository)
        {
            this._repository = repository;
        }

        public async Task<AnnualPayStatement?> GetAnnualPayStatementByEmployeeIdAsync(int employeeId)
        {
            var employee = await _repository.GetEmployeeAsync(employeeId);

            if (employee != null)
            {
                return await GetAnnualPayStatementByEmployeeAsync(employee);
            }

            else return null;
        }

        public async Task<AnnualPayStatement> GetAnnualPayStatementByEmployeeAsync(Employee employee) // Not 100% sure I implemented this correctly
        {
            var payStatement = new AnnualPayStatement
            {
                EmployeeId = employee.Id,
                TotalSalary = employee.Salary,
            };

            decimal salary = employee.Salary; // start here

            var dependents = await _repository.GetDependentsByEmployeeIdAsync(employee.Id);

            CalculateBaseSalaryDeduction(salary, payStatement.AnnualDeductionSummary);  // Thinking about this now I probably should have done a lot of this logic inside the AnnualPayStatement class
            CalculateDependentDeductions(dependents, salary, payStatement.AnnualDeductionSummary);

            return payStatement;
        }

        private void CalculateBaseSalaryDeduction(decimal salary, AnnualDeductionSummary summary) // Employee base cost of $1000/month + if employee makes more than $80k/year, extra 2% of salary is taken for the year?
        {
            if (salary > 80000)
            {
                var deductionAmount = salary * OVER_80K_DEDUCATION_RATE;
                salary -= deductionAmount;
                summary.Deductions.Add(new Deduction { DeductionAmount = deductionAmount, DeductionType = DeductionType.SalaryOver80k });
            }

            var baseDeduction = BASE_DEDUCTION * 12;
            salary -= baseDeduction;
            summary.Deductions.Add(new Deduction { DeductionAmount = baseDeduction, DeductionType = DeductionType.Base });
        }

        private void CalculateDependentDeductions(IEnumerable<Dependent> dependents, decimal salary, AnnualDeductionSummary summary)
        {
            foreach (var dependent in dependents)
            {
                var baseDeduction = DEPENDENT_DEDUCTION * 12;
                salary -= baseDeduction; // $600/month per dependent
                summary.Deductions.Add(new Deduction { DeductionAmount = baseDeduction, DeductionType = DeductionType.Dependent });

                if (CalculateAgeInYears(dependent.DateOfBirth) > 50)
                {
                    var deductionForYear = DEPENDENT_OVER_50_DEDUCTION * 12;
                    salary -= deductionForYear; // additional $200/month if dependent over 50
                    summary.Deductions.Add(new Deduction { DeductionAmount = deductionForYear, DeductionType = DeductionType.DependentOver50 });
                }
            }
        }

        private int CalculateAgeInYears(DateTime dateOfBirth)
        {
            var today = DateTime.Now;
            int age = today.Year - dateOfBirth.Year;

            if (today.Month < dateOfBirth.Month || (today.Month == dateOfBirth.Month & today.Day < dateOfBirth.Day))
                age--;

            return age;
        }
    }
}
