using Xunit;
using Moq;
using PingdomExporter.Services;
using PingdomExporter.Models;
using System.Net.Http;

namespace PingdomExporter.Tests.Services
{
    public class PingdomApiServiceTests
    {
        [Fact]
        public void Constructor_NullHttpClient_Throws()
        {
            var config = new ExportConfiguration();
            Assert.Throws<System.ArgumentNullException>(() => new PingdomApiService(null, config));
        }

        [Fact]
        public void Constructor_NullConfig_Throws()
        {
            var httpClient = new HttpClient();
            Assert.Throws<System.ArgumentNullException>(() => new PingdomApiService(httpClient, null));
        }
    }
}
