using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SecretsManager;
using Arda9UserApi.Application.Services;
using Arda9UserApi.Configuration;
using Arda9UserApi.Core.Behaviors;
using Arda9UserApi.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

//Adiciona suporte ao HttpContextAccessor
builder.Services.AddHttpContextAccessor();

//Logger
builder.Logging
        .ClearProviders()
        .AddJsonConsole();

// Configurar as opções do AWS Cognito
builder.Services.Configure<AwsCognitoConfig>(
    builder.Configuration.GetSection("AwsCognito"));

// Obter configuração do Cognito para usar na autenticação JWT
var cognitoConfig = builder.Configuration.GetSection("AwsCognito").Get<AwsCognitoConfig>();
var userPoolId = cognitoConfig?.UserPoolId ?? "us-east-1_tg7PHhZle";
var region = cognitoConfig?.Region ?? "us-east-1";
 
// Add services to the container.
builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

// Configuração da autenticação JWT com AWS Cognito
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
            ValidateAudience = false, // Cognito não usa audience padrão
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
        options.RequireHttpsMetadata = false; // Para desenvolvimento local
    });

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

    // Adicionar behaviors (opcional)
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Configurar AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Books API", 
        Version = "v1",
        Description = "API para gerenciamento de livros usando AWS Lambda e DynamoDB com CQRS customizado"
    });

    // Configuração para autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

string awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.USEast2.SystemName;
builder.Services
        .AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(awsRegion)))
        .AddSingleton<IAmazonCognitoIdentityProvider>(new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(awsRegion)))
        .AddSingleton<IAmazonSecretsManager>(new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(awsRegion)))
        .AddScoped<IDynamoDBContext, DynamoDBContext>()
        .AddScoped<IUserRepository, UserRepository>()
        .AddScoped<IAuthService, AuthService>();

// Add AWS Lambda support
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Books API v1");
    c.RoutePrefix = string.Empty; // Define o Swagger como página inicial
});

app.UseHttpsRedirection();

// Adicionar middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
