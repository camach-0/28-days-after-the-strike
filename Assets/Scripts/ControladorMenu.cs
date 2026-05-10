using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escenas

public class ControladorMenu : MonoBehaviour
{
    public void IrAlLobby()
    {
        // Carga la Escena 2 por su nombre exacto
        SceneManager.LoadScene("Escena_2_Lobby");
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }
}