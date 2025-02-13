using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using API.Auth;
using DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Protobuf.Services.Interfaces;

namespace API_Integration_Tests;

public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient client;
  private readonly IUsersService usersService;
  private readonly AuthUtilities authUtilities;
  private string token;

  public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
  {
    client = factory.CreateClient();
    var scope = factory.Services.CreateScope();
    usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
    authUtilities = scope.ServiceProvider.GetRequiredService<AuthUtilities>();
    GenerateToken();
  }

  private void GenerateToken()
  {
    token = authUtilities.GenerateJwtToken(new UserReturnDto
    {
      Id = 1,
      Username = "tesint_user",
      Email = "testingemail@email.com",
      CreatedAt = DateTime.Now,
      UpdatedAt = DateTime.Now,
      Streak = 0,
    });
  }

  private async Task AuthenticateAsync()
  {
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
  }

  [Fact]
  public async Task GetUser_ShouldReturnOk()
  {
    await AuthenticateAsync();

    var response = await client.GetAsync("/Users/1");
    response.EnsureSuccessStatusCode();

    var responseString = await response.Content.ReadAsStringAsync();
    var user = JsonSerializer.Deserialize<UserReturnDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    Assert.NotNull(user);
  }

  [Fact]
  public async Task GetUser_ShouldReturnNotFound_WhenInvalidId()
  {
    await AuthenticateAsync();

    var response = await client.GetAsync("/Users/9999");
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}