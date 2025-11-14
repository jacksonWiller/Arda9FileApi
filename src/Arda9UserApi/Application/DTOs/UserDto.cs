using Amazon.DynamoDBv2.DataModel;
using Catalog.Domain.Enums;

namespace Arda9UserApi.Application.DTOs;

/// <summary>
/// DTO for User with DynamoDB annotations
/// </summary>
[DynamoDBTable("arda-user-v1")]
public class UserDto
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }

    [DynamoDBProperty]
    public Guid CompanyId { get; set; }

    [DynamoDBProperty]
    public Guid? SubCompanyId { get; set; }

    [DynamoDBProperty]
    public string Email { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public PhoneDto? Phone { get; set; }

    [DynamoDBProperty]
    public string Status { get; set; } = UserStatus.ACTIVE.ToString();

    [DynamoDBProperty]
    public List<Guid> Roles { get; set; } = [];

    [DynamoDBProperty]
    public string? PictureUrl { get; set; }

    [DynamoDBProperty]
    public string? Locale { get; set; }

    [DynamoDBProperty]
    public string? CognitoSub { get; set; }

    [DynamoDBProperty]
    public string? Username { get; set; }

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }

    [DynamoDBProperty]
    public DateTime UpdatedAt { get; set; }

    [DynamoDBProperty]
    public Guid? CreatedBy { get; set; }

    [DynamoDBProperty]
    public Guid? UpdatedBy { get; set; }

    // GSI para buscar por email
    [DynamoDBGlobalSecondaryIndexHashKey("EmailIndex")]
    public string GSI1PK { get; set; } = string.Empty; // EMAIL#<email>

    // GSI para buscar por company
    [DynamoDBGlobalSecondaryIndexHashKey("CompanyIndex")]
    public string GSI2PK { get; set; } = string.Empty; // COMPANY#<companyId>

    // GSI para buscar por CognitoSub
    [DynamoDBGlobalSecondaryIndexHashKey("CognitoIndex")]
    public string GSI3PK { get; set; } = string.Empty; // COGNITO#<cognitoSub>
}