GitHub Copilot Instructions — Project Bloodline

Contexto do projeto

Project Bloodline é um game assíncrono dark fantasy inspirado em browser games como BiteFight, OGame e Gladiatus.

O jogo possui personagens de facções como vampiros e lobisomens, caçadas temporizadas, progressão de atributos, PvP automático, economia e ranking.

Objetivos:

* entregar um MVP jogável e evolutivo;
* estudar arquitetura backend moderna aplicada a games online;
* priorizar regras claras, rastreabilidade e evolução sustentável.

⸻

Stack

Backend:

* .NET / C#
* ASP.NET Core
* PostgreSQL
* Entity Framework Core
* Redis
* RabbitMQ + MassTransit quando necessário
* OpenTelemetry
* Docker

Frontend:

* Flutter / Dart
* mobile-first

⸻

Arquitetura

O projeto começa como um Modular Monolith.

Não criar microserviços no MVP.

Estrutura esperada:

src/
  Bloodline.Api/
  Bloodline.Application/
  Bloodline.Domain/
  Bloodline.Infrastructure/
tests/
  Bloodline.Domain.Tests/
  Bloodline.Application.Tests/
  Bloodline.IntegrationTests/

Responsabilidades:

* Bloodline.Api: endpoints, autenticação, autorização, request/response mapping.
* Bloodline.Application: commands, queries, handlers, casos de uso e orquestração.
* Bloodline.Domain: regras de negócio, agregados, entidades, value objects e eventos de domínio.
* Bloodline.Infrastructure: EF Core, PostgreSQL, Redis, mensageria, serviços externos e implementações técnicas.

Nunca colocar regra crítica de gameplay em endpoints, controllers, DTOs ou no cliente Flutter.

⸻

Server-authoritative

O backend é a fonte de decisão para regras críticas.

Nunca confiar no cliente para calcular ou validar:

* XP;
* ouro;
* energia;
* cooldowns;
* recompensa;
* resultado de batalha;
* duração de caçada;
* evolução de atributos;
* ranking;
* elegibilidade de alvo.

Use sempre horário do servidor e DateTimeOffset.UtcNow.

Não usar DateTime.Now.

⸻

DDD pragmático

Aplicar DDD apenas onde proteger regras relevantes do domínio.

Priorizar:

* agregados com invariantes claras;
* nomes orientados ao domínio;
* regras encapsuladas;
* testes de comportamento;
* código simples e explícito.

Evitar:

* entidades anêmicas;
* interfaces para tudo;
* repositories genéricos sobre EF Core;
* abstrações sem responsabilidade clara;
* services gigantes;
* lógica duplicada entre módulos.

⸻

CQRS-lite

Commands alteram estado.

Queries apenas leem dados.

Commands candidatos:

* CreateCharacterCommand
* StartHuntCommand
* CompleteHuntCommand
* ClaimHuntRewardCommand
* UpgradeAttributeCommand
* StartBattleCommand
* RecalculateRankingCommand

Queries candidatas:

* GetCharacterDashboardQuery
* GetActiveHuntQuery
* GetHuntHistoryQuery
* GetBattleHistoryQuery
* GetLeaderboardQuery
* GetCharacterProfileQuery

Commands devem:

1. validar a intenção;
2. carregar o agregado necessário;
3. aplicar regras de domínio;
4. persistir mudanças;
5. produzir eventos relevantes;
6. retornar resultado explícito.

Queries não podem alterar estado ou disparar efeitos colaterais de negócio.

⸻

Agregados principais

Character

Dono de:

* facção;
* atributos;
* nível;
* XP;
* energia;
* progressão;
* elegibilidade para ações.

Hunt

Dono de:

* início;
* duração;
* status;
* conclusão;
* recompensa;
* coleta;
* prevenção de duplicidade.

Estados esperados:

Started
Completed
Claimed
Cancelled

Uma recompensa nunca pode ser coletada duas vezes.

Battle

Dono de:

* atacante;
* defensor;
* snapshot de atributos;
* resultado;
* auditoria;
* consequências.

Resultados de batalha precisam ser auditáveis.

Wallet / Economy

Dono de:

* créditos;
* débitos;
* saldo;
* origem de transações.

Toda alteração relevante de ouro deve possuir motivo e referência.

Ranking Projection

É uma projeção derivada.

Ranking não é fonte primária de verdade.

⸻

Eventos de domínio

Eventos representam fatos que já aconteceram.

Usar nomes no passado.

Exemplos:

* CharacterCreated
* FactionChosen
* HuntStarted
* HuntCompleted
* RewardClaimed
* CharacterLeveledUp
* AttributeUpgraded
* EnergyConsumed
* EnergyRegenerated
* BattleStarted
* BattleFinished
* RankingUpdated

Não criar eventos que representem intenção ou comando.

Errado:

CompleteHuntRequested
GiveGoldToCharacter
UpdateRankingNow

Correto:

HuntCompleted
GoldAwarded
RankingUpdated

⸻

PostgreSQL e Redis

PostgreSQL é a fonte primária de verdade.

Persistir no PostgreSQL:

* personagens;
* caçadas;
* batalhas;
* recompensas;
* economia;
* histórico relevante;
* estado de processamento crítico.

Redis pode ser usado para:

* cache;
* ranking derivado;
* rate limit;
* dados temporários;
* locks distribuídos quando necessários.

Redis nunca pode ser a única fonte de verdade para:

* ouro;
* XP;
* energia;
* recompensa;
* batalha;
* progresso.

⸻

Operações síncronas e assíncronas

Usar operações síncronas quando fazem parte da regra principal e a resposta precisa ser imediata:

* criar personagem;
* iniciar caçada;
* melhorar atributo;
* validar energia;
* coletar recompensa;
* iniciar batalha.

Usar workers, jobs ou filas para trabalho desacoplado:

* concluir caçadas vencidas;
* atualizar ranking;
* enviar notificações;
* analytics;
* projeções;
* relatórios.

Para o MVP, preferir BackgroundService antes de introduzir RabbitMQ.

Usar RabbitMQ + MassTransit apenas quando houver necessidade concreta de retry, consumidores independentes, processamento distribuído ou desacoplamento real.

⸻

Idempotência

Toda operação sujeita a retry deve ser idempotente.

Fluxos críticos:

* coletar recompensa;
* concluir caçada;
* registrar recompensa;
* processar eventos;
* atualizar ranking;
* enviar notificações.

Usar quando aplicável:

* Idempotency-Key;
* IDs de operação;
* constraints únicas no banco;
* status explícito;
* inbox/outbox.

Nunca assumir entrega exatamente uma vez.

Projetar consumidores para processamento “at least once”.

⸻

Result Pattern

Usar Result Pattern para falhas esperadas de negócio.

Exemplos:

InsufficientEnergy
HuntAlreadyActive
HuntNotCompleted
RewardAlreadyClaimed
CharacterNotFound
TargetUnavailable
InsufficientGold

Não usar exceções para fluxo esperado de negócio.

Exceções devem representar falhas técnicas inesperadas ou invariantes quebradas.

⸻

Convenções C#

Preferir:

* record para DTOs e value objects imutáveis;
* sealed em classes que não precisam de herança;
* CancellationToken em operações assíncronas;
* DateTimeOffset para timestamps;
* UTC em persistência e comunicação;
* nomes explícitos e orientados ao domínio;
* métodos pequenos e coesos.

Evitar:

* lógica de domínio em endpoints;
* acesso ao DbContext diretamente pela API;
* estado global estático;
* métodos com múltiplas responsabilidades;
* dependências de infraestrutura no domínio.

⸻

Testes

Adicionar testes proporcionais ao risco.

Prioridade:

1. domínio;
2. handlers de application;
3. integração com banco;
4. workers e consumers;
5. API para fluxos críticos.

Cobrir sempre que aplicável:

* consumo e regeneração de energia;
* início e conclusão de caçada;
* coleta duplicada;
* cálculo de XP e ouro;
* upgrade de atributos;
* resultado de combate;
* idempotência;
* retry;
* ranking.

Exemplo de nome:

[Fact]
public void ClaimReward_ShouldFail_WhenRewardWasAlreadyClaimed()

Testar comportamento, não detalhes internos.

⸻

Observabilidade

Usar logs estruturados em fluxos importantes.

Incluir quando disponível:

CorrelationId
TraceId
CharacterId
HuntId
BattleId
CommandName
EventName
IdempotencyKey
FailureCode
Outcome
ElapsedMilliseconds

Toda mudança em fluxo crítico deve considerar logs, métricas e rastreabilidade.

⸻

Segurança e fraude

Assumir que o cliente pode ser adulterado.

Sempre validar no backend:

* usuário autenticado;
* ownership do personagem;
* saldo;
* energia;
* cooldown;
* estado da caçada;
* elegibilidade de alvo;
* limites de rate;
* regras de recompensa.

Não expor fórmulas internas, seeds ou dados sensíveis sem necessidade.

⸻

Antes de implementar

Antes de gerar código:

1. identificar o bounded context;
2. identificar o agregado dono da regra;
3. definir command ou query;
4. listar invariantes;
5. avaliar consistência e idempotência;
6. decidir se a operação é síncrona ou assíncrona;
7. criar testes para regras críticas;
8. implementar a menor solução coerente com o MVP.

Ao sugerir mudanças arquiteturais, diferenciar:

* solução para MVP;
* evolução futura;
* solução para alta escala/produção.