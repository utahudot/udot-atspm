using ATSPM.Data.Models.ConfigurationModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ATSPM.Application.Common.EqualityComparers
{
    public class ConfigEntityIdComparer<T, Tid> : EqualityComparer<T> where T : AtspmConfigModelBase<Tid>
    {
        public override bool Equals(T x, T y)
        {
            return x.Id.Equals(y.Id);
        }

        public override int GetHashCode([DisallowNull] T obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
