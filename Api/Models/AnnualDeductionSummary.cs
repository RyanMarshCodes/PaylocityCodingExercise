namespace Api.Models
{
    public class AnnualDeductionSummary
    {
        public decimal TotalDeductions {  
            get
            {
                return Deductions.Sum((x) => x.DeductionAmount);
            } 
        }

        public decimal DeductionsPerCheck 
        { 
            get 
            {
                return TotalDeductions > 0 ? TotalDeductions / 26 : 0; // couldn't remember off the type of my head if this would result in a DivideByZero error lol
            } 
        }

        public List<Deduction> Deductions { get; set; } = new List<Deduction>();
    }
}
