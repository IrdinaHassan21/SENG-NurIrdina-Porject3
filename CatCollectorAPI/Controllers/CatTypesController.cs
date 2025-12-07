using Microsoft.AspNetCore.Mvc;
using CatCollectorAPI.Models;

namespace CatCollectorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatTypesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var types = Enum.GetNames(typeof(CatType));
            return Ok(types);
        }
    }
}
