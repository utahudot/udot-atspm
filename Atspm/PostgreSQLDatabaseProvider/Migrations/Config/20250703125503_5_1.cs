#region license
// Copyright 2025 Utah Departement of Transportation
// for PostgreSQLDatabaseProvider - Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations/20250703125503_5_1.cs
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

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations
{
    /// <inheritdoc />
    public partial class _5_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "WatchDogIgnoreEvents",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WatchDogIgnoreEvents",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "WatchDogIgnoreEvents",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "WatchDogIgnoreEvents",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "VersionHistory",
                type: "character varying(64)",
                unicode: false,
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 3, 6, 55, 1, 778, DateTimeKind.Local).AddTicks(1158),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 2, 27, 9, 24, 49, 960, DateTimeKind.Local).AddTicks(1714));

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "VersionHistory",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "VersionHistory",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "VersionHistory",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "VersionHistory",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Routes",
                type: "character varying(100)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Routes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Routes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Routes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Routes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "RouteLocations",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RouteLocations",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "RouteLocations",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "RouteLocations",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "RouteDistances",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RouteDistances",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "RouteDistances",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "RouteDistances",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Regions",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Regions",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Regions",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Regions",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Products",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Products",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Products",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MenuItems",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MenuItems",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MenuItems",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MenuItems",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MeasureType",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MeasureType",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MeasureType",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MeasureType",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MeasureOptions",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MeasureOptions",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MeasureOptions",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MeasureOptions",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MeasureComments",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MeasureComments",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MeasureComments",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MeasureComments",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "LocationTypes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "LocationTypes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "LocationTypes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "LocationTypes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Locations",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Locations",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Locations",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Locations",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Jurisdictions",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Jurisdictions",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Jurisdictions",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Jurisdictions",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Faqs",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Faqs",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Faqs",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Faqs",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DirectionTypes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DirectionTypes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DirectionTypes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DirectionTypes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Devices",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Devices",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Devices",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Devices",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DeviceConfigurations",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DeviceConfigurations",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DeviceConfigurations",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DeviceConfigurations",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Detectors",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Detectors",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Detectors",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Detectors",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DetectorComments",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DetectorComments",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DetectorComments",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DetectorComments",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DetectionTypes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DetectionTypes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DetectionTypes",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DetectionTypes",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Areas",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Areas",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Areas",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Areas",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Approaches",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Approaches",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Approaches",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Approaches",
                type: "text",
                unicode: false,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeasureOptionPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    Option = table.Column<string>(type: "text", nullable: true),
                    MeasureTypeId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Modified = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", unicode: false, nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureOptionPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasureOptionPresets_MeasureType_MeasureTypeId",
                        column: x => x.MeasureTypeId,
                        principalTable: "MeasureType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Measure Option Presets");

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 0,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 0,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 71,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 80,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 83,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 85,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 92,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 116,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 117,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 118,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 119,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "Created", "CreatedBy", "MeasureTypeId", "Modified", "ModifiedBy", "Option", "Value" },
                values: new object[] { 120, null, null, 5, null, null, "yAxisDefault", "300" });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Created", "CreatedBy", "Modified", "ModifiedBy" },
                values: new object[] { null, null, null, null });

            migrationBuilder.InsertData(
                table: "MeasureType",
                columns: new[] { "Id", "Abbreviation", "Created", "CreatedBy", "DisplayOrder", "Modified", "ModifiedBy", "Name", "ShowOnAggregationSite", "ShowOnWebsite" },
                values: new object[] { 38, "TSP", null, null, 132, null, null, "Transit Signal Priority", false, false });

            migrationBuilder.CreateIndex(
                name: "IX_MeasureOptionPresets_MeasureTypeId",
                table: "MeasureOptionPresets",
                column: "MeasureTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeasureOptionPresets");

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DropColumn(
                name: "Created",
                table: "WatchDogIgnoreEvents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WatchDogIgnoreEvents");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "WatchDogIgnoreEvents");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "WatchDogIgnoreEvents");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "VersionHistory");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VersionHistory");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "VersionHistory");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "VersionHistory");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "RouteLocations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RouteLocations");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "RouteLocations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RouteLocations");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "RouteDistances");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RouteDistances");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "RouteDistances");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RouteDistances");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "MeasureType");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MeasureType");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "MeasureType");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MeasureType");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "MeasureOptions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MeasureOptions");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "MeasureOptions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MeasureOptions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "MeasureComments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MeasureComments");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "MeasureComments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MeasureComments");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Jurisdictions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Jurisdictions");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Jurisdictions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Jurisdictions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Faqs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Faqs");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Faqs");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Faqs");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "DirectionTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DirectionTypes");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "DirectionTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "DirectionTypes");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Detectors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Detectors");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Detectors");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Detectors");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "DetectorComments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DetectorComments");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "DetectorComments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "DetectorComments");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "DetectionTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DetectionTypes");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "DetectionTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "DetectionTypes");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Approaches");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Approaches");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Approaches");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Approaches");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "VersionHistory",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldUnicode: false,
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 27, 9, 24, 49, 960, DateTimeKind.Local).AddTicks(1714),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 7, 3, 6, 55, 1, 778, DateTimeKind.Local).AddTicks(1158));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Routes",
                type: "character varying(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
