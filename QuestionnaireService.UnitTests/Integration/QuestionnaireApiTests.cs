using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QuestionnaireService.Status;

namespace QuestionnaireService.UnitTests.Integration;

public sealed class QuestionnaireApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public QuestionnaireApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StatusEndpoint_ReturnsMetadata()
    {
        var status = await _client.GetFromJsonAsync<ServiceStatus>("/status");

        Assert.NotNull(status);
        Assert.Equal(typeof(Program).Assembly.GetName().Name, status!.Service);
        Assert.False(string.IsNullOrWhiteSpace(status.Environment));
        Assert.False(string.IsNullOrWhiteSpace(status.Version));
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
