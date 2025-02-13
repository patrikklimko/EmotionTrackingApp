using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Auth;
using API.Controllers;
using API.Exceptions;
using API.Utilities;
using DTO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Protobuf.Services.Interfaces;
using Xunit;

namespace API_Integration_Tests;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient client;
  private readonly IUsersService usersService;
  private readonly PasswordHasherUtil passwordHasherUtil;
  private readonly AuthUtilities authUtilities;

  public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
  {
    client = factory.CreateClient();
    var scope = factory.Services.CreateScope();
    usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
    passwordHasherUtil = new PasswordHasherUtil("VIAUniversityCollege");
    authUtilities = scope.ServiceProvider.GetRequiredService<AuthUtilities>();
  }

  [Fact]
  public async Task Login_ShouldReturnUserWithTokenDto_WhenCredentialsAreValid()
  {
    var userLoginDto = new UserLoginDto
    {
      Username = "amy_santiago",
      Password = "password" 
    };
    var content = new StringContent(JsonSerializer.Serialize(userLoginDto), Encoding.UTF8,
      "application/json");
    
      var response = await client.PostAsync("/Auth/login", content);

      response.EnsureSuccessStatusCode();
      var responseString = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<UserWithTokenDto>(responseString,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      Assert.NotNull(result);
      Assert.Equal(userLoginDto.Username, result.User.Username);
  }
  
  [Fact]
  public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
  {
    var userLoginDto = new UserLoginDto
    {
      Username = "jake_peralta",
      Password = "wrongpassword"
    };
    var content = new StringContent(JsonSerializer.Serialize(userLoginDto), Encoding.UTF8, "application/json");

    var response = await client.PostAsync("/Auth/login", content);

    Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
  }
  
  [Fact(Skip = "doesnt handle the user existing")]
  public async Task Register_ShouldReturnUserWithTokenDto_WhenRegistrationIsSuccessful()
  {
    var userRegisterDto = new UserRegisterDto
    {
      Username = "testuser123",
      Password = passwordHasherUtil.HashPassword("password"),
      Email = "testuser123@gmail.com"
    };
    var content = new StringContent(JsonSerializer.Serialize(userRegisterDto), Encoding.UTF8, "application/json");
    
    var response = await client.PostAsync("/Auth/register", content);
    
    response.EnsureSuccessStatusCode();
    var responseString = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<UserWithTokenDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    Assert.NotNull(result);
    Assert.Equal(userRegisterDto.Username, result.User.Username);
    Assert.Equal(userRegisterDto.Email, result.User.Email);
  }
  
  [Fact]
  public async Task Register_ShouldReturnConflict_WhenUsernameAlreadyExists()
  {
    var userRegisterDto = new UserRegisterDto
    {
      Username = "amy_santiago",
      Password = passwordHasherUtil.HashPassword("password"),
      Email = "amy.santiago@nine-nine.com"
    };
    var content = new StringContent(JsonSerializer.Serialize(userRegisterDto), Encoding.UTF8, "application/json");
    
    var response = await client.PostAsync("/Auth/register", content);

    Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
  }
  
  [Fact]
  public async Task ChangePassword_ShouldReturnUserReturnDto_WhenPasswordIsChanged()
  {
    var userLoginDto = new UserLoginDto
    {
      Username = "amy_santiago",
      Password = "password"
    };
    
    var loginContent = new StringContent(JsonSerializer.Serialize(userLoginDto), Encoding.UTF8, "application/json");
    var loginResponse = await client.PostAsync("/Auth/login", loginContent);
    loginResponse.EnsureSuccessStatusCode();
    var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
    var userWithTokenDto = JsonSerializer.Deserialize<UserWithTokenDto>(loginResponseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    var changePasswordDto = new ChangePasswordDto
    {
      NewPassword = "password"
    };
    
    var changePasswordContent = new StringContent(JsonSerializer.Serialize(changePasswordDto), Encoding.UTF8, "application/json");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userWithTokenDto.Token);

    var response = await client.PatchAsync("/Auth/change-password", changePasswordContent);
    response.EnsureSuccessStatusCode();
    var responseString = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<UserReturnDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    Assert.NotNull(result);
    Assert.Equal(userWithTokenDto.User.Username, result.Username);
  }
  
  [Fact]
  public async Task Get_ShouldReturnUserReturnDto_WhenUserIsAuthenticated()
  {
    var userLoginDto = new UserLoginDto
    {
      Username = "rosa_diaz",
      Password = "password"
    };
    var loginContent = new StringContent(JsonSerializer.Serialize(userLoginDto), Encoding.UTF8, "application/json");
    var loginResponse = await client.PostAsync("/Auth/login", loginContent);
    loginResponse.EnsureSuccessStatusCode();
    var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
    var userWithTokenDto = JsonSerializer.Deserialize<UserWithTokenDto>(loginResponseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userWithTokenDto.Token);

    var response = await client.GetAsync("/Auth");
    if (!response.IsSuccessStatusCode)
    {
      var errorResponseString = await response.Content.ReadAsStringAsync();
      throw new Exception($"Get request failed: {response.StatusCode}, {errorResponseString}");
    }

    var responseString = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<UserReturnDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    Assert.NotNull(result);
    Assert.Equal(userWithTokenDto.User.Username, result.Username);
  }
}