using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBudgetPlanner.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public Guid CategoryId { get; set; }
        public  Category Category { get; set; }
        public TransactionType Type { get; set; } = TransactionType.Expense;
        public bool IsRecurring { get; set; }
        public RecurrenceFrequency Recurrence { get; set; } = RecurrenceFrequency.None;
    }

    public enum TransactionType
    {
        Income,
        Expense
    }

    public enum RecurrenceFrequency
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
