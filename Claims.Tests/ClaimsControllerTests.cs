using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests(ClaimsApiFixture fixture)
    {
        [Fact]
        public async Task Get_Claims()
        {
            var client = fixture.CreateClient();

            var response = await client.GetAsync("/Claims");

            response.EnsureSuccessStatusCode();

            //TODO: Apart from ensuring 200 OK being returned, what else can be asserted?
        }

    }
}
