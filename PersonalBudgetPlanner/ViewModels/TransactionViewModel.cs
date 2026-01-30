using PersonalBudgetPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string CategoryName
        {
            get => _model.Category?.Name ?? "Ingen kategori";
        }


        public RecurrenceFrequency Recurrence
        {
            get => _model.Recurrence;
            set
            {
                if (_model.Recurrence != value)
                {
                    _model.Recurrence = value;
                    OnPropertyChanged();
                    _model.IsRecurring = value != RecurrenceFrequency.None;
                    OnPropertyChanged();
                   
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
                    OnPropertyChanged();
                }
            }
        }

        public Transaction GetModel() => _model;
    }
}
