#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Configuration/LocationOdataConfiguration.cs
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

using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.ConfigApi.Models;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.ValueObjects;

namespace Utah.Udot.Atspm.ConfigApi.Configuration
{
    /// <summary>
    /// Location oData configuration
    /// </summary>
    public class LocationOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix)
        {
            var model = builder.EntitySet<Location>("Location").EntityType;
            model.Page(default, default);
            model.Expand(1, SelectExpandType.Automatic, new string[] { "jurisdiction", "region" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Latitude).IsRequired();
                        model.Property(p => p.Longitude).IsRequired();
                        model.Property(p => p.Note).IsRequired();
                        model.Property(p => p.PrimaryName).IsRequired();
                        model.Property(p => p.LocationIdentifier).IsRequired();

                        model.Property(p => p.PrimaryName).MaxLength = 100;
                        model.Property(p => p.SecondaryName).MaxLength = 100;
                        model.Property(p => p.LocationIdentifier).MaxLength = 10;
                        model.Property(p => p.Note).MaxLength = 256;

                        model.Property(p => p.JurisdictionId).DefaultValueString = "0";
                        //model.EnumProperty(p => p.VersionAction).DefaultValueString = "10";
                        model.Property(p => p.Note).DefaultValueString = "Initial";

                        var a = model.Collection.Function("GetAllVersionsOfLocation");
                        a.Parameter<string>("identifier");
                        a.ReturnsCollectionFromEntitySet<Location>("Locations");

                        var b = model.Collection.Function("GetLatestVersionOfAllLocations");
                        b.ReturnsCollectionFromEntitySet<Location>("Locations");

                        var c = model.Collection.Function("GetLatestVersionOfLocation");
                        c.Parameter<string>("identifier");
                        c.ReturnsFromEntitySet<Location>("Location");

                        var d = model.Action("CopyLocationToNewVersion");
                        d.ReturnsFromEntitySet<Location>("Location");

                        var e = model.Action("SetLocationToDeleted");

                        var f = model.Collection.Function("GetLocationsForSearch");
                        //f.Parameter<int>("areaId");
                        //f.Parameter<int>("regionId");
                        //f.Parameter<int>("jurisdictionId");
                        //f.Parameter<int>("metricTypeId");
                        f.ReturnsCollectionFromEntitySet<SearchLocation>("SearchLocations");

                        var g = model.Collection.Function("GetDetectionTypeCount");
                        g.ReturnsCollectionFromEntitySet<DetectionTypeGroup>("DetectionTypeGroups");


                        var h = model.Action("SaveTemplatedLocation").ReturnsFromEntitySet<Location>("Location"); ;
                        //var h = model.Collection.Function("SaveTemplatedLocation");
                        //h.Parameter<TemplateLocationDto>("templateLocationDto");
                        h.Parameter<string>("locationIdentifier");
                        h.Parameter<double>("latitude");
                        h.Parameter<double>("longitude");
                        h.Parameter<string>("primaryName");
                        h.Parameter<string>("secondaryName");
                        h.Parameter<string>("note");
                        h.CollectionParameter<Device>("devices");                        //h.ReturnsFromEntitySet<Location>("Location");

                        //var templateLocationModifiedDto = builder.EntitySet<TemplateLocationModifiedDto>("TemplateLocationModifiedDto").EntityType;
                        //templateLocationModifiedDto.HasKey(dto => dto.Id);

                        //// Configure Location property and enable automatic expansion
                        //templateLocationModifiedDto.ComplexProperty(dto => dto.Location)
                        //    .Expand(1, SelectExpandType.Automatic);
                        //templateLocationModifiedDto.ComplexProperty(dto => dto.RemovedApproaches)
                        //    .Expand(1, SelectExpandType.Automatic);

                        //// Configure LocationDto and its nested properties
                        //var locationType = builder.ComplexType<LocationDto>();
                        //locationType.Property(loc => loc.Id);
                        //locationType.Property(loc => loc.Latitude);
                        //locationType.Property(loc => loc.Longitude);
                        //locationType.Property(loc => loc.PrimaryName);
                        //locationType.Property(loc => loc.SecondaryName);
                        //locationType.Property(loc => loc.ChartEnabled);
                        //locationType.Property(loc => loc.Note);
                        //locationType.Property(loc => loc.Start);
                        //locationType.Property(loc => loc.PedsAre1to1);
                        //locationType.Property(loc => loc.LocationIdentifier);
                        //locationType.Property(loc => loc.JurisdictionId);
                        //locationType.ComplexProperty(loc => loc.Jurisdiction)
                        //    .Expand(1, SelectExpandType.Automatic);
                        //locationType.Property(loc => loc.LocationTypeId);
                        //locationType.ComplexProperty(loc => loc.LocationType)
                        //    .Expand(1, SelectExpandType.Automatic);
                        //locationType.Property(loc => loc.RegionId);
                        //locationType.ComplexProperty(loc => loc.Region)
                        //    .Expand(1, SelectExpandType.Automatic);

                        //// Configure collections
                        //locationType.CollectionProperty(loc => loc.Approaches)
                        //    .Expand(1, SelectExpandType.Automatic);
                        //locationType.CollectionProperty(loc => loc.Areas)
                        //    .Expand(1, SelectExpandType.Automatic);
                        //locationType.CollectionProperty(loc => loc.Devices)
                        //    .Expand(1, SelectExpandType.Automatic);

                        //// Configure nested complex types
                        //var jurisdictionType = builder.ComplexType<JurisdictionDto>();
                        //jurisdictionType.Property(j => j.Name); // Example property, customize based on JurisdictionDto

                        //var regionType = builder.ComplexType<RegionDto>();
                        //regionType.Property(r => r.Id); // Example property, customize based on RegionDto

                        //var locationTypeType = builder.ComplexType<LocationTypeDto>();
                        //locationTypeType.Property(lt => lt.Id);

                        //var approachType = builder.ComplexType<ApproachDto>();
                        //approachType.Property(a => a.Id);
                        //approachType.Property(a => a.Description);
                        //approachType.Property(a => a.Mph);
                        //approachType.Property(a => a.ProtectedPhaseNumber);
                        //approachType.Property(a => a.IsProtectedPhaseOverlap);
                        //approachType.Property(a => a.PermissivePhaseNumber);
                        //approachType.Property(a => a.IsPermissivePhaseOverlap);
                        //approachType.Property(a => a.PedestrianPhaseNumber);
                        //approachType.Property(a => a.IsPedestrianPhaseOverlap);
                        //approachType.Property(a => a.PedestrianDetectors);
                        //approachType.Property(a => a.LocationId);
                        ////approachType.Property(a => a.DirectionTypeId);

                        //// Configure the Detectors collection for ApproachDto
                        //approachType.CollectionProperty(a => a.Detectors)
                        //    .Expand(1, SelectExpandType.Automatic);

                        //// Configure DetectorDto and its nested properties
                        //var detectorType = builder.ComplexType<DetectorDto>();
                        //detectorType.Property(d => d.Id);
                        //detectorType.Property(d => d.DectectorIdentifier);
                        //detectorType.Property(d => d.DetectorChannel);
                        //detectorType.Property(d => d.DistanceFromStopBar);
                        //detectorType.Property(d => d.MinSpeedFilter);
                        //detectorType.Property(d => d.DateAdded);
                        //detectorType.Property(d => d.DateDisabled);
                        //detectorType.Property(d => d.LaneNumber);
                        ////detectorType.Property(d => d.MovementType);
                        ////detectorType.Property(d => d.LaneType);
                        ////detectorType.Property(d => d.DetectionHardware);
                        //detectorType.Property(d => d.DecisionPoint);
                        //detectorType.Property(d => d.MovementDelay);
                        //detectorType.Property(d => d.LatencyCorrection);
                        //detectorType.Property(d => d.ApproachId);

                        //// Configure the DetectionTypes collection for DetectorDto
                        //detectorType.CollectionProperty(d => d.DetectionTypes)
                        //    .Expand(1, SelectExpandType.Automatic);

                        //// Configure DetectionTypeDto and its nested properties
                        //var detectionType = builder.ComplexType<DetectionTypeDto>();
                        ////detectionType.Property(d => d.Id);
                        //detectionType.Property(d => d.Description);
                        //detectionType.Property(d => d.Abbreviation);
                        //detectionType.Property(d => d.DisplayOrder);

                        //// Configure the MeasureTypes collection for DetectionTypeDto
                        //detectionType.CollectionProperty(d => d.MeasureTypes)
                        //    .Expand(1, SelectExpandType.Automatic);

                        //// Configure MeasureTypeDto and its nested properties
                        //var measureType = builder.ComplexType<MeasureTypeDto>();
                        //measureType.Property(m => m.Id);
                        //measureType.Property(m => m.Name);
                        //measureType.Property(m => m.Abbreviation);
                        //measureType.Property(m => m.ShowOnWebsite);
                        //measureType.Property(m => m.ShowOnAggregationSite);
                        //measureType.Property(m => m.DisplayOrder);

                        //// Configure collections for MeasureTypeDto
                        //measureType.CollectionProperty(m => m.MeasureComments)
                        //    .Expand(1, SelectExpandType.Automatic);
                        //measureType.CollectionProperty(m => m.MeasureOptions)
                        //    .Expand(1, SelectExpandType.Automatic);

                        //// Configure MeasureCommentsDto
                        //var measureCommentType = builder.ComplexType<MeasureCommentsDto>();
                        //measureCommentType.Property(mc => mc.Id);
                        //measureCommentType.Property(mc => mc.TimeStamp);
                        //measureCommentType.Property(mc => mc.Comment);
                        //measureCommentType.Property(mc => mc.LocationIdentifier);

                        //// Configure MeasureOptionDto
                        //var measureOptionType = builder.ComplexType<MeasureOptionDto>();
                        //measureOptionType.Property(mo => mo.Id);
                        //measureOptionType.Property(mo => mo.Option);
                        //measureOptionType.Property(mo => mo.Value);
                        //measureOptionType.Property(mo => mo.MeasureTypeId);

                        var i = model.Action("SyncLocation").ReturnsFromEntitySet<TemplateLocationModifiedDto>("templateLocationModifiedDto");

                        var j = model.Action("DeleteAllVersions");

                        var detectionTypeGroup = builder.EntitySet<DetectionTypeGroup>("DetectionTypeGroups").EntityType;
                        detectionTypeGroup.Property(d => d.Id).IsRequired();
                        detectionTypeGroup.Property(d => d.Count).IsRequired();

                        break;
                    }
            }
        }
    }
}
