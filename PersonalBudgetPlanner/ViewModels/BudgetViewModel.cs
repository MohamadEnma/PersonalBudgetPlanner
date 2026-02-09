using Microsoft.EntityFrameworkCore;
using PersonalBudgetPlanner.Commands;
using PersonalBudgetPlanner.Data;
using PersonalBudgetPlanner.Models;
using PersonalBudgetPlanner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace PersonalBudgetPlanner.ViewModels
{
    public class BudgetViewModel : ViewModelBase
    {

        private readonly IBudgetService _budgetService;
        private DateTime _viewDate = DateTime.Now;
        
        private ObservableCollection<TransactionViewModel> _transactions;
        private ObservableCollection<Category> _categories;
        private TransactionViewModel _selectedTransaction;
        private TransactionViewModel _newTransaction;
        private decimal _totalIncomes;
        private decimal _totalExpenses;
        private decimal _netBalance;
        private object _currentView;
        private double _sickHours;
        private AbsenceType _selectedAbsenceType;
        private UserProfile _userProfile;



        public ObservableCollection<CategorySummary> CategoryData { get; } = new ObservableCollection<CategorySummary>();
        public string CurrentMonthDisplay => _viewDate.ToString("MMMM").ToUpper();
        public string CurrentYearDisplay => _viewDate.Year.ToString();

        // Enums för UI
        public IEnumerable<RecurrenceFrequency> Frequencies => Enum.GetValues(typeof(RecurrenceFrequency)).Cast<RecurrenceFrequency>();
        public IEnumerable<AbsenceType> AbsenceTypes => Enum.GetValues(typeof(AbsenceType)).Cast<AbsenceType>();

        public ObservableCollection<TransactionViewModel> Transactions
        {
            get => _transactions;
            set { _transactions = value; 
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set { _categories = value; 
                OnPropertyChanged(); }
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
            set { _newTransaction = value;
                OnPropertyChanged(); }
        }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value;
                OnPropertyChanged(); }
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
            }
        }

        

        public decimal TotalIncomes
        {
            get => _totalIncomes;
            set { _totalIncomes = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set { _totalExpenses = value;
                OnPropertyChanged(); }
        }

        public decimal NetBalance
        {
            get => _netBalance;
            set
            {
                _netBalance = value;
                OnPropertyChanged();
            }
        }

        public UserProfile CurrentUserProfile
        {
            get { return _userProfile; }
            set { 
                _userProfile = value; 
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value;
                OnPropertyChanged();
                PreviousMonthCommand.RaiseCanExecuteChanged();
                NextMonthCommand.RaiseCanExecuteChanged(); }
        }


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
        public BudgetViewModel(IBudgetService budgetService)
        {
            _budgetService = budgetService;

            Transactions = new ObservableCollection<TransactionViewModel>();
            Categories = new ObservableCollection<Category>();
            NewTransaction = new TransactionViewModel(new Transaction { Date = DateTime.Now });
            CurrentView = this;

            // Commands calling synchronous execution methods
            AddCommand = new DelegateCommand(async _ => await ExecuteAdd(null));
            DeleteCommand = new DelegateCommand(async _ => await ExecuteDelete(null), _ => SelectedTransaction != null);
            PreviousMonthCommand = new DelegateCommand(_ => ChangeMonth(-1), _ => !IsBusy);
            NextMonthCommand = new DelegateCommand(_ => ChangeMonth(1), _ => !IsBusy);
            AddAbsence = new DelegateCommand(async _ => await ExecuteAddAbsence(null));
            OpenProfileCommand = new DelegateCommand(_ => ExecuteOpenProfile(null));
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var cats = _budgetService.GetCategories();
                    var trans = _budgetService.GetTransactionsForMonth(_viewDate.Month, _viewDate.Year);
                    var inc = _budgetService.GetTotalsIncomesForMonth(_viewDate.Month, _viewDate.Year);
                    var exp = _budgetService.GetTotalExpensesForMonth(_viewDate.Month, _viewDate.Year);
                    var profile = _budgetService.GetCurrentUserProfile();

                    // Push to UI Thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Categories = new ObservableCollection<Category>(cats);
                        Transactions = new ObservableCollection<TransactionViewModel>(
                            trans.Select(t => new TransactionViewModel(t)));

                        TotalIncomes = inc;
                        TotalExpenses = exp;
                        NetBalance = inc - exp;
                        CurrentUserProfile = profile;
                        UpdateCategorySummary();
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading data: {ex.Message}");
                }
            });
        }

        private async Task ExecuteAdd(object obj)
        {
            if (NewTransaction.CategoryId == Guid.Empty) return;

            var modelToSave = NewTransaction.GetModel();
            var selectedCategory = Categories.FirstOrDefault(c => c.Id == NewTransaction.CategoryId);

            if (selectedCategory != null)
            {
                modelToSave.Type = selectedCategory.Type;
            }

            _budgetService.AddTransaction(modelToSave); 

            NewTransaction = new TransactionViewModel(new Transaction { Date = DateTime.Now });
            await InitializeAsync(); 
        }

        private async Task ExecuteDelete(object obj)
        {
            if (SelectedTransaction == null) return;

            _budgetService.DeleteTransaction(SelectedTransaction.Id); 
            SelectedTransaction = null;
            await InitializeAsync(); 
        }

        private void ExecuteOpenProfile(object parameter)
        {
            CurrentView = new ProfileViewModel(this, _budgetService);
        }

        private void ChangeMonth(int months)
        {
            IsBusy = true;
            _viewDate = _viewDate.AddMonths(months);
            Task.Run(async () => {
                Application.Current.Dispatcher.Invoke(() => {
                    IsBusy = false;
                    OnPropertyChanged(nameof(CurrentMonthDisplay));
                    OnPropertyChanged(nameof(CurrentYearDisplay));
                });
                await InitializeAsync();
            });
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



        private async Task ExecuteAddAbsence(object obj)
        {
            if (SickHours <= 0) return;
            var profile = _budgetService.GetCurrentUserProfile();
            if (profile == null) return;

            var newAbsence = new Absence
            {
                Id = Guid.NewGuid(),
                Type = SelectedAbsenceType,
                HoursMissed = (int)Math.Round(SickHours),
                Date = _viewDate,
                User = profile
            };

            _budgetService.AddAbsence(newAbsence); 

            SickHours = 0;
            await InitializeAsync(); 
        }

    }
}

