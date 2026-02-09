using PersonalBudgetPlanner.ViewModels;
using System.Windows;

namespace PersonalBudgetPlanner
{
    public partial class MainWindow : Window
    {
        public MainWindow(BudgetViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            this.Loaded += async (s, e) =>
            {
                await vm.InitializeAsync();
            };
        }
    }
}