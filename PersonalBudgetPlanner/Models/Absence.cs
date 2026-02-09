

namespace PersonalBudgetPlanner.Models
{
    public class Absence
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        public AbsenceType Type { get; set; }
        public int HoursMissed { get; set; }
        public UserProfile User { get; set; }
    }

    public enum AbsenceType
    {
        Sjuk, 
        VAB   
    }
}
