using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Configuración de Slots")]
    public GameObject[] slotsVisuales; // Arrastra aquí tus 4 paneles de slots
    public Button botonEmpezar;

    private int jugadoresUnidos = 0;

    private void Start()
    {
        // Al empezar, todos los slots muestran "Presiona Start"
        ActualizarInterfaz();
    }

    // Este método lo llamaremos cuando el PlayerInputManager detecte un nuevo mando/teclado
    public void AlUnirseJugador()
    {
        if (jugadoresUnidos < 4)
        {
            jugadoresUnidos++;
            ActualizarInterfaz();

            // Si hay al menos uno, ya podemos jugar
            botonEmpezar.interactable = true;
        }
    }

    private void ActualizarInterfaz()
    {
        for (int i = 0; i < slotsVisuales.Length; i++)
        {
            if (i < jugadoresUnidos)
            {
                // Slot activado (Jugador Humano)
                slotsVisuales[i].GetComponent<Image>().color = Color.white;
                slotsVisuales[i].transform.GetChild(0).GetComponent<Text>().text = "LISTO";
            }
            else
            {
                // Slot vacío (Será un Bot en la Escena 3)
                slotsVisuales[i].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
                slotsVisuales[i].transform.GetChild(0).GetComponent<Text>().text = "BOT";
            }
        }
    }

    public void ConfirmarYJugar()
    {
        // GUARDAMOS EL DATO EN LA MEMORIA GLOBAL
        DatosGlobales.cantidadJugadoresHumanos = jugadoresUnidos;

        // Saltamos a la escena del laberinto
        SceneManager.LoadScene("Escena_3_Juego");
    }
}