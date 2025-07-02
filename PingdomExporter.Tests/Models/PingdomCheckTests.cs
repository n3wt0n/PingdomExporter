using Xunit;
using PingdomExporter.Models;
using Newtonsoft.Json.Linq;

namespace PingdomExporter.Tests.Models
{
    public class PingdomCheckTests
    {
        [Fact]
        public void PingdomCheck_Defaults_AreSet()
        {
            var check = new PingdomCheck();
            Assert.Equal(0, check.Id);
            Assert.Equal(string.Empty, check.Name);
            Assert.Equal(string.Empty, check.Hostname);
        }

        [Fact]
        public void FlexibleTypeConverter_ReadJson_String_ReturnsString()
        {
            var converter = new FlexibleTypeConverter();
            var json = "\"http\"";
            var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(json));
            reader.Read();
            var result = converter.ReadJson(reader, typeof(object), null, new Newtonsoft.Json.JsonSerializer());
            Assert.Equal("http", result);
        }

        [Fact]
        public void FlexibleTypeConverter_ReadJson_Object_ReturnsJObject()
        {
            var converter = new FlexibleTypeConverter();
            var json = "{\"type\":\"http\"}";
            var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(json));
            reader.Read();
            var result = converter.ReadJson(reader, typeof(object), null, new Newtonsoft.Json.JsonSerializer());
            Assert.IsType<JObject>(result);
        }
    }
}
