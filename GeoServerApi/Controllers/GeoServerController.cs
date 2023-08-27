using GeoServerApi.GeoServer;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GeoServerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeoServerController : ControllerBase
    {
        // GET: api/<GeoServerController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<GeoServerController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            String URLString = "http://localhost:8080/geoserver/rest/workspaces/geotest/datastores/openstreetmap/featuretypes/postgis_point.xml";
            XmlTextReader reader = new XmlTextReader(URLString);
            var rst = new RestClient("http://localhost:8080");
            var strings = rst.getFeatureType("geotest", "openstreetmap", "postgis_point");
            return strings;
        }

        // POST api/<GeoServerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<GeoServerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GeoServerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
