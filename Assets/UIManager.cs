using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject endGamePanel;
    public TextMeshProUGUI healthText;  

    // NOVAS REFERÊNCIAS DE TEXTO
    public TextMeshProUGUI timerText;      // Texto do tempo na tela do jogo
    public TextMeshProUGUI finalTimeText;  // Texto do tempo na tela de Game Over
    
    // NOVO: Textos das Zonas
    public TextMeshProUGUI zoneProgressText; // Ex: "Capturando: 12.5s"
    public TextMeshProUGUI zoneCountText;    // Ex: "Zonas: 1/4"
    public TextMeshProUGUI popupText;        // Ex: "Granada Desbloqueada!"

    private float popupTimer = 0f;

    void Start()
    {
        GameController.Init();
        
        // Validação de referências
        if (healthText == null)
            Debug.LogError("UIManager: healthText não configurada!");
        if (timerText == null)
            Debug.LogError("UIManager: timerText não configurada!");
        if (finalTimeText == null)
            Debug.LogWarning("UIManager: finalTimeText não configurada (opcional)");
        if (endGamePanel == null)
            Debug.LogError("UIManager: endGamePanel não configurada!");
        
        if (zoneProgressText != null) zoneProgressText.text = "";
        if (popupText != null) popupText.text = "";

        UpdateHealthDisplay();
        
        // Inscreve-se nos eventos do GameController
        GameController.OnHealthChanged += UpdateHealthDisplay;
        GameController.OnGameOver += OnGameOver;
        GameController.OnGameWin += OnGameWin;
        
        // Novos eventos
        Zone.OnCaptureProgress += UpdateCaptureText;
        GameController.OnZoneReward += ShowPopup;
    }

    void OnDestroy()
    {
        // Desinscreve-se dos eventos
        GameController.OnHealthChanged -= UpdateHealthDisplay;
        GameController.OnGameOver -= OnGameOver;
        GameController.OnGameWin -= OnGameWin;
        Zone.OnCaptureProgress -= UpdateCaptureText;
        GameController.OnZoneReward -= ShowPopup;
    }

    void Update() 
    {
        // Atualiza o timer a cada frame
        if (!GameController.gameOver)
        {
            GameController.UpdateGameTime(Time.deltaTime);
            UpdateTimerDisplay();

            // Apaga o aviso depois de 3 segundos
            if (popupTimer > 0)
            {
                popupTimer -= Time.deltaTime;
                if (popupTimer <= 0 && popupText != null) popupText.text = "";
            }
        }
    }

    void UpdateCaptureText(float current, float total)
    {
        if (zoneProgressText == null) return;
        
        if (current <= 0 || current >= total) zoneProgressText.text = "";
        else zoneProgressText.text = "Capturando: " + (total - current).ToString("F1") + "s";
    }

    void ShowPopup(string mensagem)
    {
        if (popupText != null)
        {
            popupText.text = mensagem;
            popupTimer = 3f; // Fica na tela por 3 segundos
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Calcula minutos e segundos
            int minutes = Mathf.FloorToInt(GameController.GameTime / 60F);
            int seconds = Mathf.FloorToInt(GameController.GameTime - minutes * 60);
            
            // Formata para ficar no estilo 00:00
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = "Vida: " + GameController.PlayerHealth + "/30";
        }
        if (zoneCountText != null)
        {
            zoneCountText.text = "Zonas: " + GameController.zonesCompleted + "/4";
        }
    }
    void OnGameOver()
        {
            endGamePanel.SetActive(true);
            GenerateRunSummary(false); // false = Derrota
        }

        void OnGameWin()
        {
            endGamePanel.SetActive(true);
            GenerateRunSummary(true);  // true = Vitória
        }

        // NOVO: Função centralizada que formata o texto dependendo de vitória ou derrota
        void GenerateRunSummary(bool isWin)
        {
            if (finalTimeText == null) return;

            // Calcula o tempo formatado
            int minutes = Mathf.FloorToInt(GameController.GameTime / 60F);
            int seconds = Mathf.FloorToInt(GameController.GameTime - minutes * 60);
            string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);

            // 1. Título dinâmico com cores Hexadecimais e tamanhos diferentes (Rich Text do TextMeshPro)
            string title = isWin ? "<color=#55FF55><size=150%><b>VITÓRIA!</b></size></color>\n" 
                                : "<color=#FF4444><size=150%><b>VOCÊ FOI DEVORADO</b></size></color>\n";

            // 2. Subtítulo charmoso
            string subtitle = "<size=80%><color=#AAAAAA>Resumo da Partida</color></size>\n\n";

            // 3. Monta as estatísticas base
            string stats = $"Tempo Sobrevivido: <color=#FFFFFF><b>{timeFormatted}</b></color>\n";
            stats += $"Zonas Capturadas: <color=#FFAA00><b>{GameController.zonesCompleted}/4</b></color>\n";

            // 4. Informação exclusiva dependendo se ganhou ou perdeu
            if (isWin)
            {
                // Se ganhou, mostramos o quão bem ele jogou (quanta vida sobrou)
                stats += $"Vida Restante: <color=#FF5555><b>{GameController.PlayerHealth}/20</b></color>";
            }
            else
            {
                // Se perdeu, lembramos qual foi o último poder que ele conseguiu pegar antes de morrer
                string lastBuff = "Nenhum";
                if (GameController.zonesCompleted == 1) lastBuff = "Granada";
                if (GameController.zonesCompleted == 2) lastBuff = "Dano da Arma";
                if (GameController.zonesCompleted == 3) lastBuff = "Velocidade de Movimento";
                
                stats += $"Último Upgrade: <color=#55AAFF><b>{lastBuff}</b></color>";
            }

            // Junta tudo e joga no painel!
            finalTimeText.text = title + subtitle + stats;
        }
    }