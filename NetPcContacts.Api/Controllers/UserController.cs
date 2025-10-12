using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NetPcContacts.Api.Controllers
{
    /// <summary>
    /// Mo¿liwoœæ póŸniejszego zaimplementowania do zarz¹dzania u¿ytkownikami
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        //[HttpGet("Hello")]
        //public IActionResult Hello()
        //{
        //    return Ok("Hello World");
        //}
    }

}
