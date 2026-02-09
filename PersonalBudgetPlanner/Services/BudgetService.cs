using Microsoft.EntityFrameworkCore;
using PersonalBudgetPlanner.Data;
using PersonalBudgetPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalBudgetPlanner.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _context;

        public BudgetService(AppDbContext context)
        {
            _context = context;
        }

        public List<Transaction> GetAllTransactions()
        {
            return _context.Transactions
                 .Include(t => t.Category)
                 .OrderByDescending(t => t.Date)
                 .ToList();
        }

        public List<Transaction> GetTransactionsForMonth(int month, int year)
        {
            return _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        private const decimal TaxFactor = 0.70m;

        public decimal CalculateNetMonthlySalary(decimal annualIncome)
            => (annualIncome / 12) * TaxFactor;

        public decimal GetTotalsIncomesForMonth(int month, int year)
        {
            return _context.Transactions
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .Join(_context.Categories, t => t.CategoryId, c => c.Id, (t, c) => new { t.Amount, c.Type })
                .Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
        }

        public decimal GetTotalExpensesForMonth(int month, int year)
        {
            var sum = _context.Transactions
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .Join(_context.Categories, t => t.CategoryId, c => c.Id, (t, c) => new { t.Amount, c.Type })
                .Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);

            return Math.Abs(sum);
        }

        public decimal GetNetBalanceForMonth(int month, int year)
        {
            var income = GetTotalsIncomesForMonth(month, year);
            var expense = GetTotalExpensesForMonth(month, year);
            return income - expense;
        }

        public void AddTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
        }

        public void DeleteTransaction(Guid id)
        {
            var transaction = _context.Transactions.Find(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                _context.SaveChanges();
            }
        }

        public List<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        public List<CategorySummary> GetCategorySummaries(int month, int year)
        {
            return _context.Transactions
                .Where(t => t.Date.Month == month && t.Date.Year == year && t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category.Name)
                .Select(g => new CategorySummary
                {
                    Name = g.Key ?? "Ospecificerad",
                    Value = g.Sum(t => t.Amount)
                })
                .OrderByDescending(s => s.Value)
                .ToList();
        }

        public UserProfile? GetCurrentUserProfile()
        {
            return _context.UserProfiles.FirstOrDefault();
        }

        public void AddAbsence(Absence absence)
        {
            _context.Absences.Add(absence);
            _context.SaveChanges();
        }

        public decimal CalculatePotentialLoss(int month, int year)
        {
            var profile = GetCurrentUserProfile();
            if (profile == null || profile.HourlyRate == 0) return 0;

            var totalAbsenceHours = _context.Absences
                .Where(a => a.Date.Month == month && a.Date.Year == year)
                .Sum(a => a.HoursMissed);

            return (decimal)totalAbsenceHours * profile.HourlyRate;
        }

        public decimal CalculateAbsenceCompensation(int month, int year)
        {
            var profile = GetCurrentUserProfile();
            if (profile == null || profile.HourlyRate == 0) return 0;

            var totalSjukHours = _context.Absences
                .Where(a => a.Date.Month == month && a.Date.Year == year && a.Type == AbsenceType.Sjuk)
                .Sum(a => a.HoursMissed);

            var totalVABHours = _context.Absences
                .Where(a => a.Date.Month == month && a.Date.Year == year && a.Type == AbsenceType.VAB)
                .Sum(a => a.HoursMissed);

            var sjukPay = (decimal)totalSjukHours * profile.HourlyRate * 0.8m;
            decimal baseHourlyRateForVAB = profile.AnnualIncome > 410000 ? 410000 / 52 / 40 : profile.AnnualIncome / 52 / 40;
            var vabPay = (decimal)totalVABHours * baseHourlyRateForVAB * 0.8m;

            return sjukPay + vabPay;
        }

        public void UpsertUserProfile(UserProfile profile)
        {
            using (var db = new AppDbContext())
            {
                var existing = db.UserProfiles.FirstOrDefault(p => p.Id == profile.Id);
                if (existing == null)
                {
                    if (profile.Id == Guid.Empty) profile.Id = Guid.NewGuid();
                    db.UserProfiles.Add(profile);
                }
                else
                {
                    // This copies the values from your ViewModel profile to the Database profile
                    db.Entry(existing).CurrentValues.SetValues(profile);
                }
                db.SaveChanges();
            }
        }
        public decimal CalculateRecurring(IEnumerable<Transaction> transactions, int targetMonth)
        {
            throw new NotImplementedException();
        }
    }
}
