using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace ATSPM.Domain.BaseClasses
{
    public abstract class ObjectModelBase : ObservableObjectBase, IEditableObject, INotifyDataErrorInfo, IRevertibleChangeTracking, ICloneable
    {
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        private Dictionary<string, Dictionary<string, LambdaExpression>> _validationDictionary = new Dictionary<string, Dictionary<string, LambdaExpression>>();
        protected Dictionary<string, object> changes = new Dictionary<string, object>();

        protected override bool Set<T>(ref T currentValue, T newValue, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = "")
        {
            //check for validation errors
            if (PropertyHasErrors(propertyName, newValue)) { return false; }

            //check IEquatable<T>
            if (currentValue is IEquatable<T> equate && equate.Equals(newValue)) { return false; }

            //see if value has changed
            if (comparer.Equals(currentValue, newValue)) { return false; }

            //raise property changing
            RaisePropertyChanging(propertyName);

            //IsChanged
            AddChange(propertyName, currentValue);

            //update value
            currentValue = newValue;

            //raise property changed
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void AddValidationRule<T1, T2>(string propertyName, string message, Expression<Func<T1, T2, bool>> rule) where T1 : ObjectModelBase
        {
            if (!_validationDictionary.ContainsKey(propertyName))
            {
                _validationDictionary.Add(propertyName, new Dictionary<string, LambdaExpression>());
            }

            if (_validationDictionary.TryGetValue(propertyName, out Dictionary<string, LambdaExpression> rules))
            {
                rules.Add(message, rule);
            }
        }

        private bool PropertyHasErrors<T>(string propertyName, T value)
        {
            if (_validationDictionary.ContainsKey(propertyName))
            {
                if (_validationDictionary.TryGetValue(propertyName, out Dictionary<string, LambdaExpression> rules))
                {
                    _errors.Remove(propertyName);
                    List<string> errors = new List<string>();

                    foreach (string key in rules.Keys)
                    {
                        if ((bool)rules[key].Compile().DynamicInvoke(this, value))
                        {
                            errors.Add(key);
                        }
                    }

                    if (errors.Count > 0) { _errors.Add(propertyName, errors); }
                }

                RaiseErrorChanged(propertyName);

                return _errors.ContainsKey(propertyName);
            }
            else
                return false;
        }

        protected virtual void AddChange<T>(string propertyName, T value)
        {
            //if IEditableObject is active don't store changes
            if (_isEditing)
            {
                changes.Clear();
                return;
            }

            if (changes.TryGetValue(propertyName, out _))
            {
                changes[propertyName] = value;
            }
            else
            {
                changes.Add(propertyName, value);
            }

            RaisePropertyChanged(nameof(IsChanged));
        }

        #region INotifyDataErrorInfo

        //[Newtonsoft.Json.JsonIgnore]
        public bool HasErrors => _errors.Keys.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (_errors.TryGetValue(propertyName, out List<string> value))
                {
                    return value;
                }
                else
                {
                    return new List<string>();
                }
            }

            if (_errors.Count > 0)
            {
                var boo = _errors.Values.Aggregate((l1, l2) => l1.Union(l2).ToList());
                return boo;
            }

            return new List<string>();
        }

        #endregion

        #region IEditableObject

        private bool _isEditing;

        public virtual void BeginEdit()
        {
            _isEditing = true;
        }

        public virtual void CancelEdit()
        {
            _isEditing = false;

            RaisePropertyChanged(string.Empty);
        }

        public virtual void EndEdit()
        {
            _isEditing = false;

            RaisePropertyChanged(string.Empty);
        }

        #endregion

        #region IRevertibleChangeTracking

        //[Newtonsoft.Json.JsonIgnore]
        public bool IsChanged => changes.Keys.Count > 0;

        public void AcceptChanges()
        {
            changes?.Clear();
            RaisePropertyChanged(nameof(IsChanged));
        }

        public void RejectChanges()
        {
            //_changes?.Clear();
            //RaisePropertyChanged(nameof(IsChanged));
            throw new NotImplementedException("RejectChanges is not complete");
        }

        #endregion

        #region IClonable

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        protected void RaiseErrorChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(nameof(HasErrors));

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
