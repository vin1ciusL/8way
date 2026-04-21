using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    public void IniciaJogo()
    {
        GameController.Init();
        SceneManager.LoadScene(1); // Vai para o jogo (Cena 1)
    }

    public void Menu()
    {
        SceneManager.LoadScene(0); // Volta para o Menu (Cena 0)
    }
}