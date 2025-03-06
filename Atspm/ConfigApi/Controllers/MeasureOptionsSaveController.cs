#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/MeasureOptionController.cs
#endregion

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using System.Text.Json;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.ConfigApi.Models;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{
    /// <summary>
    /// Measure options Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class MeasureOptionsSaveController : AtspmConfigControllerBase<MeasureOptionsSave, int>
    {
        private readonly IMeasureOptionsSaveRepository _repository;

        /// <inheritdoc/>
        public MeasureOptionsSaveController(IMeasureOptionsSaveRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region Functions

        /// <summary>
        /// Retrieves all saved measure options
        /// </summary>
        /// <returns>List of measure options</returns>
        [HttpGet("api/v1/MeasureOptionsSave/GetSavedOptions")]  // Explicit route for Swagger visibility
        [EnableQuery]
        public IActionResult GetSavedOptions()
        {
            var data = _repository
                .GetList()
                .Select(m => MeasureOptionsSaveDto.FromEntity(m));

            return Ok(data);
        }

        [HttpPost("PostSavedOptions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PostSavedOptions([FromBody] MeasureOptionsSave measureOptions)
        {
            if (measureOptions == null)
            {
                return BadRequest("The request body is required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Deserialize JSON based on MeasureTypeId
                var deserializedObject = MeasureOptionDeserializer.Deserialize(measureOptions.SelectedParametersJson, measureOptions.MeasureTypeId);

                if (deserializedObject == null)
                {
                    return BadRequest("Invalid JSON format or MeasureTypeId not supported.");
                }
            }
            catch (JsonException)
            {
                return BadRequest("Invalid JSON structure. Ensure the data is properly formatted.");
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ex.Message);
            }

            _repository.Add(measureOptions);
            return Ok(measureOptions);
        }





        /// <summary>
        /// Updates an existing measure option
        /// </summary>
        /// <param name="key">ID of measure option</param>
        /// <param name="dto">Updated data</param>
        /// <returns>Updated measure option</returns>
        [HttpPatch("api/v1/PatchSavedOptions/{key}")]  // Explicit route with parameter
        public IActionResult PatchSavedOptions([FromRoute] int key, [FromBody] MeasureOptionsSave dto)
        {
            var measureOption = _repository.GetList().FirstOrDefault(m => m.Id == key);
            if (measureOption == null)
            {
                return NotFound();
            }

            measureOption.Name = dto.Name;
            measureOption.SelectedParametersJson = dto.SelectedParametersJson;
            _repository.Update(measureOption);

            return Ok(dto);
        }

        #endregion
    }
}
