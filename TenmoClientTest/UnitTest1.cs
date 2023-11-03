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

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}