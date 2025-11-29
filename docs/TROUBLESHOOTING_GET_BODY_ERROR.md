# Troubleshooting: "Request with GET/HEAD method cannot have body"

## ?? Erro

```
TypeError: Failed to execute 'fetch' on 'Window': Request with GET/HEAD method cannot have body.
```

## ?? Descrição

Este erro ocorre quando você tenta enviar um **body (corpo)** em uma requisição HTTP **GET** ou **HEAD**, o que é **proibido** pela especificação HTTP (RFC 7231).

## ?? Causa Raiz

No ASP.NET Core, quando você **não especifica** de onde vem o parâmetro de um objeto complexo, o framework tenta fazer **model binding** de múltiplas fontes, incluindo:

1. Route values (da URL)
2. Query string
3. **Request body** ??
4. Form data
5. Headers

Para métodos GET, isso causa um problema porque o cliente tenta enviar dados no body.

## ? Exemplo do Problema

### Código Errado

```csharp
[HttpGet("{tenantId}")]
public async Task<IActionResult> GetFolders(
    Guid tenantId, 
    GetFoldersQuery query)  // ? SEM [FromQuery]
{
    query.TenantId = tenantId;
    var result = await _mediator.Send(query);
    return result.ToActionResult();
}
```

### O que acontece no cliente

```javascript
// O ASP.NET Core tenta fazer binding do body
// O cliente JavaScript tenta enviar:
fetch('/api/folders/123', {
  method: 'GET',
  body: JSON.stringify({  // ? ERRO! GET não pode ter body
    parentId: 'uuid',
    includeEmpty: true,
    depth: 2
  })
});
```

## ? Solução

### Opção 1: Usar [FromQuery] (Recomendado para GET)

```csharp
[HttpGet("{tenantId}")]
public async Task<IActionResult> GetFolders(
    Guid tenantId, 
    [FromQuery] GetFoldersQuery query)  // ? Especifica query string
{
    query.TenantId = tenantId;
    var result = await _mediator.Send(query);
    return result.ToActionResult();
}
```

**Cliente:**
```javascript
// Agora funciona corretamente
fetch('/api/folders/123?parentId=uuid&includeEmpty=true&depth=2', {
  method: 'GET',
  headers: {
    'Authorization': 'Bearer token'
  }
});
```

### Opção 2: Parâmetros Individuais

```csharp
[HttpGet("{tenantId}")]
public async Task<IActionResult> GetFolders(
    Guid tenantId,
    [FromQuery] Guid? parentId = null,
    [FromQuery] bool includeEmpty = true,
    [FromQuery] int depth = 1)
{
    var query = new GetFoldersQuery
    {
        TenantId = tenantId,
        ParentId = parentId,
        IncludeEmpty = includeEmpty,
        Depth = depth
    };

    var result = await _mediator.Send(query);
    return result.ToActionResult();
}
```

### Opção 3: Mudar para POST (Se realmente precisa de body)

```csharp
[HttpPost("{tenantId}/search")]  // ? POST pode ter body
public async Task<IActionResult> SearchFolders(
    Guid tenantId,
    [FromBody] GetFoldersQuery query)  // ? Body em POST é válido
{
    query.TenantId = tenantId;
    var result = await _mediator.Send(query);
    return result.ToActionResult();
}
```

## ?? Atributos de Binding no ASP.NET Core

| Atributo | Fonte | Uso |
|----------|-------|-----|
| `[FromRoute]` | URL path | IDs e parâmetros na rota |
| `[FromQuery]` | Query string | Filtros, paginação, pesquisa |
| `[FromBody]` | Request body | Dados complexos (POST/PUT/PATCH) |
| `[FromForm]` | Form data | Upload de arquivos, formulários |
| `[FromHeader]` | HTTP headers | Tokens, metadados |
| `[FromServices]` | DI container | Injeção de dependência |

## ?? Quando Usar Cada Método HTTP

### GET - Obter Dados
- ? **SEM body**
- ? Parâmetros na query string
- ? Idempotente e seguro
- ? Pode ser cacheado

```csharp
[HttpGet("{tenantId}")]
public async Task<IActionResult> GetFiles(
    Guid tenantId,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 50,
    [FromQuery] string? search = null)
```

### POST - Criar Recurso
- ? **COM body**
- ? Dados complexos no body
- ? Não idempotente

```csharp
[HttpPost("{tenantId}")]
public async Task<IActionResult> CreateFolder(
    Guid tenantId,
    [FromBody] CreateFolderCommand command)
```

### PUT/PATCH - Atualizar Recurso
- ? **COM body**
- ? Dados de atualização no body
- ? PUT: substituição completa
- ? PATCH: atualização parcial

```csharp
[HttpPatch("{tenantId}/{folderId}")]
public async Task<IActionResult> UpdateFolder(
    Guid tenantId,
    Guid folderId,
    [FromBody] UpdateFolderCommand command)
```

### DELETE - Remover Recurso
- ?? **Geralmente SEM body**
- ? Parâmetros na query string
- ? ID na rota

```csharp
[HttpDelete("{tenantId}/{folderId}")]
public async Task<IActionResult> DeleteFolder(
    Guid tenantId,
    Guid folderId,
    [FromQuery] bool permanent = false)
```

## ?? Correções Aplicadas

### FoldersController

**Antes:**
```csharp
public async Task<IActionResult> GetFolders(Guid tenantId, GetFoldersQuery query)
```

**Depois:**
```csharp
public async Task<IActionResult> GetFolders(Guid tenantId, [FromQuery] GetFoldersQuery query)
```

## ?? Como Testar

### Teste Correto (GET com Query String)

```bash
curl -X GET "http://localhost:5000/api/folders/123?parentId=456&depth=2" \
  -H "Authorization: Bearer token"
```

### Teste Incorreto (GET com Body)

```bash
# ? Isso vai falhar
curl -X GET "http://localhost:5000/api/folders/123" \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{"parentId": "456", "depth": 2}'
```

## ?? Dicas

1. **Sempre use `[FromQuery]`** para objetos complexos em GET
2. **Evite bodies em GET** - use query string
3. **Se precisar de muitos parâmetros**, considere POST para pesquisa
4. **Use Swagger/OpenAPI** para documentar corretamente
5. **Valide com ferramentas** como Postman ou curl

## ?? Referências

- [RFC 7231 - HTTP/1.1 Semantics](https://tools.ietf.org/html/rfc7231#section-4.3.1)
- [ASP.NET Core Model Binding](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
- [HTTP Methods](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods)

## ? Status

- [x] Problema identificado
- [x] Correção aplicada em `FoldersController`
- [x] Build validado
- [x] Documentação criada
