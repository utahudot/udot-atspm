using System;
using System.Text.Json;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Interfaces;

namespace Utah.Udot.ATSPM.ConfigApi.Models
{
    public class MeasureOptionsSaveDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Measure option name
        /// </summary>
        public string Name { get; set; }

        public string CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedByUserId { get; set; }
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// Related MeasureType
        /// </summary>
        public int MeasureTypeId { get; set; }

        /// <summary>
        /// JSON representation of Selected Parameters (Stored in DB)
        /// </summary>
        public JsonDocument SelectedParametersJson { get; set; }

        /// <summary>
        /// Deserialized Selected Parameters (Used in Application Layer)
        /// </summary>
        //public object SelectedParameters
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(SelectedParametersJson))
        //            return null;

        //        // Use MeasureOptionDeserializer to handle different measure types
        //        return MeasureOptionDeserializer.Deserialize(SelectedParametersJson, MeasureTypeId);
        //    }
        //    set
        //    {
        //        // Ensure JSON storage is updated when setting the value
        //        SelectedParametersJson = value == null ? null : MeasureOptionDeserializer.Serialize(value);
        //    }
        //}


        /// <summary>
        /// Converts DTO to Entity Model
        /// </summary>
        //public MeasureOptionsSave ToEntity()
        //{
        //    return new MeasureOptionsSave
        //    {
        //        Id = this.Id,
        //        Name = this.Name,
        //        CreatedByUserId = this.CreatedByUserId,
        //        CreatedOn = this.CreatedOn,
        //        ModifiedByUserId = this.ModifiedByUserId,
        //        ModifiedOn = this.ModifiedOn,
        //        MeasureTypeId = this.MeasureTypeId,
        //        SelectedParametersJson = this.SelectedParametersJson // Store JSON in DB
        //    };
        //}

        /// <summary>
        /// Converts Entity Model to DTO
        /// </summary>
        //public static MeasureOptionsSaveDto FromEntity(MeasureOptionsSave entity)
        //{
        //    return new MeasureOptionsSaveDto
        //    {
        //        Id = entity.Id,
        //        Name = entity.Name,
        //        CreatedByUserId = entity.CreatedByUserId,
        //        CreatedOn = entity.CreatedOn,
        //        ModifiedByUserId = entity.ModifiedByUserId,
        //        ModifiedOn = entity.ModifiedOn,
        //        MeasureTypeId = entity.MeasureTypeId,
        //        SelectedParametersJson = entity.SelectedParametersJson // Retrieve JSON from DB
        //    };
        //}
    }
}
