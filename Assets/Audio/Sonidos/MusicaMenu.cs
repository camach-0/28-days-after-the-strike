using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicaMenu : MonoBehaviour
{
    private static MusicaMenu instancia;
    private AudioSource fuenteAudio;

    void Awake()
    {
        // Esto evita que la música se duplique si vuelves al menú principal desde el juego
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // Hace que sobreviva al cambio de escena
            fuenteAudio = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Se autodestruye al entrar a la escena de acción
        if (SceneManager.GetActiveScene().name == "Escena_3_Juego")
        {
            Destroy(gameObject);
        }
    }
}