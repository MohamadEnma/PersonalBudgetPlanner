using Microsoft.EntityFrameworkCore;
using PersonalBudgetPlanner.Commands;
using PersonalBudgetPlanner.Data;
using PersonalBudgetPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBudgetPlanner.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private UserProfile _userProfile;
        private readonly BudgetViewModel _parentViewModel;

        public UserProfile UserProfile
        {
            get => _userProfile;
            set { _userProfile = value; OnPropertyChanged(); }
        }

        public DelegateCommand SaveProfileCommand { get; }
        public DelegateCommand BackCommand { get; }

        public ProfileViewModel(BudgetViewModel parent)
        {
            _parentViewModel = parent;
            LoadProfile();

            SaveProfileCommand = new DelegateCommand(ExecuteSave);
            BackCommand = new DelegateCommand(_ => _parentViewModel.CurrentView = _parentViewModel);
        }

        private void LoadProfile()
        {
            using (var db = new AppDbContext())
            {         
                UserProfile = db.UserProfiles.FirstOrDefault() ?? new UserProfile
                {
                    AnnualIncome = 0,
                    AnnualWorkHours = 1920
                };
            }
        }

        private void ExecuteSave(object obj)
        {
            using (var db = new AppDbContext())
            {
                var existingProfile = db.UserProfiles.FirstOrDefault(p => p.Id == UserProfile.Id);
                if (existingProfile == null)
                {
                    if (UserProfile.Id == Guid.Empty) UserProfile.Id = Guid.NewGuid();
                    db.UserProfiles.Add(UserProfile);
                }
                else
                {
                    db.Entry(existingProfile).CurrentValues.SetValues(UserProfile);
                }
                db.SaveChanges();
            }
            _parentViewModel.CalculateForecast(UserProfile);
            _parentViewModel.CurrentView = _parentViewModel;
        }
    }
}
