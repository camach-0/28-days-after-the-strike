using UnityEngine;
using UnityEngine.InputSystem; // Necesario para apagar/encender controles

public class GestorSupervivientes : MonoBehaviour
{
    [Header("El Equipo (Orden: P1, P2, P3, P4)")]
    public GameObject[] personajesEnEscena;

    private void Start()
    {
        AsignarControles();
    }

    private void AsignarControles()
    {
        int humanos = DatosGlobales.cantidadJugadoresHumanos;
        Debug.Log("Iniciando nivel con " + humanos + " jugadores humanos.");

        for (int i = 0; i < personajesEnEscena.Length; i++)
        {
            GameObject personaje = personajesEnEscena[i];
            PlayerInput inputHumano = personaje.GetComponent<PlayerInput>();
            JugadorController scriptHumano = personaje.GetComponent<JugadorController>();

            AliadoBotController cerebroBot = personaje.GetComponent<AliadoBotController>();
            UnityEngine.AI.NavMeshAgent agente = personaje.GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (i < humanos)
            {
                // --- ES UN HUMANO ---
                if (inputHumano != null) inputHumano.enabled = true;
                if (scriptHumano != null) scriptHumano.enabled = true;

                // VITAL: Apagamos el cerebro Bot y su motor para que no peleen con tu teclado
                if (cerebroBot != null) cerebroBot.enabled = false;
                if (agente != null) agente.enabled = false;

                personaje.name += " (Humano)";

                // ARREGLO DE LA CÁMARA: Si es el Player 1 (i == 0), le decimos a la cámara que lo siga.
                // *Nota: Dependiendo de cómo programaste tu cámara, esto puede variar. 
                // Si usas un script propio llamado "CamaraController", sería algo así:
                if (i == 0)
                {
                    Camera.main.transform.SetParent(personaje.transform);
                    Camera.main.transform.localPosition = new Vector3(0, 0, -10); // Ajusta la distancia Z
                }
            }
            else
            {
                // --- ES UN BOT ---
                if (inputHumano != null) inputHumano.enabled = false;
                if (scriptHumano != null) scriptHumano.enabled = false;

                // Encendemos su IA
                if (cerebroBot != null) cerebroBot.enabled = true;
                if (agente != null) agente.enabled = true;

                personaje.name += " (Bot)";
            }
        }
    }
}