using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBudgetPlanner.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public decimal AnnualIncome { get; set; } 
        public double AnnualWorkHours { get; set; } 
        public decimal HourlyRate => AnnualWorkHours > 0 ? AnnualIncome / (decimal)AnnualWorkHours : 0;

    }
}
