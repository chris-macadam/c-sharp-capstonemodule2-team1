using TenmoClient.Services;

namespace TenmoClientTest
{
    [TestClass]
    public class TenmoApiServiceTest
    {
        private const string baseApiUrl = "http://localhost";
        private TenmoApiService apiService;

        [TestInitialize]
        public void Setup()
        {
            //create the api service
            apiService = new TenmoApiService(baseApiUrl);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Make sure no mock client is left from the test
            TenmoApiService.client = null;
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}