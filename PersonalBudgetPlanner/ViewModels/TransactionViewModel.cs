using PersonalBudgetPlanner.Models;
using System;

namespace PersonalBudgetPlanner.ViewModels
{
    public class TransactionViewModel : ViewModelBase
    {
        private readonly Transaction _model;

        public TransactionViewModel(Transaction model)
        {
            _model = model ?? new Transaction();
        }

        public Guid Id => _model.Id;

        public decimal Amount
        {
            get => _model.Amount;
            set
            {
                if (_model.Amount != value)
                {
                    _model.Amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _model.Description;
            set
            {
                if (_model.Description != value)
                {
                    _model.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Date
        {
            get => _model.Date;
            set
            {
                if (_model.Date != value)
                {
                    _model.Date = value;
                    OnPropertyChanged();
                }
            }
        }

        public TransactionType Type
        {
            get => _model.Type;
            set
            {
                if (_model.Type != value)
                {
                    _model.Type = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CategoryName => _model.Category?.Name ?? "Ingen kategori";

        public RecurrenceFrequency Recurrence
        {
            get => _model.Recurrence;
            set
            {
                if (_model.Recurrence != value)
                {
                    _model.Recurrence = value;
                    // Notify Recurrence changed
                    OnPropertyChanged(nameof(Recurrence));
                    // Update IsRecurring in the model and notify that property explicitly
                    _model.IsRecurring = value != RecurrenceFrequency.None;
                    OnPropertyChanged(nameof(IsRecurring));
                }
            }
        }

        public bool IsRecurring => _model.IsRecurring;

        public Guid CategoryId
        {
            get => _model.CategoryId;
            set
            {
                if (_model.CategoryId != value)
                {
                    _model.CategoryId = value;
                    // If you want Category to be reloaded, do so in the saving/loading layer.
                    OnPropertyChanged(nameof(CategoryId));
                    // Notify that CategoryName may have changed
                    OnPropertyChanged(nameof(CategoryName));
                }
            }
        }

        public Transaction GetModel() => _model;
    }
}

