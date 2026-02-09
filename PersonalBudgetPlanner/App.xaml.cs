using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalBudgetPlanner.Data;
using PersonalBudgetPlanner.Services;
using PersonalBudgetPlanner.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace PersonalBudgetPlanner
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContext<AppDbContext>();

            // Services
            services.AddSingleton<IBudgetService, BudgetService>();

            // ViewModels
            services.AddSingleton<BudgetViewModel>();
            services.AddSingleton<ProfileViewModel>(); // Don't forget this if you use it!

            // Windows
            services.AddSingleton<MainWindow>();
        }

        // KEY CHANGE: Add 'async' here
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Prepare Database safely BEFORE showing UI
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    // This ensures the DB is created and up to date
                    await db.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown(); // Close app if DB fails
                    return;
                }
            }

            // 2. NOW it is safe to show the window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // Optional: If your ViewModel has an Async Load method, call it here
            if (mainWindow.DataContext is BudgetViewModel vm)
            {
                // We don't await this because we want the window to appear immediately now that DB is safe
                _ = vm.InitializeAsync();
            }

            mainWindow.Show();
        }
    }
}