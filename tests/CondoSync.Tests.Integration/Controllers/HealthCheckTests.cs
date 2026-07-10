using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;

namespace CondoSync.Tests.Integration.Controllers;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnValidJson()
    {
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HealthzEndpoint_ShouldReturnHealthy()
    {
        var response = await _client.GetAsync("/healthz");
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("status").GetString().Should().Be("healthy");
    }
}
