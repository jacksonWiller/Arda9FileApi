# Guia de Migração - V1 para API Principal

## ?? Resumo das Mudanças

Os controllers `FilesV1Controller` e `FoldersV1Controller` foram **removidos** e suas funcionalidades foram **consolidadas** nos controllers principais `FilesController` e `FoldersController`.

**Mudança Principal:** O `tenantId` agora é **obrigatório na URL** em vez de ser extraído dos claims do JWT.

---

## ?? Mapeamento de Rotas

### Arquivos (Files)

| V1 (REMOVIDO) | Novo (ATUAL) | Método |
|---------------|--------------|--------|
| `/api/v1/files` | `/api/files/{tenantId}` | GET |
| `/api/v1/files/upload` | `/api/files/{tenantId}` | POST |
| `/api/v1/files/{fileId}` | `/api/files/{tenantId}/{fileId}` | GET |
| `/api/v1/files/{fileId}` | `/api/files/{tenantId}/{fileId}` | PATCH |
| `/api/v1/files/{fileId}` | `/api/files/{tenantId}/{fileId}` | DELETE |
| `/api/v1/files/{fileId}/restore` | `/api/files/{tenantId}/{fileId}/restore` | POST |
| `/api/v1/files/{fileId}/duplicate` | `/api/files/{tenantId}/{fileId}/duplicate` | POST |
| `/api/v1/files/{fileId}/move` | `/api/files/{tenantId}/{fileId}/move` | POST |
| `/api/v1/files/{fileId}/download` | `/api/files/{tenantId}/{fileId}/download` | GET |
| N/A | `/api/files/{tenantId}/{fileId}/download-url` | GET |
| N/A | `/api/files/{tenantId}/bucket/{bucketName}` | GET |
| N/A | `/api/files/{tenantId}/folder/{folderId}` | GET |

### Pastas (Folders)

| V1 (REMOVIDO) | Novo (ATUAL) | Método |
|---------------|--------------|--------|
| `/api/v1/folders` | `/api/folders/{tenantId}` | GET |
| `/api/v1/folders` | `/api/folders/{tenantId}` | POST |
| `/api/v1/folders/{folderId}` | `/api/folders/{tenantId}/{folderId}` | GET |
| `/api/v1/folders/{folderId}` | `/api/folders/{tenantId}/{folderId}` | PATCH |
| `/api/v1/folders/{folderId}` | `/api/folders/{tenantId}/{folderId}` | DELETE |
| `/api/v1/folders/{folderId}/move` | `/api/folders/{tenantId}/{folderId}/move` | POST |
| N/A | `/api/folders/{tenantId}/bucket/{bucketId}` | GET |
| N/A | `/api/folders/{tenantId}/parent/{parentId}` | GET |

---

## ?? Exemplos de Migração

### Exemplo 1: Listar Arquivos

**Antes (V1):**
```javascript
// JavaScript/TypeScript
const response = await fetch('/api/v1/files?page=1&limit=20', {
  headers: {
    'Authorization': `Bearer ${token}` // token contém tenantId em claim
  }
});
```

**Agora:**
```javascript
// JavaScript/TypeScript
const tenantId = 'your-tenant-id';
const response = await fetch(`/api/files/${tenantId}?page=1&limit=20`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

### Exemplo 2: Upload de Arquivo

**Antes (V1):**
```javascript
const formData = new FormData();
formData.append('File', file);
formData.append('BucketName', 'my-bucket');

const response = await fetch('/api/v1/files/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});
```

**Agora:**
```javascript
const tenantId = 'your-tenant-id';
const formData = new FormData();
formData.append('File', file);
formData.append('BucketName', 'my-bucket');

const response = await fetch(`/api/files/${tenantId}`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});
```

### Exemplo 3: Mover Arquivo

**Antes (V1):**
```javascript
const fileId = 'file-uuid';
const response = await fetch(`/api/v1/files/${fileId}/move`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    folderId: 'target-folder-uuid'
  })
});
```

**Agora:**
```javascript
const tenantId = 'your-tenant-id';
const fileId = 'file-uuid';
const response = await fetch(`/api/files/${tenantId}/${fileId}/move`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    folderId: 'target-folder-uuid'
  })
});
```

### Exemplo 4: Criar Pasta

**Antes (V1):**
```javascript
const response = await fetch('/api/v1/folders', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    folderName: 'Nova Pasta',
    bucketId: 'bucket-uuid',
    parentFolderId: 'parent-uuid'
  })
});
```

**Agora:**
```javascript
const tenantId = 'your-tenant-id';
const response = await fetch(`/api/folders/${tenantId}`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    folderName: 'Nova Pasta',
    bucketId: 'bucket-uuid',
    parentFolderId: 'parent-uuid'
  })
});
```

---

## ?? Como Obter o TenantId

### Opção 1: Armazenar no Cliente
```javascript
// Após login, armazene o tenantId
localStorage.setItem('tenantId', user.tenantId);

// Use em requisições
const tenantId = localStorage.getItem('tenantId');
const response = await fetch(`/api/files/${tenantId}`, {...});
```

### Opção 2: Extrair do Token JWT
```javascript
// Decodificar JWT no cliente (apenas para leitura, não para validação!)
function parseJwt(token) {
  const base64Url = token.split('.')[1];
  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  const jsonPayload = decodeURIComponent(
    atob(base64).split('').map(c => 
      '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    ).join('')
  );
  return JSON.parse(jsonPayload);
}

const token = 'your-jwt-token';
const payload = parseJwt(token);
const tenantId = payload.tenantId || payload.CompanyId;
```

### Opção 3: Endpoint de UserInfo
```javascript
// Criar endpoint que retorna tenantId do usuário autenticado
const response = await fetch('/api/auth/userinfo', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
const { tenantId } = await response.json();
```

---

## ?? Atualização de SDKs/Clients

### Client TypeScript/JavaScript

**Antes:**
```typescript
class FilesApiClient {
  constructor(private baseUrl: string, private token: string) {}

  async listFiles(params: ListFilesParams) {
    return fetch(`${this.baseUrl}/api/v1/files?${new URLSearchParams(params)}`, {
      headers: { 'Authorization': `Bearer ${this.token}` }
    });
  }
}
```

**Agora:**
```typescript
class FilesApiClient {
  constructor(
    private baseUrl: string, 
    private token: string,
    private tenantId: string  // NOVO PARÂMETRO
  ) {}

  async listFiles(params: ListFilesParams) {
    return fetch(
      `${this.baseUrl}/api/files/${this.tenantId}?${new URLSearchParams(params)}`, 
      {
        headers: { 'Authorization': `Bearer ${this.token}` }
      }
    );
  }
}

// Uso
const client = new FilesApiClient(
  'https://api.example.com',
  userToken,
  userTenantId  // Passar tenantId na construção
);
```

### Client C#

**Antes:**
```csharp
public class FilesApiClient
{
    private readonly HttpClient _httpClient;
    
    public FilesApiClient(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<FilesResponse> ListFilesAsync(ListFilesParams parameters)
    {
        var response = await _httpClient.GetAsync("/api/v1/files");
        return await response.Content.ReadFromJsonAsync<FilesResponse>();
    }
}
```

**Agora:**
```csharp
public class FilesApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Guid _tenantId;  // NOVO CAMPO
    
    public FilesApiClient(HttpClient httpClient, string token, Guid tenantId)
    {
        _httpClient = httpClient;
        _tenantId = tenantId;  // Armazenar tenantId
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<FilesResponse> ListFilesAsync(ListFilesParams parameters)
    {
        var response = await _httpClient.GetAsync($"/api/files/{_tenantId}");
        return await response.Content.ReadFromJsonAsync<FilesResponse>();
    }
}

// Uso
var client = new FilesApiClient(httpClient, userToken, userTenantId);
```

---

## ? Checklist de Migração

- [ ] Identificar todas as chamadas à API V1
- [ ] Obter/armazenar tenantId no cliente
- [ ] Atualizar todas as URLs de `/api/v1/{resource}` para `/api/{resource}/{tenantId}`
- [ ] Atualizar clients/SDKs para incluir tenantId
- [ ] Atualizar testes automatizados
- [ ] Atualizar documentação interna
- [ ] Testar em ambiente de desenvolvimento
- [ ] Testar em ambiente de staging
- [ ] Deploy em produção
- [ ] Monitorar logs por erros 404

---

## ?? Troubleshooting

### Erro 404 - Not Found
**Problema:** Rota antiga sendo chamada
```
GET /api/v1/files ? 404
```

**Solução:** Adicionar tenantId na URL
```
GET /api/files/{tenantId} ? 200
```

### Erro 401 - Unauthorized
**Problema:** Token JWT não enviado ou inválido

**Solução:** Verificar se header `Authorization` está correto
```javascript
headers: {
  'Authorization': `Bearer ${token}`  // Bearer + espaço + token
}
```

### Erro 403 - Forbidden
**Problema:** TenantId na URL não corresponde ao usuário

**Solução:** Verificar se o tenantId usado na URL pertence ao usuário autenticado

---

## ?? Benefícios da Nova Abordagem

1. **Segurança Explícita**
   - TenantId visível na URL facilita auditoria
   - Impossível esquecer de validar tenant

2. **Performance**
   - Não precisa parsear JWT em cada request
   - TenantId já disponível no routing

3. **Debugging**
   - Logs incluem tenantId automaticamente
   - Fácil rastrear requisições por tenant

4. **API Gateway**
   - Rate limiting por tenant
   - Routing baseado em tenant
   - Métricas por tenant

5. **Developer Experience**
   - URLs mais explícitas e autodocumentadas
   - Swagger mostra parâmetros claramente
   - Testes mais fáceis (não precisa mock JWT)

---

## ?? Suporte

Se encontrar problemas durante a migração:

1. Verifique a documentação atualizada em `docs/API_V1_QUICK_REFERENCE.md`
2. Consulte exemplos em `docs/API_V1_EXAMPLES.md`
3. Revise este guia de migração
4. Entre em contato com a equipe de desenvolvimento

---

**Data de Migração:** Dezembro 2024  
**Versão:** 1.0 ? 2.0 (Consolidada)  
**Status:** ? Completo
