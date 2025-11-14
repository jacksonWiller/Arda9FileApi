//using Bogus;
//using Catalog.Domain.Entities.UserAggregate;
//using Catalog.Domain.Enums;
//using Catalog.Domain.ValueObjects;
//using Arda9UserApi.Infrastructure.Repositories;

//namespace Arda9UserApi.Tests.Mocks;

//internal class MockUserRepository : IUserRepository
//{
//    private readonly List<User> _users = new();
//    private readonly Faker _faker = new();

//    public MockUserRepository()
//    {
//        // Seed com alguns usuários fake
//        SeedUsers(10);
//    }

//    private void SeedUsers(int count)
//    {
//        var companyId = Guid.NewGuid();
        
//        for (int i = 0; i < count; i++)
//        {
//            var user = new User(
//                companyId,
//                new Email(_faker.Internet.Email()),
//                new PersonName(_faker.Name.FullName()),
//                new Phone(_faker.Phone.PhoneNumber("###########"), "+55"),
//                new List<Guid> { Guid.NewGuid() },
//                new Url(_faker.Internet.Avatar()),
//                "pt-BR",
//                _faker.Random.Guid().ToString(),
//                _faker.Internet.UserName(),
//                null,
//                Guid.NewGuid()
//            );

//            _users.Add(user);
//        }
//    }

//    public Task<bool> CreateAsync(User user)
//    {
//        _users.Add(user);
//        return Task.FromResult(true);
//    }

//    public Task<bool> DeleteAsync(User user)
//    {
//        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
//        if (existingUser != null)
//        {
//            _users.Remove(existingUser);
//            return Task.FromResult(true);
//        }
//        return Task.FromResult(false);
//    }

//    public Task<bool> UpdateAsync(User user)
//    {
//        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
//        if (existingUser != null)
//        {
//            _users.Remove(existingUser);
//            _users.Add(user);
//            return Task.FromResult(true);
//        }
//        return Task.FromResult(false);
//    }

//    public Task<User?> GetByIdAsync(Guid companyId, Guid userId)
//    {
//        var user = _users.FirstOrDefault(u => u.Id == userId && u.CompanyId == companyId);
//        return Task.FromResult(user);
//    }

//    public Task<User?> GetByEmailAsync(string email)
//    {
//        var user = _users.FirstOrDefault(u => u.Email.Value.Equals(email, StringComparison.OrdinalIgnoreCase));
//        return Task.FromResult(user);
//    }

//    public Task<User?> GetByCognitoSubAsync(string cognitoSub)
//    {
//        var user = _users.FirstOrDefault(u => u.CognitoSub == cognitoSub);
//        return Task.FromResult(user);
//    }

//    public Task<IList<User>> GetUsersByCompanyAsync(Guid companyId, int limit = 10)
//    {
//        var users = _users
//            .Where(u => u.CompanyId == companyId)
//            .Take(limit)
//            .ToList();

//        return Task.FromResult<IList<User>>(users);
//    }

//    public void AddUser(User user)
//    {
//        _users.Add(user);
//    }

//    public User GetFirstUser()
//    {
//        return _users.First();
//    }

//    public Guid GetFirstCompanyId()
//    {
//        return _users.First().CompanyId;
//    }
//}