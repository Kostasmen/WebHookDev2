using Ingenica_WebAPI.DynamicsNAVWS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Xml;

namespace Ingenica_WebAPI.Controllers
{
    public class WebHookController : ApiController
    {
        public HttpResponseMessage Post([FromBody]JArray input)
        { 
            try
            {
                System.Xml.XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode("{\"results\":" + input + "}", "XmlDocument");
                doc.Save(WebConfigurationManager.AppSettings["FileLocation"] + @"\" + System.DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".xml");                
            }
            catch
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }
            response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private HttpResponseMessage response;
    }
}
