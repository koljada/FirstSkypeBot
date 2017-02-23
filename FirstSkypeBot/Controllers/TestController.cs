using System.Web.Http;

namespace FirstSkypeBot.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("test")]
        public string Test()
        {
            return "Hello!";
        }
    }
}
