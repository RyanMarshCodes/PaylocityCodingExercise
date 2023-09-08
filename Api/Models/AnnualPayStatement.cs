namespace Api.Models
{
    public class AnnualPayStatement
    {
        public int EmployeeId { get; set; }
        public decimal TotalSalary { get; set; }

        public decimal TotalDeductions
        {
            get
            {
                return AnnualDeductionSummary.TotalDeductions;
            }
        }

        public decimal SalaryPerCheck
        {
            get
            {
                return TotalSalary / 26;
            }
        }

        public decimal DeductionsPerCheck
        {
            get
            {
                return TotalDeductions / 26;
            }
        }
        public decimal NetPayPerCheck { 
            get
            {
                return SalaryPerCheck - DeductionsPerCheck;
            }
        }

        public AnnualDeductionSummary AnnualDeductionSummary { get; set; } = new AnnualDeductionSummary();
    }
}
