#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Configuration/ProductConfiguration.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Product configuration
    /// </summary>
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasComment("Products");

            builder.Property(e => e.Manufacturer)
                .IsRequired()
                .HasMaxLength(48);

            builder.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(48);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);
        }
    }
}
