#region license
// Copyright 2025 Utah Departement of Transportation
// for SqlDatabaseProvider - Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations/20250703125932_5_1.cs
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

#nullable disable

namespace Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations
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
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WatchDogIgnoreEvents",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "WatchDogIgnoreEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "WatchDogIgnoreEvents",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "VersionHistory",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 3, 6, 59, 30, 738, DateTimeKind.Local).AddTicks(4621),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 2, 27, 9, 29, 9, 10, DateTimeKind.Local).AddTicks(3603));

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "VersionHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "VersionHistory",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "VersionHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "VersionHistory",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Routes",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Routes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Routes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Routes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Routes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "RouteLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RouteLocations",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "RouteLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "RouteLocations",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "RouteDistances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RouteDistances",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "RouteDistances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "RouteDistances",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Regions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Regions",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Regions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Regions",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Products",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MenuItems",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MenuItems",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MeasureType",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MeasureType",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MeasureType",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MeasureType",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MeasureOptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MeasureOptions",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MeasureOptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MeasureOptions",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MeasureComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MeasureComments",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "MeasureComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MeasureComments",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "LocationTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "LocationTypes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "LocationTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "LocationTypes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Locations",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Locations",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Jurisdictions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Jurisdictions",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Jurisdictions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Jurisdictions",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Faqs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Faqs",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Faqs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Faqs",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DirectionTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DirectionTypes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DirectionTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DirectionTypes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Devices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Devices",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Devices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Devices",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DeviceConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DeviceConfigurations",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DeviceConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DeviceConfigurations",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Detectors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Detectors",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Detectors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Detectors",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DetectorComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DetectorComments",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DetectorComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DetectorComments",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DetectionTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DetectionTypes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "DetectionTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "DetectionTypes",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Areas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Areas",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Areas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Areas",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Approaches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Approaches",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Approaches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Approaches",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeasureOptionPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    Option = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeasureTypeId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ModifiedBy = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
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
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldUnicode: false,
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 27, 9, 29, 9, 10, DateTimeKind.Local).AddTicks(3603),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 7, 3, 6, 59, 30, 738, DateTimeKind.Local).AddTicks(4621));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Routes",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
