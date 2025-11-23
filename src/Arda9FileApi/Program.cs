using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3Control;
using Amazon.SecretsManager;
using Arda9FileApi.Application.Services;
using Arda9FileApi.Configuration;
using Arda9FileApi.Core.Behaviors;
using Arda9FileApi.Infrastructure.Repositories;
using Arda9FileApi.Infrastructure.Services;
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

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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
        Title = "Arda9 File API", 
        Version = "v1",
        Description = "API para gerenciamento de usuários, autenticação e buckets usando AWS Lambda, DynamoDB, Cognito e S3"
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

// Configuração da região AWS
string awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.USEast1.SystemName;
var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);

// Registrar serviços AWS
builder.Services
    .AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(regionEndpoint))
    .AddSingleton<IAmazonCognitoIdentityProvider>(new AmazonCognitoIdentityProviderClient(regionEndpoint))
    .AddSingleton<IAmazonSecretsManager>(new AmazonSecretsManagerClient(regionEndpoint))
    .AddSingleton<IAmazonS3>(new AmazonS3Client(regionEndpoint))
    .AddSingleton<IAmazonS3Control>(new AmazonS3ControlClient(regionEndpoint))
    .AddScoped<IDynamoDBContext, DynamoDBContext>();

// Registrar Repositories
builder.Services
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IBucketRepository, BucketRepository>()
    .AddScoped<IFileRepository, FileRepository>()
    .AddScoped<IFolderRepository, FolderRepository>();

// Registrar Services
builder.Services
    .AddScoped<IAuthService, AuthService>()
    .AddScoped<IS3Service, S3Service>();

// Add AWS Lambda support
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arda9 File API v1");
    c.RoutePrefix = string.Empty; // Define o Swagger como página inicial
});

// Habilitar CORS
app.UseCors();

app.UseHttpsRedirection();

// Adicionar middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
