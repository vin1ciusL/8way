using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    public void IniciaJogo()
    {
        GameController.Init();
        SceneManager.LoadScene(0);
    }

    public void Menu()
    {
        SceneManager.LoadScene(1);
    }
}