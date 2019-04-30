namespace NetAopEssentialsTest.Services
{

    /// <summary>
    /// Test singleton void
    /// </summary>
    public class TestSingletonVoidService : ITestSingletonVoidService
    {

        public TestSingletonVoidService()
        {
            Value = "Init";
        }

        public string Value { get; set; }
    }
}
