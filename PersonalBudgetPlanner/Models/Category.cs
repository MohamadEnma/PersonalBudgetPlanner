using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBudgetPlanner.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public TransactionType Type { get; set; }
    }
}
