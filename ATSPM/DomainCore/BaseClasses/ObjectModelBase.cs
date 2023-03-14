using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace ATSPM.Domain.BaseClasses
{
    /// <summary>
    /// <c>ObjectModelBase</c> for data object models implementing:
    /// <list type="table">
    /// 
    /// <item>
    /// <term><see cref="INotifyPropertyChanged"/></term>
    /// <description>Notifies clients that a property value has changed.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="INotifyPropertyChanging"/></term>
    /// <description>Notifies clients that a property value is changing.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="IEditableObject"/></term>
    /// <description>Provides functionality to commit or rollback changes to an object that is used as a data source.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="INotifyDataErrorInfo"/></term>
    /// <description>Defines members that data entity classes can implement to provide custom synchronous and asynchronous validation support.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="IRevertibleChangeTracking"/></term>
    /// <description>Provides support for rolling back the changes.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="ICloneable"/></term>
    /// <description>Supports cloning, which creates a new instance of a class with the same value as an existing instance.</description>
    /// </item>
    /// 
    /// </list>
    /// </summary>
    public abstract class ObjectModelBase : ObservableObjectBase, IEditableObject, INotifyDataErrorInfo, IRevertibleChangeTracking, ICloneable
    {
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        private Dictionary<string, Dictionary<string, LambdaExpression>> _validationDictionary = new Dictionary<string, Dictionary<string, LambdaExpression>>();

        /// <summary>
        /// Dictionary of properties that have pending changes.
        /// </summary>
        protected Dictionary<string, object> changes = new Dictionary<string, object>();

        /// <summary>
        /// Sets a properties value and raises the <see cref="ObservableObjectBase.PropertyChanging"/> and <see cref="ObservableObjectBase.PropertyChanged"/> events if <paramref name="newValue"/> != <paramref name="currentValue"/>.
        /// </summary>
        /// <remarks>Overriden from <see cref="ObservableObjectBase"/> to check for validation errors and change tracking.</remarks>
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

        /// <summary>
        /// Add a validation rule. Object model will not set property values if rules are not met.
        /// <para>
        /// <example> Example:
        /// <code>AddValidationRule("TelephoneNumber", "Invalid Telephone Number", (DataModel, Value) => !DateModel.TelephoneNumber.IsValidTelephoneNumber(Value))</code>
        /// </example>
        /// </para>
        /// </summary>
        /// <remarks>Normally, rules would be defined in constructor of Object Model.</remarks>
        /// <typeparam name="T1">Object Model</typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="propertyName">Name of property to apply rule to.</param>
        /// <param name="message">Notification message if rule is not met.</param>
        /// <param name="rule">Rule expression to add to rules list for <paramref name="propertyName"/>.</param>
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

        /// <summary>
        /// Add change to <see cref="changes"/> for tracking.
        /// </summary>
        /// <typeparam name="T">Changed property type.</typeparam>
        /// <param name="propertyName">Changed property name.</param>
        /// <param name="value">Changed property value.</param>
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

        ///<inheritdoc/>
        [JsonIgnore]
        public bool HasErrors => _errors.Keys.Count > 0;

        ///<inheritdoc/>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public virtual void BeginEdit()
        {
            _isEditing = true;
        }

        ///<inheritdoc/>
        public virtual void CancelEdit()
        {
            _isEditing = false;

            RaisePropertyChanged(string.Empty);
        }

        ///<inheritdoc/>
        public virtual void EndEdit()
        {
            _isEditing = false;

            RaisePropertyChanged(string.Empty);
        }

        #endregion

        #region IRevertibleChangeTracking

        ///<inheritdoc/>
        [JsonIgnore]
        public bool IsChanged => changes.Keys.Count > 0;

        ///<inheritdoc/>
        public void AcceptChanges()
        {
            changes?.Clear();
            RaisePropertyChanged(nameof(IsChanged));
        }

        ///<inheritdoc/>
        public void RejectChanges()
        {
            //_changes?.Clear();
            //RaisePropertyChanged(nameof(IsChanged));
            throw new NotImplementedException("RejectChanges has not been implemented");
        }

        #endregion

        #region IClonable

        ///<inheritdoc/>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        /// <summary>
        /// Raise <see cref="ErrorsChanged"/> event that property has pending changes.
        /// </summary>
        /// <param name="propertyName">Name of property that has pending changes</param>
        protected void RaiseErrorChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(nameof(HasErrors));

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
