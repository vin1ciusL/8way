# Reclamation - Jogo 2D Survivor

## 📋 Visão Geral

**Reclamation** é um jogo de ação 2D estilo survivor desenvolvido em Unity. O jogador controla um personagem em uma cidade, e deve capturar areas que foram tomadas por zumbis, usando um bastão como arma principal.

### Mecânicas Principais
- ⚔️ **bastão**: Arma que gira seguindo o mouse para derrotar inimigos
- 🧟 **Inimigos**: Zumbis que perseguem o jogador inteligentemente
- 💰 **Comida**: Itens de coleta espalhadas pela arena
- ❤️ **Sistema de Saúde**: Jogador começa com 20 pontos de vida
- 🎯 **Zonas de Captura**: 4 áreas especiais que o jogador deve capturar para desbloquear recompensas
  - **Zona 1**: Desbloqueia Granada 💣
  - **Zona 2**: +Dano
  - **Zona 3**: +Velocidade
  - **Zona 4**: Vitória! 🏆

---

## 🎮 Controles

| Ação | Controle |
|------|----------|
| Movimento | `WASD` ou `Setas` |
| Atacar | `Mouse` (gira a arma na direção do cursor) |

---

## 📁 Estrutura de Arquivos e O que Cada Script Controla

### Core Game Logic
- **`GameController.cs`** ⚙️
  - **Função**: Gestor central de estado do jogo e progressão.
  - **Controla**: 
    - Saúde do player (`playerHealth`) e Tempo de jogo (`gameTime`).
    - Estado de Vitória/Derrota (`gameOver`).
    - Multiplicadores Dinâmicos: `chainsawMultiplier` e `playerSpeedMultiplier`.
    - Progressão: `zonesCompleted`, `isCapturingZone`, e desbloqueios (`hasGrenade`).
  - **Métodos Públicos**:
    - `Init()` - Reseta o jogo ao iniciar.
    - `TakeDamage(int damage)` / `Heal(int amount)` - Gestão de vida.
    - `CompleteZone()` - Processa recompensas ao dominar áreas.
  - **Eventos**: `OnHealthChanged`, `OnGameOver`, `OnGameWin`, `OnZoneReward`.

### Player
- **`PlayerMovement.cs`** 🚶
  - **Função**: Controla a física de movimento, sistema de dano e animações.
  - **Controla**:
    - Movimento suave com Lerp (interpolação).
    - Animação Direcional (4-Way) através do componente `Animator` (Run_Cima, Baixo, Esq, Dir).
    - Recebimento de dano dinâmico (lê a variável `damageToPlayer` de cada zumbi que encostar).
  - **Sistema de Dano**:
    - Dano varia de acordo com o tipo de zumbi atacando.
    - Cooldown de 0.4s de invulnerabilidade após o hit.
    - Suporta múltiplos inimigos (soma o dano de todos em contato simultâneo).
  - **Mecânica**: Usa `OnCollisionEnter2D`/`OnCollisionExit2D` com contador para evitar bugs de imunidade.

- **`ChainsawWeapon.cs`** ⚙️
  - **Função**: Rotaciona a arma seguindo o mouse.
  - **Controla**: Ângulo de rotação da arma com física de "arrasto" (Lerp).

- **`CameraFollow.cs`** 📹
  - **Função**: Segue o player suavemente pelo mapa.

### Enemies
- **`EnemyController.cs`** 🧟
  - **Função**: IA, atributos e áudio individual de cada zumbi.
  - **Variantes (Enum `EnemyType`)**:
    - `Small`: Rápido e frágil.
    - `Big`: Lento, muita vida.
    - `Axe`: Dano massivo.
    - `Flanker`: Extremamente rápido, faz curvas para atacar pelas costas.
    - `Giant`: O dobro do tamanho, funciona como um mini-boss lento e resistente.
    - `GhostAxe`: Transparente, ignora colisão com a horda (Layer "Fantasma") e imune a knockback.
  - **Mecânicas Extras**:
    - **Catch-up**: Ficam 3.5x mais rápidos se estiverem muito longe do player.
    - **Zone Buffs**: Recebem multiplicadores de vida e velocidade conforme o player captura as zonas.
    - **Áudio 3D**: Sorteia gemidos aleatórios e emite som ao ser cortado pela arma.

- **`EnemySpawner.cs`** 👹
  - **Função**: Gestor de horda e dificuldade progressiva.
  - **Controla**:
    - Posicionamento inteligente em Quadrantes (cerca o jogador por todos os lados).
    - **Sorteio Dinâmico**: Usa uma tabela de probabilidade acumulada (0 a 100) que muda dependendo de quantas Zonas foram capturadas (ex: `GhostAxe` e `Giant` só aparecem depois).
  - **Parâmetros Configuráveis**: `minSpawnRadius`, `maxSpawnRadius`, `maxEnemies`.

- **`EnemyDestroyListener.cs`** 👂
  - **Função**: Listener de destruição que notifica o spawner para liberar espaço na memória.

### Armas Extras e Itens
- **`GrenadeLauncher.cs`** 🚀
  - **Função**: Dispara explosivos automaticamente.
  - **Controla**: Cooldown de 15 segundos. Se `GameController.hasGrenade` for true, atira na direção do mouse.
- **`Grenade.cs`** 💣
  - **Função**: Comportamento do projétil explosivo.
  - **Controla**: Movimento, detonação por tempo (2s) ou impacto, dano em área (`OverlapCircleAll`) e instanciação de áudio residual (`PlayClipAtPoint`).

- **`CoinSpawner.cs`** 💰
  - **Função**: Gera moedas de cura pelo mapa a cada 18 segundos.
- **`ComidaTimer.cs`** ⏱️
  - **Função**: Destruição automática do coletável após 10 segundos para não poluir o mapa.

### Zonas
- **`Zone.cs`** 🎯
  - **Função**: Representa uma base capturável (King of the Hill).
  - **Controla**:
    - Progresso (Tempo que o player passa dentro).
    - Emite o evento `OnCaptureProgress` para a UI.
    - Fica transparente ao ser totalmente dominada.
- **`ZonePointer.cs`** 🔺
  - **Função**: Bússola dinâmica.
  - **Controla**: Busca constantemente a Zona não-capturada mais próxima e rotaciona (com Lerp) a seta apontando para ela.

### UI
- **`UIManager.cs`** 🖼️
  - **Função**: Gerencia o HUD e a tela de fim de jogo.
  - **Controla**: 
    - Textos em tempo real (Vida, Timer, Pop-ups de Recompensa e Zonas: 0/4).
    - Barra de captura da Zona ("Capturando: X.Xs").
    - **Run Summary**: Gera dinamicamente uma tela de Vitória ou Derrota usando *Rich Text*, exibindo tempo de sobrevivência e o último upgrade obtido.
  - **Eventos Inscritos**: `OnHealthChanged`, `OnGameOver`, `OnGameWin`, `OnZoneReward`, `OnCaptureProgress`.

- **`MenuActions.cs`** - **Função**: Scripts de carregamento de cena (Botões Iniciar, Reiniciar e Sair).

### Utilidades
- **`MapConfig.cs`** 🗺️
  - **Função**: Configuração centralizada dos limites da arena para spawns seguros (`GetRandomSpawnPosition()`).
- **`PlayerReference.cs`** 🎯
  - **Função**: Singleton de performance. Elimina múltiplas buscas custosas (`FindGameObjectWithTag`) pelo mapa.

## 📝 Referências e Créditos

Os recursos visuais e sonoros utilizados em **Reclamation** pertencem aos seus respectivos criadores:

* **Pixel Art Base**: [TheLazyStone](https://thelazystone.itch.io/post-apocalypse-pixel-art-asset-pack)
* **SFX - Impacto (Cortes/Hit)**: [Mixkit](https://mixkit.co/free-sound-effects/hit/)
* **SFX - Explosão de Granada**: [LMGLOLO](https://lmglolo.itch.io/free-fps-sfx)
* **SFX - Fogo/Zona (Bonfire)**: [Pixabay](https://pixabay.com/sound-effects/search/bonfire/)
* **SFX - Vozes dos Zumbis**: [Sweenus](https://sweenus.itch.io/zombie-sound-fx-reverb-heavy)

---

| **Engine**: Unity 6 | **Linguagem**: C#