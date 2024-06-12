#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.BaseClasses/ObservableObjectBase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Domain.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ATSPM.Domain.BaseClasses
{
    /// <summary>
    /// <c>ObservableObjectBase</c> for observable objects implementing:
    /// <list type="table">
    /// <item>
    /// <term><see cref="INotifyPropertyChanged"/></term>
    /// <description>Notifies clients that a property value has changed.</description>
    /// </item>
    /// <item>
    /// <term><see cref="INotifyPropertyChanging"/></term>
    /// <description>Notifies clients that a property value is changing.</description>
    /// </item>
    /// </list>
    /// </summary>
    public abstract class ObservableObjectBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region INotifyPropertyChanged

        ///<inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyPropertyChanging

        ///<inheritdoc/>
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        /// <summary>
        /// Sets a properties value and raises the <see cref="PropertyChanging"/> and <see cref="PropertyChanged"/> events if <paramref name="newValue"/> != <paramref name="currentValue"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentValue">Current property value.</param>
        /// <param name="newValue">New value to change property to.</param>
        /// <param name="propertyName">Name of property to change value of.</param>
        /// <returns>Returns <c>false</c> if property is changed to <paramref name="newValue"/>, else returns <c>false</c> </returns>
        protected virtual bool Set<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            return Set(ref currentValue, newValue, new LambdaEqualityComparer<T>((x, y) => Equals(x, y)), propertyName);
        }

        /// <summary>
        /// Sets a properties value and raises the <see cref="PropertyChanging"/> and <see cref="PropertyChanged"/> events if <paramref name="newValue"/> != <paramref name="currentValue"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentValue">Current property value.</param>
        /// <param name="newValue">New value to change property to.</param>
        /// <param name="comparer">Custom <see cref="IEqualityComparer"/> to compare <paramref name="newValue"/> and <paramref name="currentValue"/> </param>
        /// <param name="propertyName">Name of property to change value of.</param>
        /// <returns>Returns <c>false</c> if property is changed to <paramref name="newValue"/>, else returns <c>false</c> </returns>
        protected virtual bool Set<T>(ref T currentValue, T newValue, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = "")
        {
            //check IEquatable<T>
            if (currentValue is IEquatable<T> equate && equate.Equals(newValue)) { return false; }

            //see if value has changed
            if (comparer.Equals(currentValue, newValue)) { return false; }

            //raise property changing
            RaisePropertyChanging(propertyName);

            //update value
            currentValue = newValue;

            //raise property changed
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Rasie <see cref="PropertyChanging"/> event that property is changing.
        /// </summary>
        /// <param name="propertyName">Name of property that is changing.</param>
        public virtual void RaisePropertyChanging([CallerMemberName] string propertyName = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// Rasie <see cref="PropertyChanged"/> event that property has changed.
        /// </summary>
        /// <param name="propertyName">Name of property that has changed.</param>
        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
