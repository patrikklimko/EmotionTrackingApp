using System.Data.Common;
using API.Auth;
using API.Utilities;
using DTO;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Protobuf.Services;

namespace API_Integration_Tests;

public class UsersServiceIntegrationTests
{
  private readonly UsersService service;
  private readonly AuthUtilities authUtilities;
  private readonly PasswordHasherUtil passwordHasherUtil;
  public UsersServiceIntegrationTests()
  {
    passwordHasherUtil = new PasswordHasherUtil("VIAUniversityCollege");
    service = new UsersService();
    authUtilities = new AuthUtilities();
  }

  [Fact(Skip = "doesnt handle if the user already exists")]
  public async Task Create_ShouldCreateUser()
  {
    var user = new UserRegisterDto
    {
      Username = "someUser",
      Password = "password",
      Email = "someUser@gmail.com"
    };

    var result = await service.Create(user);

    Assert.NotNull(result);
    Assert.Equal(user.Username, result.Username);
    Assert.Equal(user.Email, result.Email);
  }
  [Fact]
  public async Task Create_ShouldThrowException_WhenUserIsInvalid()
  {
    var invalidUser = new UserRegisterDto
    {
      Username = null,
      Password = "password",
      Email = "invalidemail"
    };

    var exception = await Record.ExceptionAsync(() => service.Create(invalidUser));

    Assert.NotNull(exception);
    Assert.IsType<ArgumentNullException>(exception);
  }
  
  [Fact]
  public async Task GetByUsernameAndPassword_ShouldReturnUser()
  {
    var username = "terry_jeffords";
    var password = "password";

    var hashedPassword = passwordHasherUtil.HashPassword(password);

    var result = await service.GetByUsernameAndPassword(username, hashedPassword);

    Assert.Equal(username, result.Username);
  }
  
  [Fact]
  public async Task GetByUsernameAndPassword_ShouldReturnNull_WhenCredentialsAreInvalid()
  {
    var username = "invaliduser";
    var password = "wrongpassword";

    var result = await service.GetByUsernameAndPassword(username, password);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetByUsername_ShouldReturnUser()
  {
    var username = "jake_peralta";

    var result = await service.GetByUsername(username);

    Assert.NotNull(result);
    Assert.Equal(username, result.Username);
  }
  
  [Fact]
  public async Task ChangePassword_ShouldReturnNull_WhenUserIdIsInvalid()
  {
    int invalidUserId = 9999;
    var changePasswordDto = new ChangePasswordDto
    {
      NewPassword = "password"
    };

    var exception = await Record.ExceptionAsync(() => service.ChangePassword(invalidUserId, changePasswordDto));
    
    Assert.Null(exception);
  }

  [Fact]
  public async Task ChangePassword_ShouldChangePassword()
  {
    int userId = 1;
    var changePasswordDto = new ChangePasswordDto
    {
      NewPassword = "password"
    };

    var result = await service.ChangePassword(userId, changePasswordDto);

    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
  }

  [Fact]
  public async Task GetById_ShouldReturnUser()
  {
    int userId = 1;

    var result = await service.GetById(userId);

    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
  }
  
  [Fact]
  public async Task GetById_ShouldReturnNull_WhenUserIdIsInvalid()
  {
    int invalidUserId = 9999;

    var result = await service.GetById(invalidUserId);

    Assert.Null(result);
  }
}