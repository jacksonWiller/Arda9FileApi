using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9FileApi.Domain.ValueObjects;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Arda9FileApi.Application.Users.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        IValidator<CreateUserCommand> validator,
        IUserRepository repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CreateUserHandler> logger)
    {
        _validator = validator;
        _repository = repository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validar o comando
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<CreateUserResponse>.Invalid(validationResult.AsErrors());
        }

        try
        {
           
            // Verificar se já existe um usuário com o mesmo email
            var existingUser = await _repository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<CreateUserResponse>.Error();
            }

            // Obter o userId do usuário autenticado (se disponível)
            Guid? createdBy = request.CreatedBy;
            if (createdBy == null)
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    createdBy = userId;
                }
            }

            // Criar os Value Objects
            var email = new Email(request.Email);
            var name = new PersonName(request.Name);

            Phone? phone = null;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                phone = new Phone(request.PhoneNumber, request.PhoneCountryCode ?? "+55");
            }

            Url? pictureUrl = null;
            if (!string.IsNullOrWhiteSpace(request.PictureUrl))
            {
                pictureUrl = new Url(request.PictureUrl);
            }

            // Criar a entidade de domínio User
            var user = new User(
                companyId: request.CompanyId,
                email: email,
                name: name,
                phone: phone,
                roles: request.Roles,
                pictureUrl: pictureUrl,
                locale: request.Locale,
                cognitoSub: request.CognitoSub,
                username: request.Username,
                subCompanyId: request.SubCompanyId,
                createdBy: createdBy
            );

            // Persistir no repositório
            var success = await _repository.CreateAsync(user);

            if (!success)
            {
                return Result<CreateUserResponse>.Error();
            }

            // Mapear para DTO
            var userDto = _mapper.Map<UserDto>(user);

            var response = new CreateUserResponse
            {
                User = userDto
            };

            return Result<CreateUserResponse>.Success(response, "User created successfully.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating user");
            return Result<CreateUserResponse>.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return Result<CreateUserResponse>.Error();
        }
    }
}