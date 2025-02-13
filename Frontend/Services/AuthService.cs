using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DTO;
using Frontend.Services.Interfaces;
using Frontend.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using SharedUtil;

namespace Frontend.Services;

public class AuthService : AuthenticationStateProvider
{
  private readonly IStorageService _storageService;
  private readonly NonAuthedClient _httpClient;
  private ClaimsPrincipal currentClaimsPrincipal;

  public AuthService(IStorageService storageService, NonAuthedClient httpClient)
  {
    _storageService = storageService;
    _httpClient = httpClient;
  }

  public async Task<UserWithTokenDto?> Register(UserRegisterDto user)
  {
    var data = new
    {
      username = user.Username,
      password = user.Password,
      email = user.Email
    };

    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.PostAsync("Auth/register", content);

    var auth = await (new ApiParsingUtils<UserWithTokenDto>()).Process(response);

    List<Claim> claims = new()
    {
      new Claim(ClaimTypes.Name, auth?.User?.Username),
      new Claim(ClaimTypes.Email, auth?.User?.Email),
      new Claim(ClaimTypes.NameIdentifier, auth?.User?.Id.ToString()),
      new Claim("Token", auth?.Token.ToString()),
      new Claim("Streak", auth.User.Streak?.ToString())
    };

    ClaimsIdentity identity = new ClaimsIdentity(claims, "apiauth");
    currentClaimsPrincipal = new ClaimsPrincipal(identity);


    NotifyAuthenticationStateChanged(
      Task.FromResult(new AuthenticationState(currentClaimsPrincipal)));

    await _storageService.SetItem("auth", auth);

    return auth?.User is null ? null : auth;
  }

  public async Task<UserWithTokenDto?> Login(string username, string password)
  {
    var data = new
    {
      username, password
    };

    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.PostAsync("Auth/login", content);

    var auth = await (new ApiParsingUtils<UserWithTokenDto>()).Process(response);

    List<Claim> claims = new()
    {
      new Claim(ClaimTypes.Name, auth?.User?.Username),
      new Claim(ClaimTypes.Email, auth?.User?.Email),
      new Claim(ClaimTypes.NameIdentifier, auth?.User?.Id.ToString()),
      new Claim("Token", auth?.Token.ToString()),
      new Claim("Streak", auth.User.Streak?.ToString())
    };

    ClaimsIdentity identity = new ClaimsIdentity(claims, "apiauth");
    currentClaimsPrincipal = new ClaimsPrincipal(identity);

    NotifyAuthenticationStateChanged(
      Task.FromResult(new AuthenticationState(currentClaimsPrincipal)));

    await _storageService.SetItem("auth", auth);

    return auth?.User is null ? null : auth;
  }

  public async Task<UserReturnDto> ChangePassword(UserWithTokenDto userWithTokenDto,
    string newPassword)
  {
    var data = new ChangePasswordDto
    {
      NewPassword = newPassword
    };

    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var headers = new Dictionary<string, string>
    {
      { "Authorization", $"Bearer {userWithTokenDto.Token}" }
    };

    var response = await _httpClient.PatchAsync("Auth/change-password", content, headers);

    return await (new ApiParsingUtils<UserReturnDto>()).Process(response);
  }

  public async Task<UserWithTokenDto> GetAuth()
  {
    return await _storageService.GetItem<UserWithTokenDto>("auth");
  }

  public async Task Logout()
  {
    await _storageService.RemoveItem("auth");
  }

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    var auth = await _storageService.GetItem<UserWithTokenDto>("auth");

    if (auth is null)
    {
      return new AuthenticationState(new ClaimsPrincipal());
    }

    List<Claim> claims = new()
    {
      new Claim(ClaimTypes.Name, auth.User.Username),
      new Claim(ClaimTypes.Email, auth.User.Email),
      new Claim(ClaimTypes.NameIdentifier, auth.User.Id.ToString()),
      new Claim("Token", auth.Token.ToString())
    };

    ClaimsIdentity identity = new ClaimsIdentity(claims, "apiauth");
    currentClaimsPrincipal = new ClaimsPrincipal(identity);

    return new AuthenticationState(currentClaimsPrincipal);
  }
}