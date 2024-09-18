using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_KN_P_212.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;
        public LocationController(DataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        [HttpGet("{id}")]
        public List<Location> DoGet(String id)
        {
            return _dataAccessor.ContentDao.GetLocations(id);
        }

        [HttpPost]
        public String DoPost([FromForm] LocationPostModel model)
        {
            try
            {
                String? fileName = null;
                if (model.Photo != null)
                {
                    String ext = Path.GetExtension(model.Photo.FileName);
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/images/content/";
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while (System.IO.File.Exists(pathName));
                    using var stream = System.IO.File.OpenWrite(pathName);
                    model.Photo.CopyTo(stream);
                }
                _dataAccessor.ContentDao.AddLocation(
                    name: model.Name,
                    description: model.Description,
                    CategoryId: model.CategoryId,
                    Stars: model.Stars,
                    PhotoUrl: fileName);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Ok";
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Error";
            }
        }

        [HttpPatch]
        public Location? DoPatch(String slug)
        {
            return _dataAccessor.ContentDao.GetLocationBySlug(slug);
        }
    }

    public class LocationPostModel
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public Guid CategoryId { get; set; }
        public int Stars { get; set; }
        public IFormFile Photo { get; set; }
    }
}