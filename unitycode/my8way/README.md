# 8Way - Jogo 2D Survivor/Roguelike

## 📋 Visão Geral

**8Way** é um jogo de ação 2D estilo survivor/roguelike desenvolvido em Unity. O jogador controla um personagem em uma arena infinita que deve sobreviver o máximo de tempo possível contra hordas de inimigos enquanto coleta moedas.

### Mecânicas Principais
- ⚔️ **Motosserra**: Arma que gira seguindo o mouse para derrotar inimigos
- 🧟 **Inimigos**: Zumbis que perseguem o jogador inteligentemente
- 💰 **Moedas**: Objetivos de coleta espalhadas pela arena
- ❤️ **Sistema de Saúde**: Jogador começa com 30 pontos de vida
- ⏱️ **Dificuldade Progressiva**: A cada minuto, inimigos spawnam mais rápido
- 📹 **Câmera Dinâmica**: Segue o jogador suavemente

---

## 🎮 Controles

| Ação | Controle |
|------|----------|
| Movimento | `WASD` ou `Setas` |
| Atacar | `Mouse` (gira a motosserra na direção do cursor) |
| Menu | [Botão de Menu na UI] |
| Reiniciar | [Botão de Restart após Game Over] |

---

## 📁 Estrutura de Arquivos e O que Cada Script Controla

### Core Game Logic
- **`GameController.cs`** ⚙️
  - **Função**: Gestor central de estado do jogo
  - **Controla**: 
    - Contador de moedas (`coinCount`)
    - Saúde do player (`playerHealth`)
    - Estado de Game Over
    - Tempo de jogo (`gameTime`)
  - **Métodos Públicos**:
    - `Init()` - Reseta o jogo ao iniciar
    - `TakeDamage(int damage)` - Aplica dano ao player
    - `Heal(int amount)` - Restaura saúde
    - `getCoin()` - Decrementa contador quando player coleta moeda
  - **Eventos**: `OnHealthChanged`, `OnGameOver`

### Player
- **`PlayerMovement.cs`** 🚶
  - **Função**: Controla movimento do player e sistema de dano
  - **Controla**:
    - Movimento suave com Lerp (interpolação)
    - Velocidade e direção do player
    - Coleta de moedas (via Trigger)
    - Recebimento de dano de inimigos (via Collision)
  - **Parâmetros Configuráveis**: `speed`, `suavidade`
  - **Sistema de Dano**:
    - Dano de 2 pontos por hit
    - Cooldown de 0.4s entre danos
    - Suporta múltiplos inimigos (conta quantos estão em contato)
  - **Mecânica**: Usa `OnCollisionEnter2D`/`OnCollisionExit2D` com contador para evitar bug de imunidade

- **`ChainsawWeapon.cs`** ⚙️
  - **Função**: Rotaciona a motosserra seguindo o mouse
  - **Controla**: 
    - Ângulo de rotação da arma
    - Suavidade da rotação (gira "arrastando" inimigos)
  - **Parâmetro Configurável**: `velocidadeGiro` (25 padrão)

- **`CameraFollow.cs`** 📹
  - **Função**: Segue o player suavemente
  - **Controla**: Posição da câmera com interpolação Lerp
  - **Parâmetros Configuráveis**: `smoothSpeed`, `offset`

### Enemies
- **`EnemyController.cs`** 🧟
  - **Função**: Comportamento individual de cada zumbi
  - **Controla**:
    - Perseguição ao player
    - Sistema de health/dano
    - Knockback quando é atingido pela motosserra
    - Velocidade de movimento (1.5 padrão)
  - **Mecânicas**:
    - Busca atualiza a cada 0.2s (otimização de pathfinding)
    - Knockback "atordoa" o zumbi por 0.15s
    - Recebe 2 de dano por hit da motosserra

- **`EnemySpawner.cs`** 👹
  - **Função**: Gera inimigos ao longo do jogo
  - **Controla**:
    - Spawn inicial (10 inimigos)
    - Spawn contínuo cada 1.5s (reduz com dificuldade)
    - Posicionamento inteligente (em círculo ao redor do player)
    - Limite máximo de 50 inimigos simultâneos
    - Sistema de dificuldade progressiva (a cada minuto, spawn fica 1.5x mais rápido)
  - **Parâmetros Configuráveis**:
    - `minSpawnRadius` / `maxSpawnRadius`: Distância do player onde inimigos nascem
    - `mapMinX/Y`, `mapMaxX/Y`: Limites da arena
    - `initialEnemyCount`, `maxEnemies`

- **`EnemyDestroyListener.cs`** 👂
  - **Função**: Listener de destruição de inimigos
  - **Controla**: Notifica o spawner quando um inimigo morre

### Items
- **`CoinSpawner.cs`** 💰
  - **Função**: Gera moedas ao longo do jogo
  - **Controla**:
    - Spawn contínuo de moedas a cada 5s
    - Posicionamento (círculo ao redor do player)
    - Limites da arena
  - **Parâmetros Configuráveis**:
    - `minSpawnRadius` / `maxSpawnRadius`: Distância de spawn
    - Limites do mapa (duplicados com EnemySpawner)

### UI
- **`UIManager.cs`** 🖼️
  - **Função**: Gerencia toda a interface do usuário
  - **Controla**:
    - Display de saúde
    - Display de moedas
    - Timer de tempo decorrido
    - Painel de Game Over
    - Incremento de tempo de jogo
  - **Referências Necessárias**:
    - `healthText`, `coinsText`, `timerText`, `finalTimeText`
    - `endGamePanel`

- **`MenuActions.cs`** (ou `ButtonScript.cs`)
  - **Função**: Ações de botões de menu
  - **Controla**:
    - `IniciaJogo()` - Inicia novo jogo
    - `Menu()` - Volta ao menu

### Utilidades
- **`MapConfig.cs`** 🗺️
  - **Função**: Configuração centralizada dos limites da arena
  - **Controla**:
    - Limites do mapa (`MinX`, `MaxX`, `MinY`, `MaxY`)
    - Lógica de spawn aleatório em círculo
  - **Métodos Estáticos**:
    - `ClampPosition()` - Prende uma posição dentro dos limites
    - `GetRandomSpawnPosition()` - Calcula posição aleatória válida
  - **Importância**: Elimina duplicação entre `EnemySpawner` e `CoinSpawner`

- **`PlayerReference.cs`** 🎯
  - **Função**: Singleton que fornece referência centralizada ao Player
  - **Controla**: Acesso único ao Transform do Player
  - **Benefícios**: Elimina 3 chamadas O(n) a `FindGameObjectWithTag()`
  - **IMPORTANTE**: Deve ser adicionado ao Player GameObject no Inspector!

---

## ✅ Status de Correção de Bugs

Todos os 8 bugs críticos e médios foram **corrigidos** na versão atual:

- ✅ BUG #1: Redundância de UI removida
- ✅ BUG #2: Limites de mapa centralizados em MapConfig.cs
- ✅ BUG #3: GameTime agora independente de UIManager
- ✅ BUG #4: PlayerReference singleton elimina buscas redundantes
- ✅ BUG #5: Camera.main com verificação de null
- ✅ BUG #6: PlayerMovement refatorado com contador de inimigos
- ✅ BUG #7: Código de spawn consolidado em MapConfig
- ✅ BUG #8: UI validada com logs de erro

Para detalhes completos, veja [BUGS_REPORT.md](BUGS_REPORT.md) e [CORREÇÕES_IMPLEMENTADAS.md](CORREÇÕES_IMPLEMENTADAS.md).

---

### 🔴 CRÍTICO - Redundância de Atualização de UI
**Arquivo**: `UIManager.cs` - Linhas ~42-43
```csharp
void Update() 
{
    // ...
    // Mantém display de vida atualizado (redundante mas seguro)
    UpdateHealthDisplay(); // ❌ REDUNDÂNCIA
}
```
**Problema**: `UpdateHealthDisplay()` é chamada 60+ vezes por segundo no `Update()` E também está inscrita no evento `OnHealthChanged` do GameController. Isso causa atualização desnecessária constantemente.

**Solução**: Remover a chamada do `Update()`, confiar apenas no evento.

---

### 🟠 ALTO - Limites do Mapa Duplicados
**Arquivos**: `EnemySpawner.cs` e `CoinSpawner.cs`
```csharp
// Em ambos arquivos:
public float mapMinX = -100f;
public float mapMaxX = 100f;
public float mapMinY = -100f;
public float mapMaxY = 100f;
```
**Problema**: Se um mapa for alterado e o outro não, inimigos e moedas podem spawnar em locais inconsistentes.

**Solução**: Criar uma classe `ArenaManager` ou `MapConfig` estática que define os limites uma única vez.

---

### 🟠 ALTO - Dependência Implícita no Timer
**CORRIGIDO** ✅ - `GameController.UpdateGameTime()` agora é chamado por UIManager

---

### 🟡 MÉDIO - FindGameObjectWithTag Múltiplo
**CORRIGIDO** ✅ - Implementado `PlayerReference.cs` singleton

---

### 🟡 MÉDIO - Camera.main Sem Verificação
**CORRIGIDO** ✅ - Adicionada verificação e cache em `ChainsawWeapon.cs`

---

### 🟡 MÉDIO - OnCollisionStay2D Chamado a Cada Frame
**CORRIGIDO** ✅ - Refatorado com `OnCollisionEnter2D`/`OnCollisionExit2D` e contador

---

### 🟡 MÉDIO - Código Duplicado de Spawn
**CORRIGIDO** ✅ - Consolidado em `MapConfig.GetRandomSpawnPosition()`

---

### 🟢 BAIXO - Validação de UI
**CORRIGIDO** ✅ - Validação adicionada ao Start()

---

## 🎯 Fluxo do Jogo

```
1. Menu → Clica "Iniciar"
2. GameController.Init() → Reset do jogo
3. Spawner cria 10 inimigos + moedas começam a spawnar
4. Player controla com WASD, gira motosserra com mouse
5. Motosserra causa 2 dano em inimigos por hit (com cooldown de 0.1s)
6. Player sofre 2 dano a cada 0.4s quando em contato com inimigo
7. Dano acumula se múltiplos inimigos estão atacando (contador de inimigos)
8. A cada 5s, uma moeda spawna
9. A cada minuto, spawn de inimigos fica 1.5x mais rápido
10. Quando saúde chega a 0 → Game Over + mostra tempo total
11. Pode retornar ao Menu
```

---

## 📊 Configurações Importantes (Ajustar no Inspector)

| Parâmetro | Local | Valor Padrão | O que faz |
|-----------|-------|--------------|----------|
| `speed` | PlayerMovement | ? | Velocidade do player |
| `suavidade` | PlayerMovement | 25 | Smoothness do movimento |
| `damageCooldown` | PlayerMovement | 0.4s | Intervalo entre danos de inimigos |
| `moveSpeed` | EnemyController | 1.5 | Velocidade de perseguição dos zumbis |
| `spawnInterval` | EnemySpawner | 1.5s | Intervalo inicial de spawn de inimigos |
| `maxEnemies` | EnemySpawner | 50 | Max inimigos simultâneos na arena |
| `minSpawnRadius` | EnemySpawner | 10 | Distância mínima de spawn ao redor do player |
| `maxSpawnRadius` | EnemySpawner | 20 | Distância máxima de spawn ao redor do player |
| `spawnInterval` | CoinSpawner | 5s | Intervalo de spawn de moedas |
| `velocidadeGiro` | ChainsawWeapon | 25 | Velocidade de rotação da motosserra |
| `smoothSpeed` | CameraFollow | 0.125 | Suavidade do movimento da câmera |

---

## ⚠️ Notas Técnicas

- **Limite de FPS**: O jogo roda a 60 FPS (Update) e 50 FPS de Física (FixedUpdate)
- **Knockback**: Zumbis recebem knockback por 0.15s quando atingidos pela motosserra
- **Câmera**: Z = -10 para manter visualização 2D isométrica
- **Input System**: Usa `Input.GetAxisRaw()` para movimento digital puro (sem aceleração)
- **Colliders**: 
  - Player = BoxCollider (corpo) + CircleCollider (arma/motosserra)
  - Inimigos = CircleCollider (corpo que causa dano)
- **Sistema de Dano**:
  - Inimigo → Player: 2 pontos, a cada 0.4s
  - Motosserra → Inimigo: 2 pontos por hit com cooldown de 0.1s
  - Suporta múltiplos inimigos (usa contador de contactos)
- **GameTime**: Incrementado independentemente por `GameController.UpdateGameTime()`
- **Spawn**: Limites centralizados em `MapConfig` para consistência

---

## 🚀 Próximas Implementações Sugeridas

- [ ] Sistema de wave/fases
- [ ] Múltiplas armas com upgrade
- [ ] Efeitos visuais/partículas
- [ ] Som/música
- [ ] Leaderboard de tempo
- [ ] Diferentes tipos de inimigos
- [ ] Power-ups
- [ ] Persistência de dados (High Score)

---

**Versão**: 1.0  
**Data**: Abril 2026  
**Engine**: Unity 6  
**Linguagem**: C#
