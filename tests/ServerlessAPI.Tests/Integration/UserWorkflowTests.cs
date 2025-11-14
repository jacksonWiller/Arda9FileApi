//using Arda9UserApi.Application.DTOs;
//using Arda9UserApi.Application.Users.CreateUser;
//using Arda9UserApi.Application.Users.UpdateUser;
//using Arda9UserApi.Infrastructure.Repositories;
//using Arda9UserApi.Tests.Mocks;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using Xunit;

//namespace Arda9UserApi.Tests.Integration;

//public class UserWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
//{
//    private readonly WebApplicationFactory<Program> _factory;

//    public UserWorkflowTests(WebApplicationFactory<Program> factory)
//    {
//        _factory = factory.WithWebHostBuilder(builder =>
//        {
//            builder.ConfigureServices(services =>
//            {
//                var descriptor = services.SingleOrDefault(
//                    d => d.ServiceType == typeof(IUserRepository));
                
//                if (descriptor != null)
//                {
//                    services.Remove(descriptor);
//                }

//                services.AddScoped<IUserRepository, MockUserRepository>();
//            });
//        });
//    }

//    [Fact]
//    public async Task CompleteUserLifecycle_ShouldWork()
//    {
//        var client = _factory.CreateClient();
//        var companyId = Guid.NewGuid();

//        // 1. Create User
//        var createCommand = new CreateUserCommand
//        {
//            CompanyId = companyId,
//            Email = "lifecycle@example.com",
//            Name = "Lifecycle User",
//            PhoneNumber = "11999999999",
//            PhoneCountryCode = "+55",
//            Roles = new List<Guid> { Guid.NewGuid() },
//            Locale = "pt-BR"
//        };

//        var createResponse = await client.PostAsJsonAsync("/api/users", createCommand);
//        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
//        var createdUser = await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>();
//        createdUser.Should().NotBeNull();
//        var userId = createdUser!.User!.Id;

//        // 2. Get User by ID
//        var getResponse = await client.GetAsync($"/api/users/{companyId}/{userId}");
//        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
//        var retrievedUser = await getResponse.Content.ReadFromJsonAsync<UserDto>();
//        retrievedUser.Should().NotBeNull();
//        retrievedUser!.Email.Should().Be(createCommand.Email);

//        // 3. Update User
//        var updateCommand = new UpdateUserCommand
//        {
//            Id = userId,
//            CompanyId = companyId,
//            Name = "Updated Lifecycle User",
//            PhoneNumber = "11988888888",
//            PhoneCountryCode = "+55",
//            Locale = "en-US"
//        };

//        var updateResponse = await client.PutAsJsonAsync($"/api/users/{companyId}/{userId}", updateCommand);
//        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

//        // 4. Change Status to SUSPENDED
//        var statusResponse = await client.PatchAsync($"/api/users/{companyId}/{userId}/status/SUSPENDED", null);
//        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

//        // 5. Get User by Email
//        var emailResponse = await client.GetAsync($"/api/users/email/{createCommand.Email}");
//        emailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
//        var userByEmail = await emailResponse.Content.ReadFromJsonAsync<UserDto>();
//        userByEmail.Should().NotBeNull();
//        userByEmail!.Status.Should().Be("SUSPENDED");

//        // 6. Delete User
//        var deleteResponse = await client.DeleteAsync($"/api/users/{companyId}/{userId}");
//        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

//        // 7. Verify User is Deleted
//        var verifyResponse = await client.GetAsync($"/api/users/{companyId}/{userId}");
//        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
//    }
//}