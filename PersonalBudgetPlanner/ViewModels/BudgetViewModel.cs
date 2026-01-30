using Microsoft.EntityFrameworkCore;
using PersonalBudgetPlanner.Commands;
using PersonalBudgetPlanner.Data;
using PersonalBudgetPlanner.Models;
using PersonalBudgetPlanner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PersonalBudgetPlanner.ViewModels
{
    public class BudgetViewModel : ViewModelBase
    {
        
        private readonly BudgetService _budgetService = new BudgetService();
        private DateTime _viewDate = DateTime.Now;
        
        private ObservableCollection<TransactionViewModel> _transactions;
        private ObservableCollection<Category> _categories;
        private TransactionViewModel _selectedTransaction;
        private TransactionViewModel _newTransaction;
        private decimal _forecastResult;
        private decimal _totalIncomes;
        private decimal _totalExpenses;

        private object _currentView;
        private double _sickHours;
        private AbsenceType _selectedAbsenceType = AbsenceType.Sjuk;

        // --- Properties ---
        public ObservableCollection<CategorySummary> CategoryData { get; } = new ObservableCollection<CategorySummary>();

        public ObservableCollection<TransactionViewModel> Transactions
        {
            get => _transactions;
            set { _transactions = value; OnPropertyChanged(); UpdateSummary(); }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public TransactionViewModel SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                OnPropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public TransactionViewModel NewTransaction
        {
            get => _newTransaction;
            set { _newTransaction = value; OnPropertyChanged(); }
        }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public double SickHours
        {
            get => _sickHours;
            set
            {
                if (_sickHours != value)
                {
                    _sickHours = value;
                    OnPropertyChanged();
                    UpdateSummary(); 
                }
            }
        }

        public AbsenceType SelectedAbsenceType
        {
            get => _selectedAbsenceType;
            set
            {
                _selectedAbsenceType = value;
                OnPropertyChanged();
                UpdateSummary(); 
            }
        }

        public decimal ForecastResult
        {
            get => _forecastResult;
            set { _forecastResult = value; 
                OnPropertyChanged(); }
        }

        public decimal TotalIncomes
        {
            get => _totalIncomes;
            set { _totalIncomes = value; OnPropertyChanged(); }
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set { _totalExpenses = value; OnPropertyChanged(); }
        }

        public decimal NetBalance => TotalIncomes - TotalExpenses;
        public string CurrentMonthDisplay => _viewDate.ToString("MMMM").ToUpper();
        public string CurrentYearDisplay => _viewDate.Year.ToString();

        // Enums för UI-bindning
        public IEnumerable<RecurrenceFrequency> Frequencies => Enum.GetValues(typeof(RecurrenceFrequency)).Cast<RecurrenceFrequency>();
        public IEnumerable<AbsenceType> AbsenceTypes => Enum.GetValues(typeof(AbsenceType)).Cast<AbsenceType>();

        // --- Commands ---
        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand PreviousMonthCommand { get; }
        public DelegateCommand NextMonthCommand { get; }
        public DelegateCommand OpenProfileCommand { get; }
        public DelegateCommand CloseViewCommand { get; } 
        public DelegateCommand AddAbsence { get; }
        public DelegateCommand SaveProfileCommand { get; }

        // --- Constructor ---
        public BudgetViewModel()
        {
            Transactions = new ObservableCollection<TransactionViewModel>();
            Categories = new ObservableCollection<Category>();
            NewTransaction = new TransactionViewModel(new Transaction { Date = DateTime.Now });
            CurrentView = this;

            AddCommand = new DelegateCommand(ExecuteAdd);
            DeleteCommand = new DelegateCommand(ExecuteDelete, _ => SelectedTransaction != null);
            PreviousMonthCommand = new DelegateCommand(_ => ChangeMonth(-1));
            NextMonthCommand = new DelegateCommand(_ => ChangeMonth(1));
            OpenProfileCommand = new DelegateCommand(ExecuteOpenProfile);
            CloseViewCommand = new DelegateCommand(_ => CurrentView = this);
            AddAbsence = new DelegateCommand(ExecuteAddAbsence);
           

            LoadData(_viewDate);
        }

        // --- Logic ---
        private void LoadData(DateTime date)
        {
            using (var db = new AppDbContext())
            {
                var categories = db.Categories.AsNoTracking().ToList();
                Categories = new ObservableCollection<Category>(categories);

                var transactions = db.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.Date.Month == date.Month && t.Date.Year == date.Year)
                    .AsNoTracking()
                    .ToList();

                Transactions = new ObservableCollection<TransactionViewModel>(
                    transactions.Select(t => new TransactionViewModel(t))
                );
            }
        }

        private void UpdateSummary()
        {
            TotalIncomes = Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            TotalExpenses = Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            using (var db = new AppDbContext())
            {
                var profile = db.UserProfiles.AsNoTracking().FirstOrDefault();
                if (profile != null)
                { 
                    var absences = db.Absences
                        .Where(a => a.Date.Month == _viewDate.Month && a.Date.Year == _viewDate.Year)
                        .ToList();
                    if (SickHours > 0)
                    {
                        absences.Add(new Absence { Type = AbsenceType.Sjuk, HoursMissed = (int)Math.Round(SickHours) });
                    }

                    CalculateForecastWithAbsence(profile, absences);
                }
            }
            UpdateCategorySummary();
            OnPropertyChanged(nameof(NetBalance));
        }

        private void ExecuteAdd(object obj)
        {
            if (NewTransaction.CategoryId == Guid.Empty) return;

            var selectedCategory = Categories.FirstOrDefault(c => c.Id == NewTransaction.CategoryId);
            if (selectedCategory == null) return;

            using (var db = new AppDbContext())
            {
                var modelToSave = NewTransaction.GetModel();
                if (modelToSave.Id == Guid.Empty) modelToSave.Id = Guid.NewGuid();
                modelToSave.CategoryId = selectedCategory.Id;
                modelToSave.Type = selectedCategory.Type;
                db.Transactions.Add(modelToSave);
                db.SaveChanges();
                modelToSave.Category = selectedCategory;
                Transactions.Add(new TransactionViewModel(modelToSave));
            }

            UpdateSummary();
            NewTransaction = new TransactionViewModel(new Transaction { Date = DateTime.Now });
        }

        private void ExecuteDelete(object obj)
        {
            if (SelectedTransaction == null) return;

            using (var db = new AppDbContext())
            {
                var transactionToDelete = db.Transactions.Find(SelectedTransaction.Id);
                if (transactionToDelete != null)
                {
                    db.Transactions.Remove(transactionToDelete);
                    db.SaveChanges();
                }
            }

            Transactions.Remove(SelectedTransaction);
            SelectedTransaction = null;
            UpdateSummary();
        }

        private void ExecuteOpenProfile(object obj)
        {
            
            CurrentView = new ProfileViewModel(this);
        }

        private void ChangeMonth(int months)
        {
            _viewDate = _viewDate.AddMonths(months);
            OnPropertyChanged(nameof(CurrentMonthDisplay));
            OnPropertyChanged(nameof(CurrentYearDisplay));
            LoadData(_viewDate);
        }

    
        public void CalculateForecast(UserProfile profile)
        {
            if (profile == null) return;
            decimal netSalary = _budgetService.CalculateNetMonthlySalary(profile.AnnualIncome);
            int nextMonth = DateTime.Now.AddMonths(1).Month;
            decimal recurringBalance = _budgetService.CalculateRecurringTotal(Transactions, nextMonth);
            ForecastResult = netSalary + recurringBalance;
        }

        public void CalculateForecastWithAbsence(UserProfile profile, IEnumerable<Absence> monthlyAbsences)
        {
            // Starta med nettolön (redan skattad)
            CalculateForecast(profile);

            decimal totalImpact = 0;
            // Gruppera frånvaro per typ så att vi inte får flera karensavdrag för samma sjukperiod
            var groupedAbsences = monthlyAbsences.GroupBy(a => a.Type);

            foreach (var group in groupedAbsences)
            {
                decimal hourlyRate = profile.AnnualWorkHours > 0 ? profile.AnnualIncome / (decimal)profile.AnnualWorkHours : 0;
                int totalHoursForType = group.Sum(a => a.HoursMissed);

                // 1. Vad försvinner från nettolönen? (Bruttoavdrag * 0.7)
                decimal grossDeduction = _budgetService.CalculateAbsenceDeduction(
                    profile.AnnualIncome,
                    profile.AnnualWorkHours,
                    totalHoursForType,
                    group.Key);

                decimal netDeduction = grossDeduction * 0.70m;

                // 2. Vad får vi in i ersättning? (Använder nu korrigerad service)
                decimal netCompensation = _budgetService.CalculateCompensation(grossDeduction, group.Key, hourlyRate);

                totalImpact += (netCompensation - netDeduction);
            }

            ForecastResult += totalImpact;
        }

        private void UpdateCategorySummary()
        {
            var summary = Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.CategoryName)
                .Select(g => new CategorySummary
                {
                    Name = g.Key ?? "Ospecificerad",
                    Value = g.Sum(t => t.Amount)
                })
                .OrderByDescending(s => s.Value)
                .ToList();

            CategoryData.Clear();
            foreach (var item in summary) CategoryData.Add(item);
        }

        private void ExecuteAddAbsence(object obj)
        {
            if (SickHours <= 0) return;
            using (var db = new AppDbContext())
            { 
                var profile = db.UserProfiles.FirstOrDefault();
                if (profile == null) return;
                var newAbsence = new Absence
                {
                    Id = Guid.NewGuid(),
                    Type = AbsenceType.Sjuk, 
                    HoursMissed = (int)Math.Round(SickHours),
                    Date = _viewDate, 
                    User = profile
                };
                
                db.Absences.Add(newAbsence);
                db.SaveChanges();
            }
            SickHours = 0; 
            UpdateSummary();
        }
    }
}