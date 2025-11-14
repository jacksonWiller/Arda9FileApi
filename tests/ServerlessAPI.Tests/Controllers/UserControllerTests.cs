//using Arda9UserApi.Application.DTOs;
//using Arda9UserApi.Application.Users.CreateUser;
//using Arda9UserApi.Application.Users.UpdateUser;
//using Arda9UserApi.Infrastructure.Repositories;
//using Arda9UserApi.Tests.Mocks;
//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using Xunit;

//namespace Arda9UserApi.Tests.Controllers;

//public class UserControllerTests : IClassFixture<WebApplicationFactory<Program>>
//{
//    private readonly WebApplicationFactory<Program> _factory;
//    private readonly MockUserRepository _mockRepository;

//    public UserControllerTests(WebApplicationFactory<Program> factory)
//    {
//        _mockRepository = new MockUserRepository();
        
//        _factory = factory.WithWebHostBuilder(builder =>
//        {
//            builder.ConfigureServices(services =>
//            {
//                // Remove o repository real e adiciona o mock
//                var descriptor = services.SingleOrDefault(
//                    d => d.ServiceType == typeof(IUserRepository));
                
//                if (descriptor != null)
//                {
//                    services.Remove(descriptor);
//                }

//                services.AddScoped<IUserRepository>(_ => _mockRepository);
//            });
//        });
//    }

//    #region CreateUser Tests

//    [Fact]
//    public async Task CreateUser_WithValidData_ShouldReturnSuccess()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var command = new CreateUserCommand
//        {
//            CompanyId = Guid.NewGuid(),
//            Email = "test@example.com",
//            Name = "Test User",
//            PhoneNumber = "11999999999",
//            PhoneCountryCode = "+55",
//            Roles = new List<Guid> { Guid.NewGuid() },
//            Locale = "pt-BR"
//        };

//        // Act
//        var response = await client.PostAsJsonAsync("/api/users", command);

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//        var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>();
//        result.Should().NotBeNull();
//        result!.User.Should().NotBeNull();
//        result.User!.Email.Should().Be(command.Email);
//        object value = result.User.Name.Should().Be(command.Name);
//    }

//    [Fact]
//    public async Task CreateUser_WithInvalidEmail_ShouldReturnBadRequest()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var command = new CreateUserCommand
//        {
//            CompanyId = Guid.NewGuid(),
//            Email = "invalid-email",
//            Name = "Test User"
//        };

//        // Act
//        var response = await client.PostAsJsonAsync("/api/users", command);

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//    }

//    [Fact]
//    public async Task CreateUser_WithEmptyName_ShouldReturnBadRequest()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var command = new CreateUserCommand
//        {
//            CompanyId = Guid.NewGuid(),
//            Email = "test@example.com",
//            Name = ""
//        };

//        // Act
//        var response = await client.PostAsJsonAsync("/api/users", command);

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//    }

//    #endregion

//    #region GetUserById Tests

//    [Fact]
//    public async Task GetUserById_WithExistingUser_ShouldReturnUser()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var user = _mockRepository.GetFirstUser();
//        var companyId = user.CompanyId;
//        var userId = user.Id;

//        // Act
//        var response = await client.GetAsync($"/api/users/{companyId}/{userId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//        var result = await response.Content.ReadFromJsonAsync<UserDto>();
//        result.Should().NotBeNull();
//        result!.Id.Should().Be(userId);
//    }

//    [Fact]
//    public async Task GetUserById_WithNonExistingUser_ShouldReturnNotFound()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var companyId = Guid.NewGuid();
//        var userId = Guid.NewGuid();

//        // Act
//        var response = await client.GetAsync($"/api/users/{companyId}/{userId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//    }

//    #endregion

//    #region GetUserByEmail Tests

//    [Fact]
//    public async Task GetUserByEmail_WithExistingEmail_ShouldReturnUser()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var user = _mockRepository.GetFirstUser();
//        var email = user.Email.Value;

//        // Act
//        var response = await client.GetAsync($"/api/users/email/{email}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//        var result = await response.Content.ReadFromJsonAsync<UserDto>();
//        result.Should().NotBeNull();
//        result!.Email.Should().Be(email);
//    }

//    [Fact]
//    public async Task GetUserByEmail_WithNonExistingEmail_ShouldReturnNotFound()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var email = "nonexisting@example.com";

//        // Act
//        var response = await client.GetAsync($"/api/users/email/{email}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//    }

//    #endregion

//    #region GetUsersByCognitoSub Tests

//    [Fact]
//    public async Task GetUserByCognitoSub_WithExistingSub_ShouldReturnUser()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var user = _mockRepository.GetFirstUser();
//        var cognitoSub = user.CognitoSub!;

//        // Act
//        var response = await client.GetAsync($"/api/users/cognito/{cognitoSub}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//        var result = await response.Content.ReadFromJsonAsync<UserDto>();
//        result.Should().NotBeNull();
//        result!.CognitoSub.Should().Be(cognitoSub);
//    }

//    [Fact]
//    public async Task GetUserByCognitoSub_WithNonExistingSub_ShouldReturnNotFound()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var cognitoSub = Guid.NewGuid().ToString();

//        // Act
//        var response = await client.GetAsync($"/api/users/cognito/{cognitoSub}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//    }

//    #endregion

//    #region GetUsersByCompany Tests

//    [Theory]
//    [InlineData(5)]
//    [InlineData(10)]
//    public async Task GetUsersByCompany_WithValidLimit_ShouldReturnLimitedList(int limit)
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var companyId = _mockRepository.GetFirstCompanyId();

//        // Act
//        var response = await client.GetAsync($"/api/users/company/{companyId}?limit={limit}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//        var result = await response.Content.ReadFromJsonAsync<IList<UserDto>>();
//        result.Should().NotBeNull();
//        result!.Count.Should().BeLessOrEqualTo(limit);
//    }

//    [Theory]
//    [InlineData(0)]
//    [InlineData(-1)]
//    [InlineData(101)]
//    public async Task GetUsersByCompany_WithInvalidLimit_ShouldReturnBadRequest(int limit)
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var companyId = _mockRepository.GetFirstCompanyId();

//        // Act
//        var response = await client.GetAsync($"/api/users/company/{companyId}?limit={limit}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//    }

//    #endregion

//    #region UpdateUser Tests

//    [Fact]
//    public async Task UpdateUser_WithValidData_ShouldReturnSuccess()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var user = _mockRepository.GetFirstUser();
        
//        var command = new UpdateUserCommand
//        {
//            Id = user.Id,
//            CompanyId = user.CompanyId,
//            Name = "Updated Name",
//            PhoneNumber = "11988888888",
//            PhoneCountryCode = "+55",
//            Locale = "en-US",
//            UpdatedBy = Guid.NewGuid()
//        };

//        // Act
//        var response = await client.PutAsJsonAsync($"/api/users/{command.CompanyId}/{command.Id}", command);

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//        var result = await response.Content.ReadFromJsonAsync<UpdateUserResponse>();
//        result.Should().NotBeNull();
//        result!.User.Should().NotBeNull();
//        result.User!.Name.Should().Be(command.Name);
//    }

//    [Fact]
//    public async Task UpdateUser_WithNonExistingUser_ShouldReturnNotFound()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var companyId = Guid.NewGuid();
//        var userId = Guid.NewGuid();
        
//        var command = new UpdateUserCommand
//        {
//            Id = userId,
//            CompanyId = companyId,
//            Name = "Updated Name"
//        };

//        // Act
//        var response = await client.PutAsJsonAsync($"/api/users/{companyId}/{userId}", command);

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//    }

//    #endregion

//    #region DeleteUser Tests

//    [Fact]
//    public async Task DeleteUser_WithExistingUser_ShouldReturnSuccess()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var user = _mockRepository.GetFirstUser();
//        var companyId = user.CompanyId;
//        var userId = user.Id;

//        // Act
//        var response = await client.DeleteAsync($"/api/users/{companyId}/{userId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//    }

//    [Fact]
//    public async Task DeleteUser_WithNonExistingUser_ShouldReturnNotFound()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var companyId = Guid.NewGuid();
//        var userId = Guid.NewGuid();

//        // Act
//        var response = await client.DeleteAsync($"/api/users/{companyId}/{userId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//    }

//    #endregion

//    #region ChangeStatus Tests

//    [Theory]
//    [InlineData("ACTIVE")]
//    [InlineData("SUSPENDED")]
//    [InlineData("INACTIVE")]
//    public async Task ChangeUserStatus_WithValidStatus_ShouldReturnSuccess(string status)
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var user = _mockRepository.GetFirstUser();
//        var companyId = user.CompanyId;
//        var userId = user.Id;

//        // Act
//        var response = await client.PatchAsync(
//            $"/api/users/{companyId}/{userId}/status/{status}", 
//            null
//        );

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//    }

//    [Fact]
//    public async Task ChangeUserStatus_WithInvalidStatus_ShouldReturnBadRequest()
//    {
//        // Arrange
//        var client = _factory.CreateClient();
//        var user = _mockRepository.GetFirstUser();
//        var companyId = user.CompanyId;
//        var userId = user.Id;

//        //// Act
//        var response = await client.PatchAsync(
//            $"/api/users/{companyId}/{userId}/status/INVALID_STATUS", 
//            null
//        );

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//    }

//    #endregion
//}