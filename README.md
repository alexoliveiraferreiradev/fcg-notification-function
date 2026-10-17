# Fiap Cloud Games (FCG) - Notification Function

Azure Function (isolated worker, .NET 9) responsável pelo **envio simulado de e-mails** transacionais da plataforma **Fiap Cloud Games (FCG)**. O serviço é orientado a eventos: consome mensagens do **Azure Service Bus** publicadas pelos demais microsserviços (Usuários, Pagamentos e Pedidos) e as processa via `ServiceBusTrigger`, persistindo o histórico de notificações em SQL Server e controlando idempotência via Redis.

---

## 🛠️ Tecnologias e Bibliotecas

- **.NET 9.0** / **Azure Functions Worker (isolated) v4**
- **Azure Service Bus** (`Microsoft.Azure.Functions.Worker.Extensions.ServiceBus`): trigger de mensageria para consumo assíncrono de eventos (tópicos + subscriptions)
- **Entity Framework Core + SQL Server**: persistência do histórico de notificações
- **Redis**: controle de idempotência de mensagens (`IdempotencyBehavior` via pipeline do MediatR)
- **MediatR**: orquestração dos casos de uso (CQRS)
- **xUnit**: testes automatizados

---

## 🏗️ Arquitetura da Solução

O projeto segue os princípios de **Clean Architecture**:

```
src/
├── Fcg.Notification.Function.Domain          # Entidades, enums e contratos de repositório
├── Fcg.Notification.Function.Application     # Casos de uso (Commands/Handlers) e pipeline de idempotência
├── Fcg.Notification.Function.Infrastructure  # EF Core, Redis, Service Bus (topologia) e EmailService
└── Fcg.Notification.Functions                # Host da Azure Function (Program.cs e ServiceBusTriggers)
```

---

## 📐 Fluxo de Integração e Eventos Consumidos

Os microsserviços publicam os eventos com MassTransit em **tópicos com nome fixo** (definidos via `SetEntityName` em cada API). A function escuta cada tópico pela subscription `notification`. Os nomes dos tópicos e da subscription vêm da classe `Topics` do pacote `Fcg.Core.SharedContracts` (namespace `Fcg.Core.SharedContracts.Messaging`), compartilhada com as APIs:

| Function | Tópico / Subscription | Evento consumido | Publicado por |
|---|---|---|---|
| `SendWelcomeFunction` | `user-created` / `notification` | `IUserCreatedIntegrationEvent` (cadastro de usuário) | Usuários |
| `SyncRepublishedUserFunction` | `user-republished` / `notification` | `IRepublishUserIntegrationEvent` (sincronização de usuário, sem e-mail) | Usuários |
| `SendApprovedPaymentEmailFunction` | `payment-approved` / `notification` | `IPaymentApprovedIntegrationEvent` (pagamento aprovado) | Pagamentos |
| `SendFailedPaymentEmailFunction` | `purchase-rejected` / `notification` | `IPurchaseRejectedIntegrationEvent` (pagamento recusado) | Pagamentos |
| `SendDeliveryEmailFunction` | `delivery-failed` / `notification` | `IDeliveryFailedIntegrationEvent` (falha ao liberar o jogo) | Pedidos |

> ⚠️ O `ServiceBusTrigger` **não cria** tópicos nem subscriptions. Antes de subir a function, crie-os com o script [`scripts/create-servicebus-subscriptions.sh`](scripts/create-servicebus-subscriptions.sh) (requer [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) autenticada):
> ```bash
> ./scripts/create-servicebus-subscriptions.sh <resource-group> <namespace>
> ```
> Mensagens publicadas antes da subscription existir não são entregues à function.

Mensagens duplicadas são descartadas pelo `IdempotencyBehavior`, que usa o Redis para registrar o `EventId` já processado. Em caso de erro, o Service Bus reentrega a mensagem até o `max-delivery-count` da subscription e depois a move para a dead-letter queue da própria subscription. O envio de e-mail em si é apenas **simulado**: o `EmailService` loga os dados no console em vez de integrar com um provedor real.

---

## ⚙️ Configuração e Variáveis de Ambiente

Nenhum segredo é versionado. A configuração local fica dividida em dois lugares:

### `local.settings.json` (host do Functions)

O arquivo **não é versionado** (está no `.gitignore`). Crie-o a partir do exemplo:

```bash
cp src/Fcg.Notification.Functions/local.settings.example.json src/Fcg.Notification.Functions/local.settings.json
```

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "<connection-string-do-service-bus>"
  }
}
```

`ServiceBusConnection` é a connection usada pelos `ServiceBusTrigger` e precisa ficar aqui (ou em uma variável de ambiente com o mesmo nome): os triggers são resolvidos pelo host do Functions, que **não** lê o User Secrets.

### User Secrets (worker)

As demais configurações são lidas pelo worker via [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), carregados quando `AZURE_FUNCTIONS_ENVIRONMENT=Development` (padrão ao rodar com `func start` ou pelo Visual Studio). A partir de `src/Fcg.Notification.Functions`:

```bash
dotnet user-secrets set "AzureDatabaseSettings:ConnectionString" "Server=localhost,1433;Database=Fcg_Notifications;User Id=<usuario-sql>;Password=<senha-sql>;TrustServerCertificate=True"
dotnet user-secrets set "RedisSettings:Host" "localhost"
dotnet user-secrets set "RedisSettings:Port" "6379"
dotnet user-secrets set "RedisSettings:Password" "<senha-redis>"
dotnet user-secrets set "RedisSettings:InstanceName" "FiapCloudGames:"
```

| Chave | Uso |
|---|---|
| `AzureDatabaseSettings:ConnectionString` | Connection string do SQL Server do histórico de notificações, com autenticação por usuário e senha (também usada pelo `dotnet ef` em design-time) |
| `RedisSettings:*` | Redis do controle de idempotência |

---

## 🚀 Como Executar Localmente

> ⚠️ A function roda na máquina do desenvolvedor (via `func start` ou Visual Studio). A mensageria usa um namespace do **Azure Service Bus** na nuvem; SQL Server e Redis podem estar em containers Docker soltos/Docker Compose ou em um cluster Kubernetes local.

### Pré-requisitos
- [SDK do .NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (emulador de Storage exigido pelo runtime do Functions) — pode ser iniciado via `azurite` (npm) ou automaticamente pelo Visual Studio
- Um namespace do **Azure Service Bus** (tier Standard ou superior, necessário para tópicos) com os tópicos/subscriptions criados (veja [Fluxo de Integração](#-fluxo-de-integração-e-eventos-consumidos))
- Para SQL Server e Redis, escolha **uma** das opções abaixo:
  - **Opção A — Docker:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) ou Docker Engine (com Docker Compose, se for usar essa variante)
  - **Opção B — Kubernetes:** um cluster local ([Minikube](https://minikube.sigs.k8s.io/), Docker Desktop Kubernetes, Kind, k3d) com `kubectl` configurado

Em ambos os casos, a function roda **fora** do cluster/containers, então o que muda entre as opções é apenas o host/porta configurados no User Secrets (o `Server` da connection string e o `RedisSettings:Host`/`Port`).

### 1. Subir a infraestrutura local

#### Opção A — Docker

**Via containers avulsos (`docker run`):**
```bash
docker run -d --name sqlserver-dev -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<senha-sql>" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```
```bash
docker run -d --name redis-dev -p 6379:6379 redis:7-alpine redis-server --requirepass <senha-redis>
```

**Via Docker Compose (reaproveitando a stack do [`fcg-infrastructure`](../fcg-infrastructure)):**
```bash
docker compose up -d fcg-db-central redis
```
> Rode o comando a partir da raiz do repositório `fcg-infrastructure` (`.env` já configurado — veja o README daquele repositório). Ele sobe apenas os serviços de infraestrutura necessários pela notification function, sem subir as demais APIs.

Nas duas variantes, os containers publicam as portas diretamente no host, então o `Server` da `AzureDatabaseSettings:ConnectionString` fica como `localhost,1433` e o `RedisSettings:Host` como `localhost`, com a porta padrão `6379`.

#### Opção B — Kubernetes

Os manifestos de infraestrutura (SQL Server e Redis) vivem no repositório [`fcg-infrastructure`](../fcg-infrastructure), em `k8s/`. A partir da raiz daquele repositório (com os Secrets já aplicados — veja o README daquele repositório):

```bash
kubectl apply -f k8s/configs/
kubectl apply -f k8s/infra/sql.yaml -f k8s/infra/redis.yaml
```

Aguarde os pods `fcg-db-central` e `db-redis` ficarem `Running`:
```bash
kubectl get pods -w
```

Os Services `sql-service` e `redis-service` são do tipo `ClusterIP`. Como a function roda fora do cluster, exponha-os com `kubectl port-forward`, em um terminal separado para cada um (mantenha rodando enquanto usa a function):
```bash
kubectl port-forward svc/sql-service 1433:1433
```
```bash
kubectl port-forward svc/redis-service 6379:6379
```
Como o `port-forward` escuta em `localhost`, o `Server` da `AzureDatabaseSettings:ConnectionString` continua como `localhost,1433` e o `RedisSettings:Host` como `localhost`. As credenciais (`User Id`/`Password` do SQL e senha do Redis) são as definidas nos Secrets do cluster.

### 2. Aplicar as migrations no banco (Update-Database)

O banco `Fcg_Notifications` precisa existir e estar com o schema atualizado **antes** de rodar a function. Execute a partir da raiz do repositório:

```bash
dotnet ef database update --project src/Fcg.Notification.Function.Infrastructure --startup-project src/Fcg.Notification.Functions
```

Ou, pelo Package Manager Console do Visual Studio:
```powershell
Update-Database -Project Fcg.Notification.Function.Infrastructure -StartupProject Fcg.Notification.Functions
```

> A própria function também tenta aplicar migrations pendentes ao subir (`NotificationSeed.ApplyMigrationsAsync`), mas rodar o `Update-Database` antes evita falhas de inicialização caso o banco ainda não exista.

### 3. Restaurar, compilar e executar

```bash
dotnet restore
dotnet build
func start --csharp src/Fcg.Notification.Functions
```

Ou abra a solução `Fcg.Notification.Function.slnx` no Visual Studio e execute com F5 (projeto de inicialização: `Fcg.Notification.Functions`).

Ao subir, a function passa a consumir os eventos das subscriptions listadas acima, logando no console a simulação de envio de e-mail.

---

## 🧪 Testes Automatizados

Para rodar os testes unitários (Domain e Application), execute a partir do diretório raiz:
```bash
dotnet test
```
