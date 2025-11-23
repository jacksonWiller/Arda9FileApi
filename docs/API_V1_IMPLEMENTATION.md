# API v1 - Endpoints Implementados

## Arquivos (Files)

### Implementados ?

#### 3.1 GET `/api/v1/files`
- Lista arquivos do usuário com filtros e paginação
- **Handler**: `GetFilesQueryHandler`
- **Recursos**:
  - Paginação (page, limit)
  - Ordenação (sortBy, order)
  - Filtros: folderId, type, extension, search, size, date range
  - Resumo com totais por tipo

#### 3.2 GET `/api/v1/files/{fileId}`
- Retorna detalhes completos de um arquivo
- **Handler**: `GetFileByIdQueryHandler` (existente)

#### 3.3 POST `/api/v1/files/upload`
- Upload de arquivos
- **Handler**: `UploadFileCommandHandler` (existente)

#### 3.8 PATCH `/api/v1/files/{fileId}`
- Atualiza informações do arquivo
- **Handler**: `UpdateFileCommandHandler` (existente)

#### 3.9 DELETE `/api/v1/files/{fileId}`
- Move arquivo para lixeira (soft delete)
- **Handler**: `DeleteFileCommandHandler` (existente)

#### 3.10 POST `/api/v1/files/{fileId}/restore`
- Restaura arquivo da lixeira
- **Handler**: `RestoreFileCommandHandler` ? NOVO

#### 3.11 POST `/api/v1/files/{fileId}/duplicate`
- Cria uma cópia do arquivo
- **Handler**: `DuplicateFileCommandHandler` ? NOVO
- **Recursos**:
  - Copia arquivo físico no S3
  - Permite especificar novo nome e pasta

#### 3.12 POST `/api/v1/files/{fileId}/move`
- Move arquivo para outra pasta
- **Handler**: `MoveFileCommandHandler` ? NOVO
- **Recursos**:
  - Move arquivo físico no S3
  - Atualiza metadados

#### 3.14 GET `/api/v1/files/{fileId}/download`
- Retorna URL para download
- **Handler**: `GetFileDownloadUrlQueryHandler` ? NOVO

### Pendentes (Não Implementados) ?

#### 3.4 POST `/api/v1/files/upload/chunked`
- Upload chunked para arquivos grandes
- **Status**: Não implementado (requer infraestrutura adicional)

#### 3.5 PUT `/api/v1/files/upload/chunked/{uploadId}/chunk/{chunkIndex}`
- Upload de chunk específico
- **Status**: Não implementado

#### 3.6 POST `/api/v1/files/upload/chunked/{uploadId}/complete`
- Finaliza upload chunked
- **Status**: Não implementado

#### 3.7 DELETE `/api/v1/files/upload/chunked/{uploadId}`
- Cancela upload chunked
- **Status**: Não implementado

#### 3.13 POST `/api/v1/files/{fileId}/favorite`
- Adiciona/remove arquivo dos favoritos
- **Status**: Não implementado (requer adicionar campo `IsFavorite` ao DTO)

#### 3.15 GET `/api/v1/files/{fileId}/preview`
- Gera preview do arquivo
- **Status**: Não implementado (requer processamento de imagens/documentos)

#### 3.16 POST `/api/v1/files/bulk`
- Executa ações em múltiplos arquivos
- **Status**: Não implementado

#### 3.17 POST `/api/v1/files/{fileId}/convert`
- Converte arquivo para outro formato
- **Status**: Não implementado (requer serviço de conversão)

#### 3.18 GET `/api/v1/files/tasks/{taskId}`
- Verifica status de processamento/conversão
- **Status**: Não implementado

---

## Pastas (Folders)

### Implementados ?

#### 4.1 GET `/api/v1/folders`
- Lista todas as pastas do usuário
- **Handler**: `GetFoldersQueryHandler` ? NOVO
- **Recursos**:
  - Filtro por parentId
  - Controle de profundidade (depth)
  - Incluir/excluir pastas vazias
  - Estrutura hierárquica com children

#### 4.2 GET `/api/v1/folders/{folderId}`
- Retorna detalhes de uma pasta específica
- **Handler**: `GetFolderByIdQueryHandler` (existente)

#### 4.3 POST `/api/v1/folders`
- Cria nova pasta
- **Handler**: `CreateFolderCommandHandler` (existente)

#### 4.4 PATCH `/api/v1/folders/{folderId}`
- Atualiza informações da pasta
- **Handler**: `UpdateFolderCommandHandler` (existente)

#### 4.5 DELETE `/api/v1/folders/{folderId}`
- Exclui pasta e seu conteúdo
- **Handler**: `DeleteFolderCommandHandler` (existente)

#### 4.6 POST `/api/v1/folders/{folderId}/move`
- Move pasta para outro local
- **Handler**: `MoveFolderCommandHandler` ? NOVO
- **Recursos**:
  - Validação de referência circular
  - Atualização de paths

---

## Modelos Criados

### Common Models
- `PaginationQuery` - Query base para paginação
- `PaginatedResult<T>` - Resposta paginada genérica
- `PaginationMetadata` - Metadados de paginação

### File Models
- `GetFilesQuery` - Query com filtros avançados
- `GetFilesResponse` - Resposta com arquivos paginados e resumo
- `FileDetailDto` - DTO detalhado de arquivo
- `FilesSummary` - Resumo estatístico dos arquivos

### Folder Models
- `GetFoldersQuery` - Query para listagem de pastas
- `GetFoldersResponse` - Resposta com estrutura hierárquica
- `FolderDetailDto` - DTO detalhado de pasta com children

---

## Controllers

### FilesV1Controller ? NOVO
- Endpoints REST completos seguindo especificação API v1
- Autenticação via JWT
- Extração automática de TenantId dos claims

### FoldersV1Controller ? NOVO
- Endpoints REST completos seguindo especificação API v1
- Autenticação via JWT
- Suporte a operações hierárquicas

---

## Observações Técnicas

### Autenticação
- Todos os endpoints requerem autenticação (`[Authorize]`)
- TenantId extraído automaticamente dos claims JWT
- Claims suportados: `"tenantId"` ou `"CompanyId"`

### Validações
- Validação de permissões por tenant
- Verificação de soft delete
- Validação de referências circulares (pastas)
- Limites de paginação (máx: 100 itens)

### S3 Operations
- Upload/download de arquivos
- Movimentação física de arquivos ao mover/duplicar
- Suporte a URLs públicas e privadas

### DynamoDB
- Uso de GSIs para queries eficientes
- Single table design mantido
- Soft delete implementado

---

## Próximos Passos Sugeridos

1. **Upload Chunked**: Implementar para suportar arquivos grandes (>100MB)
2. **Favoritos**: Adicionar campo `IsFavorite` ao `FileMetadataDto`
3. **Tags**: Implementar sistema de tags (campo já existe mas não é usado)
4. **Compartilhamento**: Implementar sistema de permissões e compartilhamento
5. **Preview**: Integrar serviço de geração de thumbnails/previews
6. **Bulk Operations**: Implementar operações em lote
7. **File Conversion**: Integrar serviço de conversão de arquivos
8. **Virus Scan**: Integrar antivírus para scan de uploads
9. **Versioning**: Implementar versionamento de arquivos
10. **Activity Log**: Implementar log de atividades
