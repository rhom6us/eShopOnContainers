using System;
using System.Collections.Generic;
using System.Linq;
using eShopOnContainers.Core.ViewModels.Base;

namespace eShopOnContainers.Core.Validations {
    public class ValidatableObject<T> : ExtendedBindableObject, IValidity {
        public List<IValidationRule<T>> Validations { get; }

        public List<string> Errors {
            get => _errors;
            set {
                _errors = value;
                this.RaisePropertyChanged(() => this.Errors);
            }
        }

        public T Value {
            get => _value;
            set {
                _value = value;
                this.RaisePropertyChanged(() => this.Value);
            }
        }

        public static implicit operator T(ValidatableObject<T> source) {
            return source.Value;
        }

        public ValidatableObject() {
            _isValid = true;
            _errors = new List<string>();
            this.Validations = new List<IValidationRule<T>>();
        }

        public bool Validate() {
            this.Errors = this.Validations
                .Where(rule => !rule.Check(this.Value))
                .Select(rule => rule.ValidationMessage)
                .ToList();
            this.IsValid = !this.Errors.Any();

            return this.IsValid;
        }

        public bool IsValid {
            get => _isValid;
            set {
                _isValid = value;
                this.RaisePropertyChanged(() => this.IsValid);
            }
        }

        private List<string> _errors;
        private bool _isValid;
        private T _value;
    }

   
}
