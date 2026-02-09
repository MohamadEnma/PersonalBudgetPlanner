using PersonalBudgetPlanner.Commands;
using PersonalBudgetPlanner.Models;
using PersonalBudgetPlanner.Services;

namespace PersonalBudgetPlanner.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private UserProfile _userProfile;
        private readonly BudgetViewModel _parentViewModel;
        private readonly IBudgetService _budgetService; // Use the service!

        public UserProfile UserProfile
        {
            get => _userProfile;
            set { _userProfile = value; OnPropertyChanged(); }
        }

        public DelegateCommand SaveProfileCommand { get; }
        public DelegateCommand BackCommand { get; }

        public ProfileViewModel(BudgetViewModel parent, IBudgetService budgetService)
        {
            _parentViewModel = parent;
            _budgetService = budgetService;

            LoadProfile();

            // Simplified synchronous commands
            SaveProfileCommand = new DelegateCommand(_ => ExecuteSave());
            BackCommand = new DelegateCommand(_ => _parentViewModel.CurrentView = _parentViewModel);
        }

        private void LoadProfile()
        {
            // Get from service, not a new context
            UserProfile = _budgetService.GetCurrentUserProfile() ?? new UserProfile
            {
                AnnualIncome = 0,
                AnnualWorkHours = 1920
            };
        }

        private async Task ExecuteSave()
        {
            // Delegate the DB complexity to the service
            _budgetService.UpsertUserProfile(UserProfile);

            // Return to parent view
            _parentViewModel.CurrentView = _parentViewModel;

            // Refresh parent data (in case income changed)
            await _parentViewModel.InitializeAsync();
        }
    }
}
