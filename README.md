# Fiap Cloud Games (FCG) - Notification Function

Azure Function (isolated worker, .NET 9) responsável pelo **envio simulado de e-mails** transacionais da plataforma **Fiap Cloud Games (FCG)**. O serviço é orientado a eventos: consome mensagens do RabbitMQ publicadas pelos demais microsserviços (Usuários, Pagamentos e Catálogo) e as processa via `RabbitMQTrigger`, persistindo o histórico de notificações em SQL Server e controlando idempotência via Redis.

---

## 🛠️ Tecnologias e Bibliotecas

- **.NET 9.0** / **Azure Functions Worker (isolated) v4**
- **RabbitMQ** (`Microsoft.Azure.Functions.Worker.Extensions.RabbitMQ`): trigger de mensageria para consumo assíncrono de eventos
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
├── Fcg.Notification.Function.Infrastructure  # EF Core, Redis, RabbitMQ (conexão e topologia) e EmailService
└── Fcg.Notification.Functions                # Host da Azure Function (Program.cs e RabbitMQTriggers)
```

---

## 📐 Fluxo de Integração e Eventos Consumidos

Cada evento é consumido de uma fila dedicada declarada em `NotificationRabbitTopology`, com exchange do tipo *fanout* e uma fila de dead-letter (`-dlq`) associada:

| Function | Fila | Evento consumido |
|---|---|---|
| `SendWelcomeFunction` | `notification-user-created` | `UserCreatedIntegrationEvent` (cadastro de usuário) |
| `SendApprovedPaymentEmailFunction` | `notification-payment-processed` | `PaymentProcessedIntegrationEvent` (pagamento aprovado) |
| `SendFailedPaymentEmailFunction` | `notification-payment-failed` | `PaymentFailedIntegrationEvent` (pagamento recusado) |
| `SendDeliveryEmailFunction` | `notification-delivery-failed` | `DeliveryFailedIntegrationEvent` (falha ao liberar o jogo) |

A topologia (exchanges, filas e dead-letter queues) é declarada automaticamente pela própria aplicação ao subir (`NotificationRabbitTopology.DeclareAllAsync`), não sendo necessário criá-la manualmente no RabbitMQ.

Mensagens duplicadas são descartadas pelo `IdempotencyBehavior`, que usa o Redis para registrar o `EventId` já processado. O envio de e-mail em si é apenas **simulado**: o `EmailService` loga os dados no console em vez de integrar com um provedor real.

---

## ⚙️ Configuração e Variáveis de Ambiente

As configurações de exemplo já estão versionadas em [`local.settings.json`](src/Fcg.Notification.Functions/local.settings.json) e devem corresponder aos serviços de infraestrutura levantados localmente:

```json
{
  "AzureWebJobsStorage": "UseDevelopmentStorage=true",
  "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
  "DatabaseSettings:Host": "localhost",
  "DatabaseSettings:Port": "1433",
  "DatabaseSettings:Username": "sa",
  "DatabaseSettings:Password": "TechChallenge@2026",
  "DatabaseSettings:DatabaseName": "Fcg_Notifications",
  "RabbitMqSettings:Host": "localhost",
  "RabbitMqSettings:Port": "5672",
  "RabbitMqSettings:Username": "guest",
  "RabbitMqSettings:Password": "guest",
  "RabbitMqConnection": "amqp://guest:guest@localhost:5672",
  "RedisSettings:Host": "localhost",
  "RedisSettings:Port": "6379",
  "RedisSettings:Password": "TechChallenge@2026",
  "RedisSettings:InstanceName": "FiapCloudGames:"
}
```

---

## 🚀 Como Executar Localmente

> ⚠️ **Este projeto roda apenas localmente.** Não há deploy em nuvem configurado, pois não há uma assinatura do Azure disponível para provisionar os recursos (Function App, Storage Account, etc.). Todo o fluxo — RabbitMQ, banco de dados e Redis — deve ser levantado na máquina do desenvolvedor.

### Pré-requisitos
- [SDK do .NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- Docker (para subir RabbitMQ, SQL Server e Redis) ou instâncias equivalentes já instaladas
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (emulador de Storage exigido pelo runtime do Functions) — pode ser iniciado via `azurite` (npm) ou automaticamente pelo Visual Studio

### 1. Subir a infraestrutura local

**RabbitMQ:**
```bash
docker run -d --name rabbitmq-dev -p 5672:5672 -p 15672:15672 rabbitmq:3-management-alpine
```

**SQL Server:**
```bash
docker run -d --name sqlserver-dev -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=TechChallenge@2026" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

**Redis:**
```bash
docker run -d --name redis-dev -p 6379:6379 redis:7-alpine redis-server --requirepass TechChallenge@2026
```

As credenciais acima já correspondem ao [`local.settings.json`](src/Fcg.Notification.Functions/local.settings.json) padrão do projeto.

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

Ao subir, a aplicação declara automaticamente a topologia de filas/exchanges no RabbitMQ e passa a consumir os eventos listados acima, logando no console a simulação de envio de e-mail.

---

## 🧪 Testes Automatizados

Para rodar os testes unitários (Domain e Application), execute a partir do diretório raiz:
```bash
dotnet test
```
