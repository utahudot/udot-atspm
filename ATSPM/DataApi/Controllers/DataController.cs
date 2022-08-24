using Microsoft.AspNetCore.Mvc;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;

namespace ATSPM.DataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<IEnumerable<DetectionType>> GetDetectors()
        {
            return Ok(new DetectionRepo().GetAllDetectionTypes());
        }

        [HttpGet("{id}")]
        public ActionResult<DetectionRepo> GetDetectors(int id)
        {
            var result = new DetectionRepo().GetDetectionTypeByDetectionTypeID(id);

            if (result == null)
                return NotFound(result);

            return Ok(result);
        }
    }

    public class DetectionRepo : IDetectionTypeRepository
    {
        private List<DetectionType> _list;

        public DetectionRepo()
        {
            _list = new List<DetectionType>() {
                new DetectionType() { Id = Data.Enums.DetectionTypes.LLC, Description = "Christian" },
                new DetectionType() { Id = Data.Enums.DetectionTypes.B, Description = "Randi" } };
        }
        
        public void Add(DetectionType detectionType)
        {
            throw new NotImplementedException();
        }

        public List<DetectionType> GetAllDetectionTypes()
        {
            return _list;
        }

        public List<DetectionType> GetAllDetectionTypesNoBasic()
        {
            throw new NotImplementedException();
        }

        public DetectionType GetDetectionTypeByDetectionTypeID(int detectionTypeID)
        {
            return _list.FirstOrDefault(i => i.Id == (Data.Enums.DetectionTypes)detectionTypeID);
        }

        public void Remove(DetectionType detectionType)
        {
            throw new NotImplementedException();
        }

        public void Update(DetectionType detectionType)
        {
            throw new NotImplementedException();
        }
    }
}
