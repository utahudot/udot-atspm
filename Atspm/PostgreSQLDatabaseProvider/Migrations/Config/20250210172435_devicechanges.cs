using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class devicechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Firmware",
                table: "DeviceConfigurations");

            migrationBuilder.RenameColumn(
                name: "SearchTerms",
                table: "DeviceConfigurations",
                newName: "Query");

            migrationBuilder.RenameColumn(
                name: "Directory",
                table: "DeviceConfigurations",
                newName: "Path");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 10, 10, 24, 34, 63, DateTimeKind.Local).AddTicks(6320),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2024, 12, 20, 12, 28, 12, 764, DateTimeKind.Local).AddTicks(2478));

            migrationBuilder.AddColumn<string>(
                name: "DeviceIdentifier",
                table: "Devices",
                type: "character varying(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeviceProperties",
                table: "Devices",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionProperties",
                table: "DeviceConfigurations",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DeviceConfigurations",
                type: "character varying(24)",
                unicode: false,
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LoggingOffset",
                table: "DeviceConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Body", "Header" },
                values: new object[] { "There are two ways to navigate the UDOT Automated Traffic Location Performance Measures website</b><br/><br/><u>MAP</u><ol><li>Zoom in on the map and click on the desired intersection (note: the map can be filtered by selecting “metric type” ).</li><li>Select the available chart on the map from the list of available measures for the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol><u>Location LIST</u><ol><li>Select the chart by clicking the checkbox for the desired chart.</li><li>Click the “Location List” bar at the top of the map window.</li><li>Click “Select” next to the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol>", "How do I navigate the UDOT Automated Traffic Location Performance Measures website?</b>" });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 2,
                column: "Header",
                value: "What are Automated Traffic Location Performance Measures</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Header",
                value: "How do Automated Traffic Location Performance Measures work?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 4,
                column: "Header",
                value: "Which central traffic management system is used to get the Automated Traffic Location Performance Measures</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 5,
                column: "Header",
                value: "Why does Utah need Automated Traffic Location Performance Measures?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 6,
                column: "Header",
                value: "Where did you get the Automated Traffic Location Performance Measure software?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 7,
                column: "Header",
                value: "How did the Automated Traffic Location Performance Measures Begin?</b> ");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 8,
                column: "Header",
                value: "Why are there no passwords or firewalls to access the website and see the measures?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 9,
                column: "Header",
                value: "How do you use the various Location Performance Measures and what do they do?</b> ");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 10,
                column: "Header",
                value: "How effective are Automated Traffic Location Performance Measures</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 11,
                column: "Header",
                value: "Does this mean I never have to stop at a red light?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 12,
                column: "Header",
                value: "Will Automated Traffic Location Performance Measures save me money?  If so, how are cost savings measured?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 13,
                column: "Header",
                value: "How do Automated Traffic Location Performance Measures enhance safety?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 14,
                column: "Header",
                value: "Can real-time Automated Traffic Location Performance Measures be used as a law enforcement tool?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 15,
                column: "Header",
                value: "Server and Data Storage Requirements</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 16,
                column: "Header",
                value: "Who uses the Automated Traffic Location Performance Measures data?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Body", "Header" },
                values: new object[] { "<table class='table table-bordered'>\r\n 	                            <tr>\r\n                                    <th> MEASURE </th>\r\n                                    <th> DETECTION NEEDED </th>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Coordination Diagram </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Volume </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Speed </td>\r\n                                    <td> Setback count (350 ft – 400 ft) using radar </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Phase Termination </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Split Monitor </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Turning Movement Counts </td>\r\n                                    <td> Stop bar (lane-by-lane) count </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Delay </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Arrivals on Red </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Yellow and Red Actuations </td>\r\n                                    <td> Stop bar (lane-by-lane) count that is either in front of the stop bar or has a speed filter enabled </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Split Failure </td>\r\n                                    <td> Stop bar presence detection, either by lane group or individual lane </td>\r\n                                </tr>\r\n                        </table>\r\n                         Automated Traffic Location Performance Measures will work with any type of detector that is capable of counting vehicles, e.g., loops, video, pucks, radar. (The only exception to this is the speed measure, where UDOT’s Automated Location Performance Measures for speeds will only work with the Wavetronix Advance SmartSensor.) Please note that two of the measures (Purdue Phase Termination and Split Monitor) do not use detection and are extremely useful measures.</b>", "What are the detection requirements for each metric?</b> " });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 18,
                column: "Header",
                value: "Why do some intersections only show a few metrics and others have more?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Body", "Header" },
                values: new object[] { "  System Requirements:</b>\r\n                         Operating Systems and Software:</b>\r\n                        The UDOT Automated Location Performance Measures system runs on Microsoft Windows Servers.\r\n                        The web components are hosted by Microsoft Internet Information Server(IIS).\r\n                        The database server is a Microsoft SQL 2016 server.\r\n                         Storage and Processing:</b>\r\n                        Detector data uses about 40 % of the storage space of the UDOT system,\r\n                        so the number of detectors attached to a controller will have a huge impact on the amount of storage space required.Detector data is also the most important information we collect.\r\n                        We estimate that each Location will generate 19 MB of data per day.\r\n                        The amount of processing power required is highly dependant on how many Locations are on the system,\r\n                        how many servers will be part of the system,\r\n                        and how many people will be using the system.  It is possible to host all of the system functions on one powerful server, or split them out into multiple, less expensive servers.  If your agency decided to make the Automated Location Performance Measures available to the public, it might be best to have a web server separate from the database server.Much of the heavy processing for the charts is done by web services, and it is possible to host these services on a dedicated computer.\r\n                        While each agency should consult with their IT department for specific guidelines on how to best deliver a secure, stable and responsive solution, we can estimate that most mid-range to high-end servers will be able to handle the task of hosting and creating measures for most agencies.<ul>\r\n                        <li>Windows Server 2008 or newer installed</li>\r\n                        <li>.NET 4.5.2 Framework installed</li>\r\n                        <li>IIS 7 or better installed, along with ASP.NET 4.0 or later</li>\r\n                        <li>SQL Server Express, SQL Server 2008 R2, or newer installed</li>\r\n                        <li>Firewall exceptions for connections to the controllers</li>\r\n                        <li>If Watchdog features are desired, installation requires access to an SMTP (email) server. It will accept email from the Automated Location Performance Measures (ATSPM) server. The SMTP server can reside on the same machine.</li>\r\n                        <li>Microsoft Visual Studio 2013 or later is recommended</li></ul>", "What are the System Requirements?</b>" });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 20,
                column: "Header",
                value: "Who do I contact to find out more information about Automated Traffic Location Performance Measures</b> ");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 21,
                column: "Header",
                value: "How do I get the source code for the Automated Traffic Location Performance Measures Website?</b> ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceIdentifier",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceProperties",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ConnectionProperties",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "LoggingOffset",
                table: "DeviceConfigurations");

            migrationBuilder.RenameColumn(
                name: "Query",
                table: "DeviceConfigurations",
                newName: "SearchTerms");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "DeviceConfigurations",
                newName: "Directory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 20, 12, 28, 12, 764, DateTimeKind.Local).AddTicks(2478),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 2, 10, 10, 24, 34, 63, DateTimeKind.Local).AddTicks(6320));

            migrationBuilder.AddColumn<string>(
                name: "Firmware",
                table: "DeviceConfigurations",
                type: "character varying(16)",
                unicode: false,
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Body", "Header" },
                values: new object[] { "<b>There are two ways to navigate the UDOT Automated Traffic Location Performance Measures website</b><br/><br/><u>MAP</u><ol><li>Zoom in on the map and click on the desired intersection (note: the map can be filtered by selecting “metric type” ).</li><li>Select the available chart on the map from the list of available measures for the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol><u>Location LIST</u><ol><li>Select the chart by clicking the checkbox for the desired chart.</li><li>Click the “Location List” bar at the top of the map window.</li><li>Click “Select” next to the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol>", "<b>How do I navigate the UDOT Automated Traffic Location Performance Measures website?</b>" });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 2,
                column: "Header",
                value: "<b>What are Automated Traffic Location Performance Measures</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Header",
                value: "<b>How do Automated Traffic Location Performance Measures work?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 4,
                column: "Header",
                value: "<b>Which central traffic management system is used to get the Automated Traffic Location Performance Measures</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 5,
                column: "Header",
                value: "<b>Why does Utah need Automated Traffic Location Performance Measures?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 6,
                column: "Header",
                value: "<b>Where did you get the Automated Traffic Location Performance Measure software?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 7,
                column: "Header",
                value: "<b>How did the Automated Traffic Location Performance Measures Begin?</b> ");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 8,
                column: "Header",
                value: "<b>Why are there no passwords or firewalls to access the website and see the measures?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 9,
                column: "Header",
                value: "<b>How do you use the various Location Performance Measures and what do they do?</b> ");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 10,
                column: "Header",
                value: "<b>How effective are Automated Traffic Location Performance Measures</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 11,
                column: "Header",
                value: "<b>Does this mean I never have to stop at a red light?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 12,
                column: "Header",
                value: "<b>Will Automated Traffic Location Performance Measures save me money?  If so, how are cost savings measured?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 13,
                column: "Header",
                value: "<b>How do Automated Traffic Location Performance Measures enhance safety?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 14,
                column: "Header",
                value: "<b>Can real-time Automated Traffic Location Performance Measures be used as a law enforcement tool?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 15,
                column: "Header",
                value: "<b>Server and Data Storage Requirements</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 16,
                column: "Header",
                value: "<b>Who uses the Automated Traffic Location Performance Measures data?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Body", "Header" },
                values: new object[] { "<table class='table table-bordered'>\r\n 	                            <tr>\r\n                                    <th> MEASURE </th>\r\n                                    <th> DETECTION NEEDED </th>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Coordination Diagram </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Volume </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Speed </td>\r\n                                    <td> Setback count (350 ft – 400 ft) using radar </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Phase Termination </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Split Monitor </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Turning Movement Counts </td>\r\n                                    <td> Stop bar (lane-by-lane) count </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Delay </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Arrivals on Red </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Yellow and Red Actuations </td>\r\n                                    <td> Stop bar (lane-by-lane) count that is either in front of the stop bar or has a speed filter enabled </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Split Failure </td>\r\n                                    <td> Stop bar presence detection, either by lane group or individual lane </td>\r\n                                </tr>\r\n                        </table>\r\n                        <b> Automated Traffic Location Performance Measures will work with any type of detector that is capable of counting vehicles, e.g., loops, video, pucks, radar. (The only exception to this is the speed measure, where UDOT’s Automated Location Performance Measures for speeds will only work with the Wavetronix Advance SmartSensor.) Please note that two of the measures (Purdue Phase Termination and Split Monitor) do not use detection and are extremely useful measures.</b>", "<b>What are the detection requirements for each metric?</b> " });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 18,
                column: "Header",
                value: "<b>Why do some intersections only show a few metrics and others have more?</b>");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Body", "Header" },
                values: new object[] { " <b> System Requirements:</b>\r\n                        <b> Operating Systems and Software:</b>\r\n                        The UDOT Automated Location Performance Measures system runs on Microsoft Windows Servers.\r\n                        The web components are hosted by Microsoft Internet Information Server(IIS).\r\n                        The database server is a Microsoft SQL 2016 server.\r\n                        <b> Storage and Processing:</b>\r\n                        Detector data uses about 40 % of the storage space of the UDOT system,\r\n                        so the number of detectors attached to a controller will have a huge impact on the amount of storage space required.Detector data is also the most important information we collect.\r\n                        We estimate that each Location will generate 19 MB of data per day.\r\n                        The amount of processing power required is highly dependant on how many Locations are on the system,\r\n                        how many servers will be part of the system,\r\n                        and how many people will be using the system.  It is possible to host all of the system functions on one powerful server, or split them out into multiple, less expensive servers.  If your agency decided to make the Automated Location Performance Measures available to the public, it might be best to have a web server separate from the database server.Much of the heavy processing for the charts is done by web services, and it is possible to host these services on a dedicated computer.\r\n                        While each agency should consult with their IT department for specific guidelines on how to best deliver a secure, stable and responsive solution, we can estimate that most mid-range to high-end servers will be able to handle the task of hosting and creating measures for most agencies.<ul>\r\n                        <li>Windows Server 2008 or newer installed</li>\r\n                        <li>.NET 4.5.2 Framework installed</li>\r\n                        <li>IIS 7 or better installed, along with ASP.NET 4.0 or later</li>\r\n                        <li>SQL Server Express, SQL Server 2008 R2, or newer installed</li>\r\n                        <li>Firewall exceptions for connections to the controllers</li>\r\n                        <li>If Watchdog features are desired, installation requires access to an SMTP (email) server. It will accept email from the Automated Location Performance Measures (ATSPM) server. The SMTP server can reside on the same machine.</li>\r\n                        <li>Microsoft Visual Studio 2013 or later is recommended</li></ul>", "<b>What are the System Requirements?</b>" });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 20,
                column: "Header",
                value: "<b>Who do I contact to find out more information about Automated Traffic Location Performance Measures</b> ");

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 21,
                column: "Header",
                value: "<b>How do I get the source code for the Automated Traffic Location Performance Measures Website?</b> ");
        }
    }
}
