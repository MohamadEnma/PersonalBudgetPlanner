using PersonalBudgetPlanner.Models;

namespace PersonalBudgetPlanner.Services
{
    public interface IBudgetService
    {
        // Transaction methods
        List<Transaction> GetTransactionsForMonth(int month, int year);
        List<Transaction> GetAllTransactions();
        void AddTransaction(Transaction transaction);
        void DeleteTransaction(Guid id);
        decimal CalculateRecurring(IEnumerable<Transaction> transactions, int targetMonth);

        // Category methods
        List<Category> GetCategories();
        List<CategorySummary> GetCategorySummaries(int month, int year);

        // Summary methods
        decimal GetTotalsIncomesForMonth(int month, int year);
        decimal GetTotalExpensesForMonth(int month, int year);
        decimal GetNetBalanceForMonth(int month, int year);

        // User & Absence methods
        UserProfile GetCurrentUserProfile();
        void AddAbsence(Absence absence);
        void UpsertUserProfile(UserProfile profile);
        decimal CalculatePotentialLoss(int month, int year);
        decimal CalculateAbsenceCompensation(int month, int year);
    }
}

