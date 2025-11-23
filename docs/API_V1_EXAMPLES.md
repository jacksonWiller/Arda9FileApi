# Exemplos de Uso - API v1

## Autenticação

Todos os endpoints requerem um token JWT válido no header:

```http
Authorization: Bearer {accessToken}
```

O token deve conter o claim `tenantId` ou `CompanyId` com o ID da empresa.

---

## Arquivos

### 1. Listar Arquivos com Filtros

```http
GET /api/v1/files?page=1&limit=20&sortBy=name&order=asc&type=image
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "files": {
      "items": [
        {
          "id": "uuid",
          "name": "foto",
          "originalName": "foto.jpg",
          "extension": "jpg",
          "mimeType": "image/jpeg",
          "type": "image",
          "size": 1024000,
          "url": "https://s3.amazonaws.com/...",
          "folderId": "uuid",
          "createdAt": "2024-01-15T10:30:00Z"
        }
      ],
      "pagination": {
        "page": 1,
        "limit": 20,
        "total": 150,
        "totalPages": 8,
        "hasNext": true,
        "hasPrev": false
      }
    },
    "summary": {
      "totalSize": 153600000,
      "filesByType": {
        "image": 50,
        "document": 30,
        "video": 20
      }
    }
  }
}
```

### 2. Buscar Arquivo por ID

```http
GET /api/v1/files/{fileId}
```

### 3. Upload de Arquivo

```http
POST /api/v1/files/upload
Content-Type: multipart/form-data

--boundary
Content-Disposition: form-data; name="File"; filename="document.pdf"
Content-Type: application/pdf

[arquivo binário]
--boundary
Content-Disposition: form-data; name="BucketName"

my-bucket
--boundary
Content-Disposition: form-data; name="FolderId"

uuid-da-pasta
--boundary--
```

### 4. Atualizar Metadados

```http
PATCH /api/v1/files/{fileId}
Content-Type: application/json

{
  "name": "novo-nome",
  "folderId": "uuid-nova-pasta",
  "isPublic": true
}
```

### 5. Mover Arquivo

```http
POST /api/v1/files/{fileId}/move
Content-Type: application/json

{
  "folderId": "uuid-pasta-destino"
}
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "folderId": "uuid-pasta-destino"
  },
  "message": "Arquivo movido com sucesso"
}
```

### 6. Duplicar Arquivo

```http
POST /api/v1/files/{fileId}/duplicate
Content-Type: application/json

{
  "name": "Cópia do arquivo",
  "folderId": "uuid-pasta-destino"
}
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "id": "novo-uuid",
    "name": "Cópia do arquivo.pdf",
    "url": "https://s3.amazonaws.com/...",
    "size": 1024000
  },
  "message": "Arquivo duplicado com sucesso"
}
```

### 7. Obter URL de Download

```http
GET /api/v1/files/{fileId}/download
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "url": "https://s3.amazonaws.com/...",
    "expiresAt": "2024-01-15T11:30:00Z",
    "filename": "document.pdf",
    "size": 1024000
  }
}
```

### 8. Excluir Arquivo (Soft Delete)

```http
DELETE /api/v1/files/{fileId}
```

**Resposta:**
```json
{
  "success": true,
  "message": "Arquivo movido para lixeira"
}
```

### 9. Restaurar Arquivo

```http
POST /api/v1/files/{fileId}/restore
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "document.pdf",
    "folderId": "uuid",
    "restoredAt": "2024-01-15T10:45:00Z"
  },
  "message": "Arquivo restaurado com sucesso"
}
```

---

## Pastas

### 1. Listar Pastas (Raiz)

```http
GET /api/v1/folders?depth=2&includeEmpty=true
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "folders": [
      {
        "id": "uuid",
        "name": "Documentos",
        "parentId": null,
        "path": ["Documentos"],
        "depth": 0,
        "fileCount": 15,
        "folderCount": 3,
        "totalSize": 5242880,
        "permissions": {
          "canView": true,
          "canEdit": true,
          "canDelete": true,
          "canShare": true
        },
        "children": [
          {
            "id": "uuid-child",
            "name": "2024",
            "parentId": "uuid",
            "path": ["Documentos", "2024"],
            "depth": 1,
            "fileCount": 5,
            "folderCount": 0,
            "children": []
          }
        ],
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ]
  }
}
```

### 2. Listar Subpastas

```http
GET /api/v1/folders?parentId={folderId}&depth=1
```

### 3. Obter Detalhes de Pasta

```http
GET /api/v1/folders/{folderId}
```

### 4. Criar Pasta

```http
POST /api/v1/folders
Content-Type: application/json

{
  "folderName": "Nova Pasta",
  "bucketId": "uuid-bucket",
  "parentFolderId": "uuid-parent"
}
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "id": "novo-uuid",
    "folderName": "Nova Pasta",
    "bucketId": "uuid-bucket",
    "parentFolderId": "uuid-parent",
    "path": "parent/Nova Pasta",
    "createdAt": "2024-01-15T10:30:00Z"
  },
  "message": "Pasta criada com sucesso"
}
```

### 5. Atualizar Pasta

```http
PATCH /api/v1/folders/{folderId}
Content-Type: application/json

{
  "folderName": "Pasta Renomeada",
  "parentFolderId": "uuid-novo-parent"
}
```

### 6. Mover Pasta

```http
POST /api/v1/folders/{folderId}/move
Content-Type: application/json

{
  "parentId": "uuid-novo-parent"
}
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "parentId": "uuid-novo-parent",
    "path": ["parent", "Nova Pasta"]
  },
  "message": "Pasta movida com sucesso"
}
```

### 7. Excluir Pasta

```http
DELETE /api/v1/folders/{folderId}?recursive=false&permanent=false
```

**Resposta:**
```json
{
  "success": true,
  "message": "Pasta excluída com sucesso"
}
```

---

## Filtros Avançados - Arquivos

### Buscar por Tipo

```http
GET /api/v1/files?type=document
```

Tipos suportados: `image`, `video`, `document`, `audio`, `archive`, `code`, `other`

### Buscar por Extensão

```http
GET /api/v1/files?extension=pdf
```

### Buscar por Nome

```http
GET /api/v1/files?search=contrato
```

### Buscar por Tamanho

```http
GET /api/v1/files?minSize=1048576&maxSize=10485760
```

(Entre 1MB e 10MB)

### Buscar por Data

```http
GET /api/v1/files?fromDate=2024-01-01&toDate=2024-01-31
```

### Filtros Combinados

```http
GET /api/v1/files?type=image&extension=jpg&minSize=100000&sortBy=size&order=desc&page=1&limit=50
```

---

## Códigos de Status HTTP

- `200 OK` - Requisição bem-sucedida
- `201 Created` - Recurso criado com sucesso
- `400 Bad Request` - Dados inválidos
- `401 Unauthorized` - Token inválido ou ausente
- `403 Forbidden` - Sem permissão para acessar recurso
- `404 Not Found` - Recurso não encontrado
- `409 Conflict` - Conflito (ex: pasta já existe)
- `413 Payload Too Large` - Arquivo muito grande
- `415 Unsupported Media Type` - Tipo de arquivo não suportado
- `500 Internal Server Error` - Erro no servidor
- `507 Insufficient Storage` - Cota de armazenamento excedida

---

## Notas

1. **Paginação**: Máximo de 100 itens por página
2. **Autenticação**: Obrigatória em todos os endpoints
3. **Soft Delete**: Arquivos deletados ficam na lixeira por 30 dias
4. **Tenant Isolation**: Cada empresa acessa apenas seus próprios dados
5. **S3 Operations**: Movimentações físicas são executadas de forma assíncrona
