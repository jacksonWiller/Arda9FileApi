# Problema: Internal Server Error após dividir em camadas

## Contexto
Após dividir a aplicação em camadas (Core, Domain, Application, Infra, Api), o comando `sam build` compilava com sucesso, mas ao fazer deploy na AWS Lambda, a API retornava erro `{"message":"Internal Server Error"}`.

## Causas do Problema

### 1. **Dependências Duplicadas nos Projetos**
Todos os projetos tinham as mesmas dependências instaladas, causando conflitos e aumentando o tamanho do pacote desnecessariamente.

**Problema:** Cada camada tinha todos os pacotes NuGet, incluindo `AWSSDK.*`, `MediatR`, `FluentValidation`, etc.

### 2. **Handler Incorreto no template.yaml**
O `Handler` no SAM template estava como `Arda9FileApi`, mas após a divisão em camadas, o namespace e o assembly name mudaram.

**Problema:** 
```yaml
Handler: Arda9FileApi  # ? Incorreto
```

**Solução:**
```yaml
Handler: Arda9File.Api  # ? Correto - corresponde ao AssemblyName
```

### 3. **Falta de AssemblyName Explícito**
O projeto `Arda9File.Api` não tinha o `AssemblyName` definido explicitamente, podendo causar inconsistências.

### 4. **Dependências AWS SDK Faltantes nas Camadas Corretas**
- **Core:** Precisava de `AWSSDK.DynamoDBv2` para atributos de mapeamento
- **Application:** Precisava de `AWSSDK.S3` para serviços S3
- **Infra:** Precisava de todos os SDKs AWS usados em repositórios

## Solução Implementada

### 1. Estrutura de Dependências Correta

#### **Arda9File.Core** (Abstrações e Modelos Base)
```xml
<PackageReference Include="Ardalis.Result" Version="8.0.0" />
<PackageReference Include="Ardalis.Result.FluentValidation" Version="4.1.0" />
<PackageReference Include="AWSSDK.DynamoDBv2" Version="3.7.3.7" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="MediatR" Version="13.1.0" />
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Core" Version="2.2.5" />
```

#### **Arda9File.Domain** (Modelos e Interfaces)
```xml
<PackageReference Include="AWSSDK.DynamoDBv2" Version="3.7.3.7" />
<ProjectReference Include="..\Arda9File.Core\Arda9File.Core.csproj" />
```

#### **Arda9File.Application** (Casos de Uso e Serviços)
```xml
<PackageReference Include="Ardalis.Result" Version="8.0.0" />
<PackageReference Include="Ardalis.Result.FluentValidation" Version="4.1.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AWSSDK.S3" Version="3.7.504.1" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
<PackageReference Include="MediatR" Version="13.1.0" />
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
<ProjectReference Include="..\Arda9File.Core\Arda9File.Core.csproj" />
<ProjectReference Include="..\Arda9File.Domain\Arda9File.Domain.csproj" />
```

#### **Arda9File.Infra** (Implementação de Repositórios)
```xml
<PackageReference Include="AWSSDK.DynamoDBv2" Version="3.7.3.7" />
<PackageReference Include="AWSSDK.S3" Version="3.7.504.1" />
<PackageReference Include="AWSSDK.S3Control" Version="3.7.503.5" />
<PackageReference Include="AWSSDK.CognitoIdentityProvider" Version="3.7.103.27" />
<PackageReference Include="AWSSDK.SecretsManager" Version="3.7.103.69" />
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
<ProjectReference Include="..\Arda9File.Domain\Arda9File.Domain.csproj" />
```

#### **Arda9File.Api** (Entry Point)
Mantém todos os pacotes necessários + referências a todos os projetos.

### 2. Correção do template.yaml

```yaml
Resources:
  NetCodeWebAPIServerless:
    Type: AWS::Serverless::Function
    Properties:
      Description: A simple example includes a .NET Core WebAPI App with DynamoDB table.
      CodeUri: ./src/Arda9File.Api/
      Handler: Arda9File.Api  # ? Nome correto do assembly
      Runtime: dotnet8
      MemorySize: 1024
      # ... rest of config
```

### 3. AssemblyName Explícito

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <AssemblyName>Arda9File.Api</AssemblyName>
  <AWSProjectType>Lambda</AWSProjectType>
  <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

## Como Testar

1. **Build Local:**
   ```bash
   dotnet build
   ```

2. **SAM Build:**
   ```bash
   sam build
   ```

3. **SAM Local:**
   ```bash
   sam local start-api
   ```

4. **Deploy:**
   ```bash
   sam deploy --guided
   ```

## Princípios de Arquitetura em Camadas para Lambda

### ? Boas Práticas

1. **Separação de Responsabilidades:**
   - **Core:** Abstrações, interfaces, modelos base
   - **Domain:** Entidades, Value Objects, Interfaces de repositórios
   - **Application:** Casos de uso, DTOs, Commands/Queries (CQRS)
   - **Infra:** Implementação de repositórios, acesso a dados
   - **Api:** Controllers, Program.cs, configuração

2. **Dependências Mínimas:**
   - Cada camada deve ter apenas as dependências necessárias
   - Evitar duplicação de pacotes entre camadas
   - Use `ProjectReference` ao invés de duplicar pacotes

3. **Handler Configuration:**
   - O `Handler` no SAM template deve corresponder ao `AssemblyName`
   - Use `AssemblyName` explícito no .csproj
   - Para projetos executáveis (.NET 6+), use apenas o nome do assembly

4. **Copy Dependencies:**
   - `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` garante que todas as dependências sejam copiadas
   - Essencial para Lambda funcionar corretamente

### ? Erros Comuns

1. ? Duplicar todos os pacotes em todos os projetos
2. ? Usar namespaces inconsistentes (Arda9FileApi vs Arda9File.Api)
3. ? Handler apontando para namespace errado
4. ? Não definir `AssemblyName` explicitamente
5. ? Esquecer `CopyLocalLockFileAssemblies` no projeto Lambda

## Resultado

Após as correções:
- ? Compilação bem-sucedida
- ? `sam build` funciona corretamente
- ? Deploy no Lambda funcional
- ? API responde corretamente (não mais Internal Server Error)
- ? Tamanho do pacote otimizado
- ? Estrutura de código organizada e manutenível
