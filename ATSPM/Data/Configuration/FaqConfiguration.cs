#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Configuration/FaqConfiguration.cs
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
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// Frequently asked questions configuration
    public class FaqConfiguration : IEntityTypeConfiguration<Faq>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Faq> builder)
        {
            builder.HasComment("Frequently Asked Questions");

            builder.Property(e => e.Header)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Body)
                .IsRequired()
                .HasMaxLength(8000);

            builder.HasData(
                new Faq
                {
                    Id = 1,
                    DisplayOrder = 1,
                    Header = @"<b>How do I navigate the UDOT Automated Traffic Location Performance Measures website?</b>",
                    Body = @"<b>There are two ways to navigate the UDOT Automated Traffic Location Performance Measures website</b><br/><br/><u>MAP</u><ol><li>Zoom in on the map and click on the desired intersection (note: the map can be filtered by selecting “metric type” ).</li><li>Select the available chart on the map from the list of available measures for the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol><u>Location LIST</u><ol><li>Select the chart by clicking the checkbox for the desired chart.</li><li>Click the “Location List” bar at the top of the map window.</li><li>Click “Select” next to the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol>"
                },
                new Faq
                {
                    Id = 2,
                    DisplayOrder = 2,
                    Header = @"<b>What are Automated Traffic Location Performance Measures</b>",
                    Body = @"Automated Traffic Location Performance Measures show real-time and a history of performance at Locationized intersections.  The various measures will evaluate the quality of progression of traffic along the corridor, and displays any unused green time that may be available from various movements. This information informs UDOT of vehicle and pedestrian detector malfunctions, measures vehicle delay and lets us know volumes, speeds and travel time of vehicles.   The measures are used to optimize mobility and manage traffic Location timing and maintenance to reduce congestion, save fuel costs and improve safety.  There are several measures currently in use with others in development. "
                },
                new Faq
                {
                    Id = 3,
                    DisplayOrder = 3,
                    Header = @"<b>How do Automated Traffic Location Performance Measures work?</b>",
                    Body = @"The traffic Location controller manufactures (Econolite, Intelight, Siemens, McCain, TrafficWare and some others) wrote a “data-logger” program that runs in the background of the traffic Location controller firmware. The Indiana Traffic Location Hi Resolution Data Logger Enumerations (http://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata) encode events to a resolution to the nearest 100 milliseconds.  The recorded enumerations will have events for “phase begin green”, “phase gap out”, “phase max out”, “phase begin yellow clearance”, “phase end yellow clearance”, “pedestrian begin walk”, “pedestrian begin clearance”, “detector off”, “detector on”, etc.  For each event, a time-stamp is given and the event is stored temporarily in the Location controller.  Over 125 various enumerations are currently in use.  Then, using an FTP connection from a remote server to the traffic Location controller, packets of the hi resolution data logger enumerations (with its 1/10th second resolution time-stamp) are retrieved and stored on a web server at the UDOT Traffic Operations Center about every 10 to 15 minutes (unless the “upload current data” checkbox is enabled, where an FTP connection will be immediately made and the data will be displayed in real-time).  Software was written in-house by UDOT that allows us to graph and display the various data-logger enumerations and to show the results on the UDOT Automated Traffic Location Performance Measures website."
                },
                new Faq
                {
                    Id = 4,
                    DisplayOrder = 4,
                    Header = @"<b>Which central traffic management system is used to get the Automated Traffic Location Performance Measures</b>",
                    Body = @"A central traffic management system is not used or needed for the UDOT Automated Traffic Location Performance Measures.  It is all being done through FTP connections from a web server through the network directly to the traffic Location controller which currently has the Indiana Traffic Location Hi Resolution Data Logger Enumerations running in the background of the controller firmware.  The UDOT Automated Traffic Location Performance Measures are independent of any central traffic management system.
"
                },
                new Faq
                {
                    Id = 5,
                    DisplayOrder = 5,
                    Header = @"<b>Why does Utah need Automated Traffic Location Performance Measures?</b>",
                    Body = @"In 2011, UDOT’s executive director assigned a Quality Improvement Team (QIT) to make recommendations that will result in UDOT providing “world-class traffic Location maintenance and operations”.  The QIT evaluated existing operations, national best practices, and NCHRP recommendations to better improve UDOT’s Location maintenance and operations practices.  One of the recommendations from the QIT was to “implement real-time monitoring of system health and quality of operations”.  The real-time Automated Location Performance Measures allow us to greatly improve the quality of Location operations and to also know when equipment such as pedestrian detection or vehicle detection is not working properly.  We are simply able to do more with less and to manage traffic more effectively 24/7.  In addition, we are able to optimize intersections and corridors when they need to be re-optimized, instead of on a set schedule."
                },
                new Faq
                {
                    Id = 6,
                    DisplayOrder = 6,
                    Header = @"<b>Where did you get the Automated Traffic Location Performance Measure software?</b>",
                    Body = @"The UDOT Automated Traffic Location Measures software was developed in-house at UDOT by the Department of Technology Services.  Purdue University and the Indiana Department of Transportation (INDOT) assisted in getting us started on this endeavor."
                },
                new Faq
                {
                    Id = 7,
                    DisplayOrder = 7,
                    Header = @"<b>How did the Automated Traffic Location Performance Measures Begin?</b> ",
                    Body = @"The Purdue coordination diagram concept was introduced in 2009 by Purdue University to visualize the temporal relationship between the coordinated phase indications and vehicle arrivals on a cycle-by-cycle basis.  The Indiana Traffic Location HI Resolution Data Logger Enumerations (http://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata) was a joint transportation research program (updated in November 2012 but started earlier) that included people from Indiana DOT, Purdue University, Econolite, PEEK, and Siemens.  <br/><br/>After discussions with Dr. Darcy Bullock from Purdue University and INDOT’s James Sturdevant, UDOT started development of the UDOT Automated Location Performance Measures website April of 2012."
                },
                new Faq
                {
                    Id = 8,
                    DisplayOrder = 8,
                    Header = @"<b>Why are there no passwords or firewalls to access the website and see the measures?</b>",
                    Body = @"UDOT’s goal is transparency and unrestricted access to all who have a desire for traffic Location data.  Our goal in optimizing mobility, improving safety, preserving infrastructure and strengthening the economy means that all who have a desire to use the data should have access to the data without restrictions.  This includes all of UDOT (various divisions and groups), consultants, academia, MPO’s, other jurisdictions, FHWA, the public, and others.  It is also UDOT’s goal to be the most transparent Department of Transportation in the country.  Having a website where real-time Automated Location Performance Measures can be obtained without special software, passwords or restricted firewalls will help UDOT in achieving the goal of transparency, and allows everyone access to the data without any silos."
                },
                new Faq
                {
                    Id = 9,
                    DisplayOrder = 9,
                    Header = @"<b>How do you use the various Location Performance Measures and what do they do?</b> ",
                    Body = @"There are many uses and benefits of the various measures.  Some of the key uses are:<br/><br/><u>Purdue Coordination Diagrams (PCD’s)</u> – Used to visualize the temporal relationship between the coordinated phase indications and vehicle arrivals on a cycle-by-cycle basis.  The PCD’s show the progression quality along the corridor and answer questions, such as “What percent of vehicles are arriving during the green?” or “What is the platoon ratio during various coordination patterns?” The PCD’s are instrumental in optimizing offsets, identifying oversaturated or under saturated splits for the coordinated phases, the effects of early return of green and coordinated phase actuations, impacts of queuing, adjacent Location synchronization, etc.<br/> <br/>In reading the PCD’s, between the green and yellow lines, the phase is green; between the yellow and red lines, the phase is yellow; and underneath the green line, the phase is red.  The long vertical red lines during the late night hours is showing the main street sitting in green as the side streets and left turns are being skipped.  The short vertical red lines show skipping of the side street / left turns or a late start of green for the side street or left turn.  AoG is the percent of vehicles arriving during the green phase.  GT is the percent of green split time for the phase and PR is the Platoon Ratio (Equation 15-4 from the 2000 Highway Capacity Manual).<br/><br/><u>Approach Volumes</u> – Counts the approach vehicle volumes as shown arriving upstream of the intersection about 350 ft – 400 ft.  The detection zones are in advance of the turning lanes, so the approach volumes don’t know if the vehicles are going straight through, turning left or right.  The accuracy of the approach volumes tends to undercount under heavy traffic and under multi-lane facilities.  Approach volumes are used in traffic models, as well as identifying directional splits in traffic. In addition, the measure is also used in evaluating the least disruptive time to allow lanes to be taken for maintenance and construction activities.<br/><br/><u/>Approach Speeds</u> – The speeds are obtained from the Wavetronix radar Advance Smartsensor.  As vehicles cross the 10-foot wide detector in advance of the intersection (350 ft – 400 ft upstream of the stop bar), the speed is captured, recorded, and time-stamped.  In graphing the results, a time filter is used that starts 15 seconds (user defined) after the initial green to the start of the yellow.  The time filter allows for free-flow speed conditions to be displayed that are independent of the traffic Location timings.  The approach speed measure is beneficial in knowing the approach speeds to use for modeling purposes – both for normal time-of-day coordination plans and for adverse weather or special event plans.  They are also beneficial in knowing when speed conditions degrade enough to warrant a change in time-of-day coordination plans to adverse weather or special event plans.  In addition, the speed data is used to set yellow and all-red intervals for Location timing, as well as for various speed studies.<br/><br/><u>Purdue Phase Termination Charts</u> – Shows how each phase terminates when it changes from green to red.  The measure will show if the termination occurred by a gapout, a maxout / forceoff, or skip.  A gapout means that not all of the programmed time was used.  A maxout occurs during fully actuated (free) operations, while forceoff’s occur during coordination.  Both a maxout and forceoff shows that all the programmed time was used. A skip means that the phase was not active and did not turn on.  In addition, the termination can be evaluated by consecutive occurrences in a approach.  For example, you can evaluate if three (range is between 1 and 5) gapouts or skips occurred in a approach.  This feature is helpful in areas where traffic volume fluctuations are high.  Also shown are the pedestrian activations for each phase.  What this measure does not show is the amount of gapout time remaining if a gapout occurred.  The Split Monitor measure is used to answer that question.<br/><br/>This measure is used to identify movements where split time may need to be taken from some phases and given to other phases.  Also, this measure is very useful in identifying problems with vehicle and pedestrian detection.  For example, if the phase is showing constant maxouts all night long, it is assumed that there is a detection problem.<br/><br/><u>Split Monitor</u> – This measure shows the amount of split time (green, yellow and all-red) used by the various phases at the intersection.  Greens show gapouts, reds show maxouts, blues show forceoffs and yellows show pedestrian activations.  This measure is useful to know the amount of split time each phase uses.Turning Movement Volume Counts – this measure shows the lane-by-lane vehicles per hour (vph) and total volume for each movement.  Three graphs are available for each approach (left, thru, right).  Also shown for each movement are the total volume, peak hour, peak hour factor and lane utilization factor.  The lane-by-lane volume counts are used for traffic models and traffic studies.<br/><br/><u>Approach Delay</u> – This measure shows a simplified approach delay by displaying the time between detector activations during the red phase and when the phase turns green for the coordinated movements.  This measure does not account for start-up delay, deceleration, or queue length that exceeds the detection zone.  This measure is beneficial in evaluating over time the delay per vehicle and delay per hour values for each coordinated approach.<br/><br/><u>Arrivals on Red</u> – This measure shows the percent of vehicles arriving on red (inverse of % vehicles arriving on green) and the percent red time for each coordination pattern.  The Y axis is graphing the volume (vph) and the secondary Y axis graphs the percent vehicles arriving on red.  This measure is useful in identifying areas where the progression quality is poor.<br/><br/><u>Yellow and Red Actuations</u> – This measure plots vehicle arrivals during the yellow and red portions of an intersection's movements where the speed of the vehicle is interpreted to be too fast to stop before entering the intersection. It provides users with a visual indication of occurrences, violations, and several related statistics. The purpose of this chart is to identify engineering countermeasures to deal with red light running.<br/><br/><u>Purdue Split Failure</u> – This measure calculates the percent of time that stop bar detectors are occupied during the green phase and then during the first five seconds of red. This measure is a good indication that at least one vehicle did not clear during the green."
                },
                new Faq
                {
                    Id = 10,
                    DisplayOrder = 10,
                    Header = @"<b>How effective are Automated Traffic Location Performance Measures</b>",
                    Body = @"The Automated Location Performance Measures are an effective way to reduce congestion, save fuel costs and improve safety.  We are simply able to do more with less and are more effectively able to manage traffic every day of the week and at all times of the day, even when a traffic Location engineer is not available.  We have identified several detection problems, corrected time-of-day coordination errors in the traffic Location controller scheduler, corrected offsets, splits, among other things.  In addition, we have been able to use more accurate data in optimizing models and doing traffic studies, and have been able to more correctly set various Location timing parameters."
                },
                new Faq
                {
                    Id = 11,
                    DisplayOrder = 11,
                    Header = @"<b>Does this mean I never have to stop at a red light?</b>",
                    Body = @"Although the UDOTAutomated Traffic Location Performance Measures cannot guarantee you will only get green lights, the system does help make traveling through Utah more efficient.  UDOT Automatic Location Performance Measures have already already helped to reduce the number of stops and delay at Locationized intersections.  Continued benefits are anticipated."
                },
                new Faq
                {
                    Id = 12,
                    DisplayOrder = 12,
                    Header = @"<b>Will Automated Traffic Location Performance Measures save me money?  If so, how are cost savings measured?</b>",
                    Body = @"Yes, UDOT Automated Traffic Location Performance Measures has already saved Utahans time and money.  By increasing corridor speeds while reducing intersection delays, traffic Location stops, and the ability to monitor operations 24/7."
                },
                new Faq
                {
                    Id = 13,
                    DisplayOrder = 13,
                    Header = @"<b>How do Automated Traffic Location Performance Measures enhance safety?</b>",
                    Body = @"By reducing congestion and reducing the percent of vehicles arriving on a red light, UDOT Automated Traffic Location Performance Measures helps decrease the number of accidents that occur.  In addition, we are better able to manage detector failures and improve the duration of the change intervals and clearance times at intersections."
                },
                new Faq
                {
                    Id = 14,
                    DisplayOrder = 14,
                    Header = @"<b>Can real-time Automated Traffic Location Performance Measures be used as a law enforcement tool?</b>",
                    Body = @"UDOT Automated Traffic Location Performance Measures are designed to increase the safety and efficiency at Locationized intersections.  It is not intended to identify speeders or enforce traffic laws.  No personal information is recorded or used in data gathering."
                },
                new Faq
                {
                    Id = 15,
                    DisplayOrder = 15,
                    Header = @"<b>Server and Data Storage Requirements</b>",
                    Body = @"We can estimate that each Location controller high resolution data requires approximately 19 MB of storage space each day.  For the UDOT system, we have approximately 2040 traffic Locations bringing back about 1 TB of data per month. In addition to the high resolution data, version 4.2.0 and above also allows for the data to be rolled up into aggregated tables in 15-minute bins. UDOT averages approximately 6 GB of aggregated tables per month. UDOT uses a SAN server that holds approximately 40 TB that runs SQL 2016. Our goal is to keep between 24 months and 36 months of high resolution data files and then to cold storage the old high resolution  data files for up to five years after that. The cold storage flat files (comma deliminated file with no indexing) will require about 2 TB of storage per year. UDOT plans on keeping the aggregated tables in 15-minute bins indefinitely. "
                },
                new Faq
                {
                    Id = 16,
                    DisplayOrder = 16,
                    Header = @"<b>Who uses the Automated Traffic Location Performance Measures data?</b>",
                    Body = @"The data has been useful for some of the following users in Utah:<br/><br/><ul><li><u>Location engineers</u> in optimize and fine-tuning Location timing.</li><li><u>Maintenance Location technicians</u> in identifying broken detector problems and investigating trouble calls.</li><li><u>Traffic engineers</u> in conducting various traffic studies, such as speed studies, turning movement studies, modeling studies, and optimizing the intersection operations.</li><li><u>Consultants</u> in improving traffic Location operations, as UDOT outsources some of the Location operations, design and planning to consultants.</li><li><u>UDOT Traffic & Safety, UDOT Traffic Engineers, UDOT Resident Construction Engineers</u> in conducting various traffic studies and/or in determining the time-of-day where construction or maintenance activities would be least disruptive to the traveling motorists.</li><li><u>Metropolitan Planning Organizations</u> (MPO’s) in calibrating the regional traffic models.</li><li><u>Academia</u> in conducting various research studies, such as evaluating the effectiveness of operations during adverse weather, evaluating the optimum Location timing for innovative intersections such as DDI’s, CFI’s and Thru-Turns, etc.</li><li><u>City and County</u> Government in using the data in similar manner to UDOT.</li></ul>"
                },
                new Faq
                {
                    Id = 17,
                    DisplayOrder = 17,
                    Header = @"<b>What are the detection requirements for each metric?</b> ",
                    Body = @"<table class='table table-bordered'>
 	                            <tr>
                                    <th> MEASURE </th>
                                    <th> DETECTION NEEDED </th>
                                </tr>
                                <tr>
                                    <td> Purdue Coordination Diagram </td>
                                    <td> Setback count (350 ft – 400 ft) </td>
                                </tr>
                                <tr>
                                    <td> Approach Volume </td>
                                    <td> Setback count (350 ft – 400 ft) </td>
                                </tr>
                                <tr>
                                    <td> Approach Speed </td>
                                    <td> Setback count (350 ft – 400 ft) using radar </td>
                                </tr>
                                <tr>
                                    <td> Purdue Phase Termination </td>
                                    <td> No detection needed or used </td>
                                </tr>
                                <tr>
                                    <td> Split Monitor </td>
                                    <td> No detection needed or used </td>
                                </tr>
                                <tr>
                                    <td> Turning Movement Counts </td>
                                    <td> Stop bar (lane-by-lane) count </td>
                                </tr>
                                <tr>
                                    <td> Approach Delay </td>
                                    <td> Setback count (350 ft – 400 ft) </td>
                                </tr>
                                <tr>
                                    <td> Arrivals on Red </td>
                                    <td> Setback count (350 ft – 400 ft) </td>
                                </tr>
                                <tr>
                                    <td> Yellow and Red Actuations </td>
                                    <td> Stop bar (lane-by-lane) count that is either in front of the stop bar or has a speed filter enabled </td>
                                </tr>
                                <tr>
                                    <td> Purdue Split Failure </td>
                                    <td> Stop bar presence detection, either by lane group or individual lane </td>
                                </tr>
                        </table>
                        <b> Automated Traffic Location Performance Measures will work with any type of detector that is capable of counting vehicles, e.g., loops, video, pucks, radar. (The only exception to this is the speed measure, where UDOT’s Automated Location Performance Measures for speeds will only work with the Wavetronix Advance SmartSensor.) Please note that two of the measures (Purdue Phase Termination and Split Monitor) do not use detection and are extremely useful measures.</b>"
                },
                new Faq
                {
                    Id = 18,
                    DisplayOrder = 18,
                    Header = @"<b>Why do some intersections only show a few metrics and others have more?</b>",
                    Body = @"Some measures have different detection requirements than other measures. For example, for approach speeds, UDOT uses the Wavetronix Advance Smartsensor radar detector and has been using this detector since 2006 for dilemma zone purposes if the design speed is 40 mph or higher.  This same detector is what we use for our setback counts 350 feet – 400 feet upstream of the intersection.  In addition, we are also able to pull the raw radar speeds from the sensor back to the TOC server for the speed data.  Not all intersections have the Wavetronix Advance Smartsensors, therefore we are not able to display speed data, as well as the PCD’s, approach volume, arrivals on red or approach delay at each intersection.<br/><br/>The turning movement count measure requires lane-by-lane detection capable of counting vehicles in each lane.  Configuring the detection for lane-by-lane counts is time consuming and takes a commitment to financial resources."
                },
                new Faq
                {
                    Id = 19,
                    DisplayOrder = 19,
                    Header = @"<b>What are the System Requirements?</b>",
                    Body = @" <b> System Requirements:</b>
                        <b> Operating Systems and Software:</b>
                        The UDOT Automated Location Performance Measures system runs on Microsoft Windows Servers.
                        The web components are hosted by Microsoft Internet Information Server(IIS).
                        The database server is a Microsoft SQL 2016 server.
                        <b> Storage and Processing:</b>
                        Detector data uses about 40 % of the storage space of the UDOT system,
                        so the number of detectors attached to a controller will have a huge impact on the amount of storage space required.Detector data is also the most important information we collect.
                        We estimate that each Location will generate 19 MB of data per day.
                        The amount of processing power required is highly dependant on how many Locations are on the system,
                        how many servers will be part of the system,
                        and how many people will be using the system.  It is possible to host all of the system functions on one powerful server, or split them out into multiple, less expensive servers.  If your agency decided to make the Automated Location Performance Measures available to the public, it might be best to have a web server separate from the database server.Much of the heavy processing for the charts is done by web services, and it is possible to host these services on a dedicated computer.
                        While each agency should consult with their IT department for specific guidelines on how to best deliver a secure, stable and responsive solution, we can estimate that most mid-range to high-end servers will be able to handle the task of hosting and creating measures for most agencies.<ul>
                        <li>Windows Server 2008 or newer installed</li>
                        <li>.NET 4.5.2 Framework installed</li>
                        <li>IIS 7 or better installed, along with ASP.NET 4.0 or later</li>
                        <li>SQL Server Express, SQL Server 2008 R2, or newer installed</li>
                        <li>Firewall exceptions for connections to the controllers</li>
                        <li>If Watchdog features are desired, installation requires access to an SMTP (email) server. It will accept email from the Automated Location Performance Measures (ATSPM) server. The SMTP server can reside on the same machine.</li>
                        <li>Microsoft Visual Studio 2013 or later is recommended</li></ul>"
                },
                new Faq
                {
                    Id = 20,
                    DisplayOrder = 20,
                    Header = @"<b>Who do I contact to find out more information about Automated Traffic Location Performance Measures</b> ",
                    Body = @"You can contact UDOT’s Traffic Location Operations Engineer, Mark Taylor at marktaylor@utah.gov or phone at 801-887-3714 to find out more information about Automated Location Performance Measures."
                },
                new Faq
                {
                    Id = 21,
                    DisplayOrder = 21,
                    Header = @"<b>How do I get the source code for the Automated Traffic Location Performance Measures Website?</b> ",
                    Body = @"You can download the source code at GitHub at: https://github.com/udotdevelopment/ATSPM. GitHub is more for development and those interested in further developing and modifying the code. We encourage developers to contribute the enhancements back to GitHub so others can benefit as well.  For those interested in the executable ATSPM files, those are found on the FHWA's open source portal at: https://www.itsforge.net/index.php/community/explore-applications#/30."
                }

                );
        }
    }
}
