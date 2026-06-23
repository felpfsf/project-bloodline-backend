# AGENTS.md — Project Bloodline

Visão do projeto

Project Bloodline é um game assíncrono mobile/web de dark fantasy, inspirado em browser games como BiteFight, OGame e Gladiatus.

O jogo possui facções como vampiros e lobisomens, progressão baseada em caçadas temporizadas, evolução de atributos, PvP automático, economia simples e ranking global.

Objetivos do projeto:

1. Entregar um MVP jogável, pequeno e evolutivo.
2. Servir como laboratório de arquitetura backend moderna aplicada a games online.

O jogo deve priorizar:

* sessões curtas de 2 a 5 minutos;
* progressão constante;
* retorno periódico do jogador;
* competição leve;
* regras simples, porém com builds e atributos relevantes;
* comportamento assíncrono e offline-friendly.

⸻

Stack e direcionamento técnico

Backend

* .NET / C#
* ASP.NET Core
* PostgreSQL
* Entity Framework Core
* Redis
* RabbitMQ + MassTransit quando necessário
* OpenTelemetry para observabilidade
* Docker para ambiente local e infraestrutura de suporte

Frontend

* Flutter / Dart
* Mobile-first
* Web como possível extensão futura

Arquitetura inicial

Começar como Modular Monolith.

Não criar microserviços no MVP.

A arquitetura deve evoluir somente quando houver uma necessidade real de escala, autonomia de deploy, isolamento de falhas ou ownership claramente separado.

Estrutura desejada:

src/
  Bloodline.Api/
  Bloodline.Application/
  Bloodline.Domain/
  Bloodline.Infrastructure/
tests/
  Bloodline.Domain.Tests/
  Bloodline.Application.Tests/
  Bloodline.IntegrationTests/

⸻

Princípios arquiteturais

Server-authoritative

O backend decide todas as regras críticas do jogo.

Nunca confiar no cliente para validar:

* recompensa de caçada;
* consumo ou regeneração de energia;
* resultado de batalha;
* XP;
* ouro;
* atributos;
* ranking;
* cooldowns;
* tempo de conclusão de ações.

O cliente apenas solicita ações e apresenta o estado retornado pelo backend.

⸻

CQRS-lite

Usar CQRS de forma pragmática.

Commands

Commands representam intenção de alterar estado.

Exemplos:

* CreateCharacterCommand
* StartHuntCommand
* CompleteHuntCommand
* ClaimHuntRewardCommand
* UpgradeAttributeCommand
* StartBattleCommand
* RecalculateRankingCommand

Commands devem:

* validar intenção e regras de negócio;
* alterar estado por meio do domínio;
* persistir mudanças;
* gerar eventos relevantes;
* retornar resultado explícito;
* ser idempotentes quando expostos a retries.

Queries

Queries apenas leem dados.

Exemplos:

* GetCharacterDashboardQuery
* GetActiveHuntQuery
* GetHuntHistoryQuery
* GetBattleHistoryQuery
* GetLeaderboardQuery
* GetCharacterProfileQuery

Queries não devem alterar estado, disparar eventos de domínio ou conter regras críticas de negócio.

⸻

DDD pragmático

Aplicar DDD onde ele ajuda a proteger regras importantes.

Não criar abstrações apenas para parecer “enterprise”.

Foco principal:

* invariantes claras;
* nomes do domínio;
* regras encapsuladas;
* fronteiras entre módulos;
* testes de comportamento.

Evitar:

* entidades anêmicas;
* services genéricos sem significado de domínio;
* repositories desnecessários em cima do EF Core;
* interfaces para tudo;
* camadas duplicadas sem responsabilidade real.

⸻

Bounded Contexts iniciais

Os módulos iniciais são:

* Identity
* Character
* Hunt
* Combat
* Ranking
* Economy
* Notification
* Observability

No MVP, eles podem coexistir no mesmo deploy e banco PostgreSQL.

Separação deve ocorrer por módulos, namespaces, responsabilidades e contratos internos — não por microserviços.

⸻

Agregados iniciais

Character

Responsável por:

* identidade do personagem;
* facção;
* nível;
* experiência;
* atributos;
* energia;
* progressão;
* estado necessário para iniciar ações.

Regras de atributos, energia e evolução devem viver preferencialmente neste agregado ou em serviços de domínio bem definidos.

Hunt

Responsável por:

* início da caçada;
* duração;
* estado;
* recompensa calculada;
* conclusão;
* coleta de recompensa;
* prevenção de duplicidade.

Estados esperados:

Started
Completed
Claimed
Cancelled

Uma recompensa não pode ser coletada duas vezes.

Battle

Responsável por:

* atacante;
* defensor;
* snapshot dos atributos usados;
* seed ou referência de RNG controlado;
* resultado;
* recompensa ou consequência;
* auditoria do combate.

Combates devem ser reproduzíveis ou auditáveis o suficiente para investigação de bugs e suspeitas de fraude.

Wallet / Economy

Responsável por:

* ouro;
* créditos futuros;
* débitos;
* ganhos;
* rastreabilidade de transações.

Evitar atualizar saldo “solto” em vários módulos sem registrar a origem.

Toda alteração relevante de saldo deve ter motivo, referência e origem.

Ranking Projection

Responsável por projeções de leaderboard.

Ranking não é fonte primária da verdade.

O ranking pode ser recalculado ou atualizado de forma eventual a partir dos dados de personagem e progresso.

⸻

Eventos de domínio

Eventos representam fatos que já aconteceram.

Usar nomes no passado.

Eventos candidatos:

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

Eventos não devem representar comandos ou desejos.

Errado:

CompleteHuntRequested
UpdateRankingNow
GiveGoldToCharacter

Correto:

HuntCompleted
RankingUpdated
GoldAwarded

⸻

Regras de consistência

PostgreSQL

PostgreSQL é a fonte primária de verdade.

Persistir nele:

* personagens;
* caçadas;
* batalhas;
* inventário futuro;
* carteira/economia;
* histórico de eventos relevantes;
* estado de processamento importante.

Nunca usar Redis como banco principal de regras críticas.

Redis

Redis pode ser usado para:

* cache de queries;
* rate limiting;
* leaderboard derivado;
* locks distribuídos quando estritamente necessários;
* dados temporários;
* otimização de leitura.

Redis não deve ser a única fonte de verdade para:

* ouro;
* XP;
* energia;
* resultado de batalha;
* recompensa;
* progresso do personagem.

Consistência eventual

É aceitável para:

* ranking;
* notificações;
* analytics;
* projeções;
* cache;
* feeds futuros.

Não é aceitável sem compensação explícita para:

* recompensa de caçada;
* gasto de ouro;
* upgrade de atributo;
* energia;
* resultado de combate;
* progressão do personagem.

⸻

Filas, workers e jobs

Usar chamadas síncronas quando a resposta precisa ser imediata e a operação faz parte da regra principal da ação.

Exemplos síncronos:

* criar personagem;
* iniciar caçada;
* melhorar atributo;
* validar energia;
* registrar uma batalha;
* coletar recompensa.

Usar filas e workers quando o trabalho puder ser desacoplado, reprocessado ou executado depois.

Exemplos assíncronos:

* concluir caçadas vencidas;
* atualizar ranking;
* enviar notificações;
* processar analytics;
* recalcular projeções;
* gerar relatórios;
* processar eventos de integração.

Para o MVP, um BackgroundService pode ser suficiente para processar caçadas vencidas.

Não introduzir RabbitMQ apenas porque existe uma ação assíncrona.

Adicionar RabbitMQ + MassTransit quando houver necessidade real de:

* desacoplamento entre módulos;
* retry estruturado;
* consumidores independentes;
* aumento de volume;
* processamento distribuído;
* dead-letter queue;
* maior resiliência operacional.

⸻

Idempotência e retries

Toda operação que possa ser repetida por timeout, retry de cliente, worker ou consumidor deve considerar idempotência.

Exemplos críticos:

* coletar recompensa;
* concluir caçada;
* registrar recompensa;
* processar evento;
* atualizar ranking;
* entregar notificação.

Preferir:

* Idempotency-Key para endpoints críticos;
* IDs de operação;
* constraints únicas no banco;
* status explícito de processamento;
* inbox/outbox quando a arquitetura evoluir.

Nunca assumir que uma mensagem será entregue exatamente uma vez.

Projetar para processamento “at least once”.

⸻

Outbox Pattern

No MVP, eventos internos podem ser publicados após persistência, desde que o risco seja conhecido e controlado.

Quando houver eventos externos, RabbitMQ, múltiplos consumers relevantes ou necessidade maior de confiabilidade, implementar Outbox Pattern.

Fluxo desejado:

Command
→ alteração no agregado
→ persistência no PostgreSQL
→ gravação de evento na Outbox na mesma transação
→ worker publica evento
→ consumer processa de forma idempotente

Não publicar evento externo antes de confirmar a transação principal.

⸻

Regras de gameplay

Core loop

Entrar no jogo
→ coletar recompensas
→ iniciar caçadas
→ evoluir atributos
→ atacar jogadores
→ subir no ranking
→ repetir

Facções

Vampiros

Características iniciais:

* maior chance crítica;
* maior dano;
* menor resistência.

Lobisomens

Características iniciais:

* maior vida;
* maior resistência;
* dano consistente.

As diferenças devem gerar estilos de jogo distintos sem tornar uma facção objetivamente superior.

Toda alteração de balanceamento deve considerar impacto em:

* PvP;
* retenção;
* economia;
* progressão;
* ranking;
* jogadores novos versus veteranos.

Energia

Energia limita ações e incentiva retorno periódico.

Regras de energia devem ser calculadas no servidor usando timestamps confiáveis.

Nunca confiar no relógio do dispositivo.

Preferir calcular regeneração sob demanda com base em:

energia atual
última atualização
taxa de regeneração
limite máximo
horário atual do servidor

Não criar um job por jogador apenas para regenerar energia, salvo necessidade comprovada.

⸻

Convenções de código

C#

Preferir:

* classes pequenas e coesas;
* record para DTOs e value objects imutáveis quando fizer sentido;
* sealed em handlers e classes que não devem ser herdadas;
* CancellationToken em operações assíncronas;
* DateTimeOffset para timestamps persistidos;
* UTC em toda comunicação e persistência;
* nomes explícitos e orientados ao domínio.

Evitar:

* DateTime.Now;
* lógica de domínio em Controllers ou Endpoints;
* acesso direto ao DbContext pela API;
* serviços gigantes;
* static global para estado de jogo;
* exceções para fluxo esperado de negócio.

Result Pattern

Usar Result Pattern para erros esperados de domínio e aplicação.

Exemplos:

InsufficientEnergy
HuntAlreadyActive
HuntNotCompleted
RewardAlreadyClaimed
CharacterNotFound
TargetUnavailable
InsufficientGold

Exceções devem representar falhas inesperadas, técnicas ou invariantes realmente quebradas.

Endpoints

Endpoints ou Controllers devem ser finos.

Responsabilidades:

* autenticação;
* autorização;
* binding;
* validação básica de request;
* delegação ao command/query handler;
* mapeamento de resposta HTTP.

Não colocar regras de gameplay, persistência complexa ou decisões de domínio em endpoints.

⸻

Testes

Testes são obrigatórios para regras críticas.

Prioridade de testes

1. Testes de domínio.
2. Testes de application handlers.
3. Testes de integração com PostgreSQL.
4. Testes de consumers e workers.
5. Testes de API para fluxos críticos.

Cobrir pelo menos:

* consumo de energia;
* regeneração de energia;
* início e conclusão de caçada;
* coleta duplicada de recompensa;
* cálculo de XP e ouro;
* evolução de nível;
* upgrade de atributos;
* resultado de combate;
* idempotência;
* retry de consumidores;
* atualização de ranking.

Nomes de testes devem descrever comportamento:

[Fact]
public void ClaimReward_ShouldFail_WhenRewardWasAlreadyClaimed()

Evitar testes que apenas verificam detalhes internos de implementação.

⸻

Observabilidade

Toda operação importante deve ser rastreável.

Usar logs estruturados.

Campos úteis:

CorrelationId
TraceId
CharacterId
HuntId
BattleId
CommandName
EventName
IdempotencyKey
Outcome
FailureCode
ElapsedMilliseconds

Métricas iniciais importantes:

* caçadas iniciadas;
* caçadas concluídas;
* recompensas coletadas;
* falhas de recompensa;
* batalhas iniciadas;
* batalhas concluídas;
* consumo de energia;
* tempo médio de handlers;
* falhas de workers;
* retries de consumidores;
* tamanho de filas;
* tempo de processamento de eventos.

Não registrar dados sensíveis desnecessariamente.

Logs devem ajudar a responder:

* qual personagem executou a ação;
* qual comando falhou;
* qual regra bloqueou a ação;
* se houve retry;
* se um evento foi processado mais de uma vez;
* qual foi o impacto técnico e de gameplay.

⸻

Segurança e fraude

Assumir que o cliente pode ser modificado, automatizado ou malicioso.

Sempre validar no backend:

* identidade do usuário;
* ownership do personagem;
* energia;
* cooldown;
* saldo;
* elegibilidade de alvo;
* estado da caçada;
* regras de recompensa;
* limites de rate.

Adicionar rate limit em endpoints expostos a abuso, especialmente:

* iniciar caçada;
* coletar recompensa;
* iniciar batalha;
* buscar ranking;
* autenticação.

Não expor seeds, fórmulas internas completas ou dados sensíveis de outros jogadores sem necessidade.

⸻

Banco de dados

Usar migrations versionadas.

Toda mudança de schema deve considerar:

* retrocompatibilidade;
* impacto em produção;
* índices necessários;
* constraints de unicidade;
* rollback ou estratégia de correção;
* volume futuro.

Preferir constraints no banco para invariantes críticas complementares.

Exemplos:

* apenas uma caçada ativa por personagem;
* uma recompensa só pode ser coletada uma vez;
* uma transação econômica não pode ser duplicada por mesma referência;
* um evento de inbox não pode ser processado duas vezes.

⸻

Definition of Done

Uma alteração está pronta quando:

* atende ao comportamento solicitado;
* respeita as regras deste arquivo;
* possui testes proporcionais ao risco;
* não quebra regras server-authoritative;
* considera idempotência quando aplicável;
* possui logs ou métricas quando altera fluxo crítico;
* não introduz infraestrutura sem necessidade;
* mantém naming claro;
* passa build, testes e formatadores.

Antes de finalizar mudanças, executar:

dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet format --verify-no-changes

Caso algum comando não exista ou não esteja configurado, informar claramente em vez de assumir sucesso.

⸻

Como agir ao implementar mudanças

Antes de criar código:

1. Identificar o bounded context afetado.
2. Identificar o agregado dono da regra.
3. Definir command ou query.
4. Definir invariantes e erros esperados.
5. Avaliar consistência, retry e idempotência.
6. Decidir se precisa ser síncrono ou assíncrono.
7. Criar testes primeiro para regras críticas.
8. Implementar a menor solução coerente com o MVP.

Ao propor arquitetura, diferenciar explicitamente:

* solução para MVP;
* evolução provável;
* solução de produção em maior escala.

Priorizar sempre clareza de domínio, jogabilidade, rastreabilidade e evolução sustentável.
