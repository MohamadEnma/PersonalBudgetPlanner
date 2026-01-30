using PersonalBudgetPlanner.Models;
using PersonalBudgetPlanner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBudgetPlanner.Services
{
    public class BudgetService
    {
        private const decimal TaxFactor = 0.70m; // 30% skatt

        public decimal CalculateNetMonthlySalary(decimal annualIncome)
            => (annualIncome / 12) * TaxFactor;

        public decimal CalculateRecurringTotal(IEnumerable<TransactionViewModel> transactions, int targetMonth)
        {
            return transactions.Sum(t =>
            {
                bool isRelevant = t.Recurrence == RecurrenceFrequency.Monthly ||
                                 (t.Recurrence == RecurrenceFrequency.Yearly && t.Date.Month == targetMonth);

                if (!isRelevant) return 0;
                return t.Type == TransactionType.Income ? t.Amount : -t.Amount;
            });
        }

        public decimal CalculateAbsenceDeduction(decimal annualIncome, double annualHours, int missedHours, AbsenceType type)
        {
            if (annualHours <= 0 || missedHours <= 0) return 0;
            decimal calculationBase = (type == AbsenceType.VAB && annualIncome > 410000) ? 410000 : annualIncome;
            decimal hourlyRate = calculationBase / (decimal)annualHours;
            decimal totalDeduction = hourlyRate * missedHours;
            return totalDeduction;
        }

        public decimal CalculateCompensation(decimal deduction, AbsenceType type, decimal hourlyRate)
        {
            if (type == AbsenceType.VAB)
            {
                return deduction * 0.80m; 
            }
            decimal grossSickPay = deduction * 0.80m;
            decimal karensAmount = (hourlyRate * 8.0m) * 0.80m;

            return Math.Max(0, grossSickPay - karensAmount);
        }
    }
}
