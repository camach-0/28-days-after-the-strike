using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // <-- Agregamos esta librería para TextMeshPro (muy común en Unity 6)

public class LobbyManager : MonoBehaviour
{
    [Header("Configuración de Slots")]
    public GameObject[] slotsVisuales;
    public Button botonEmpezar;

    private int jugadoresUnidos = 0;

    private void Start()
    {
        ActualizarInterfaz();
    }

    public void AlUnirseJugador()
    {
        if (jugadoresUnidos < 4)
        {
            jugadoresUnidos++;
            ActualizarInterfaz();
            botonEmpezar.interactable = true;
        }
    }

    private void ActualizarInterfaz()
    {
        for (int i = 0; i < slotsVisuales.Length; i++)
        {
            // 1. Cambiamos el color del panel de fondo
            slotsVisuales[i].GetComponent<Image>().color = (i < jugadoresUnidos) ? Color.white : new Color(0.3f, 0.3f, 0.3f);

            // 2. Buscamos al hijo por su nombre EXACTO, sin importar su orden
            Transform objetoTexto = slotsVisuales[i].transform.Find("STAR");

            if (objetoTexto != null)
            {
                string mensaje = (i < jugadoresUnidos) ? "LISTO" : "BOT";

                // Intenta cambiarlo si es un Texto Clásico
                Text textoClasico = objetoTexto.GetComponent<Text>();
                if (textoClasico != null) textoClasico.text = mensaje;

                // Intenta cambiarlo si es un TextMeshPro
                TMP_Text textoModerno = objetoTexto.GetComponent<TMP_Text>();
                if (textoModerno != null) textoModerno.text = mensaje;
            }
            else
            {
                Debug.LogWarning("¡Cuidado! El panel " + slotsVisuales[i].name + " no tiene un hijo llamado 'STAR'.");
            }
        }
    }

    public void ConfirmarYJugar()
    {
        DatosGlobales.cantidadJugadoresHumanos = jugadoresUnidos;
        SceneManager.LoadScene("Escena_3_Juego");
    }
}