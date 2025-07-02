using Xunit;
using Moq;
using PingdomExporter.Services;
using PingdomExporter.Models;
using System.Threading.Tasks;

namespace PingdomExporter.Tests.Services
{
    public class ExportServiceTests
    {
        [Fact]
        public void Constructor_NullApiService_Throws()
        {
            var config = new ExportConfiguration();
            Assert.Throws<System.ArgumentNullException>(() => new ExportService(null, config));
        }

        [Fact]
        public void Constructor_NullConfig_Throws()
        {
            var apiMock = new Mock<IPingdomApiService>();
            Assert.Throws<System.ArgumentNullException>(() => new ExportService(apiMock.Object, null));
        }
    }
}
