# API - Referência Rápida

## Base URL
```
/api
```

## Autenticação
Todos os endpoints requerem:
```http
Authorization: Bearer {token}
```

**Importante:** O `tenantId` deve ser passado na URL como parte do path.

---

## ?? Files

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/files/{tenantId}` | Lista arquivos (com filtros) |
| `POST` | `/files/{tenantId}` | Upload de arquivo |
| `GET` | `/files/{tenantId}/{fileId}` | Detalhes do arquivo |
| `GET` | `/files/{tenantId}/bucket/{bucketName}` | Arquivos por bucket |
| `GET` | `/files/{tenantId}/folder/{folderId}` | Arquivos por pasta |
| `GET` | `/files/{tenantId}/{fileId}/download` | Download direto |
| `GET` | `/files/{tenantId}/{fileId}/download-url` | URL de download |
| `PATCH` | `/files/{tenantId}/{fileId}` | Atualizar metadados |
| `DELETE` | `/files/{tenantId}/{fileId}` | Deletar (soft) |
| `POST` | `/files/{tenantId}/{fileId}/restore` | Restaurar arquivo |
| `POST` | `/files/{tenantId}/{fileId}/duplicate` | Duplicar arquivo |
| `POST` | `/files/{tenantId}/{fileId}/move` | Mover para pasta |

### Query Parameters (GET /files/{tenantId})
```
?page=1
&limit=50
&sortBy=createdAt|name|size|type
&order=asc|desc
&folderId=uuid
&type=image|video|document|audio|archive|code|other
&extension=pdf|jpg|mp4
&search=termo
&minSize=bytes
&maxSize=bytes
&fromDate=ISO8601
&toDate=ISO8601
```

---

## ?? Folders

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/folders/{tenantId}` | Lista pastas (hierárquica) |
| `POST` | `/folders/{tenantId}` | Criar pasta |
| `GET` | `/folders/{tenantId}/{folderId}` | Detalhes da pasta |
| `GET` | `/folders/{tenantId}/bucket/{bucketId}` | Pastas por bucket |
| `GET` | `/folders/{tenantId}/parent/{parentId}` | Subpastas |
| `PATCH` | `/folders/{tenantId}/{folderId}` | Atualizar pasta |
| `DELETE` | `/folders/{tenantId}/{folderId}` | Deletar pasta |
| `POST` | `/folders/{tenantId}/{folderId}/move` | Mover pasta |

### Query Parameters (GET /folders/{tenantId})
```
?parentId=uuid
&includeEmpty=true|false
&depth=1-5
```

---

## ?? Request Bodies

### Upload File
```http
POST /files/{tenantId}
Content-Type: multipart/form-data

File: [binary]
BucketName: "bucket-name"
FolderId: "uuid" (opcional)
```

### Update File
```json
PATCH /files/{tenantId}/{fileId}

{
  "name": "novo-nome",
  "folderId": "uuid",
  "isPublic": true
}
```

### Duplicate File
```json
POST /files/{tenantId}/{fileId}/duplicate

{
  "name": "Cópia",
  "folderId": "uuid"
}
```

### Move File
```json
POST /files/{tenantId}/{fileId}/move

{
  "folderId": "uuid"
}
```

### Create Folder
```json
POST /folders/{tenantId}

{
  "folderName": "Nova Pasta",
  "bucketId": "uuid",
  "parentFolderId": "uuid"
}
```

### Update Folder
```json
PATCH /folders/{tenantId}/{folderId}

{
  "folderName": "Renomeada",
  "parentFolderId": "uuid"
}
```

### Move Folder
```json
POST /folders/{tenantId}/{folderId}/move

{
  "parentId": "uuid"
}
```

---

## ?? Response Format

### Sucesso
```json
{
  "success": true,
  "data": { ... },
  "successMessage": "Operação concluída"
}
```

### Erro
```json
{
  "success": false,
  "statusCode": 400,
  "errors": [
    {
      "message": "Mensagem de erro"
    }
  ]
}
```

### Lista Paginada
```json
{
  "success": true,
  "data": {
    "files": {
      "items": [...],
      "pagination": {
        "page": 1,
        "limit": 50,
        "total": 150,
        "totalPages": 3,
        "hasNext": true,
        "hasPrev": false
      }
    },
    "summary": {
      "totalSize": 1024000,
      "filesByType": {
        "image": 10,
        "document": 5
      }
    }
  }
}
```

---

## ?? Status Codes

| Code | Significado |
|------|-------------|
| `200` | OK |
| `201` | Created |
| `400` | Bad Request |
| `401` | Unauthorized |
| `403` | Forbidden |
| `404` | Not Found |
| `409` | Conflict |
| `500` | Internal Server Error |

---

## ?? Tipos de Arquivo

```
image    ? jpg, png, gif, svg
video    ? mp4, avi, mov, webm
document ? pdf, doc, docx, xls, xlsx, txt
audio    ? mp3, wav, ogg, m4a
archive  ? zip, rar, 7z, tar, gz
code     ? js, ts, cs, py, java, cpp
other    ? demais extensões
```

---

## ?? Dicas

### TenantId
- **Sempre passado na URL** como primeiro parâmetro
- Exemplo: `/api/files/123e4567-e89b-12d3-a456-426614174000`

### Paginação
- Limite máximo: 100 itens
- Padrão: 50 itens

### Ordenação
- Campos: `name`, `createdAt`, `size`, `type`
- Direção: `asc`, `desc`

### Filtros
- Combine múltiplos filtros
- Case-insensitive na busca
- Filtros são aplicados com AND

---

## ?? Exemplos Rápidos

### Buscar PDFs maiores que 1MB
```http
GET /api/files/{tenantId}?extension=pdf&minSize=1048576
```

### Listar imagens recentes
```http
GET /api/files/{tenantId}?type=image&sortBy=createdAt&order=desc&limit=20
```

### Buscar em pasta específica
```http
GET /api/files/{tenantId}?folderId=uuid-da-pasta
```

### Estrutura completa de pastas (3 níveis)
```http
GET /api/folders/{tenantId}?depth=3&includeEmpty=false
```

### Subpastas de uma pasta
```http
GET /api/folders/{tenantId}?parentId=uuid-parent&depth=1
```

### Upload de arquivo
```http
POST /api/files/{tenantId}
Content-Type: multipart/form-data

File: [arquivo]
BucketName: "meu-bucket"
FolderId: "uuid-pasta"
```

### Mover arquivo entre pastas
```http
POST /api/files/{tenantId}/{fileId}/move
Content-Type: application/json

{
  "folderId": "uuid-nova-pasta"
}
```

### Duplicar arquivo
```http
POST /api/files/{tenantId}/{fileId}/duplicate
Content-Type: application/json

{
  "name": "Cópia do Documento",
  "folderId": "uuid-pasta-destino"
}
```

---

## ?? Performance

### Otimizações
- Use paginação adequada
- Limite depth em folders (max: 5)
- Cache de listagens frequentes
- Filtros reduzem payload

### Limites
- Upload: Configurado no servidor
- Paginação: 100 itens/request
- Depth: 5 níveis de pastas
- Query strings: Limite do HTTP

---

## ?? Mudanças da V1

**Os controllers V1 foram consolidados nos controllers principais.**

Mudanças de rota:
- ? `/api/v1/files` ? ? `/api/files/{tenantId}`
- ? `/api/v1/folders` ? ? `/api/folders/{tenantId}`

**Vantagens do novo padrão:**
- TenantId explícito na URL
- Melhor isolamento e segurança
- Mais fácil para logging e monitoramento
- Compatível com sistemas de API Gateway

---

Veja documentação completa em:
- `docs/API_V1_IMPLEMENTATION.md`
- `docs/API_V1_EXAMPLES.md`
- `docs/IMPLEMENTATION_SUMMARY.md`
