using Microsoft.AspNetCore.Mvc;
using NetGenerator5.Model;

namespace NetGenerator5.Web.Controllers
{
    [Route("api/example")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        [Route("")]
        public ExampleModel Get()
        {
            return new ExampleModel
            {
                Id = 1,
                Name = "Name"
            };
        }
    }
}
