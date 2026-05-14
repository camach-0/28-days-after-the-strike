using UnityEngine;
using UnityEngine.InputSystem;

public class GestorSupervivientes : MonoBehaviour
{
    [Header("El Equipo (Debe coincidir con el orden del Lobby)")]
    [Tooltip("0=Cholo, 1=Colla, 2=Camba, 3=Chola")]
    public GameObject[] personajesEnEscena;

    [Header("Cámaras")]
    public Camera[] camaras;

    private void Start()
    {
        AsignarControles();
    }

    private void AsignarControles()
    {
        int humanos = DatosGlobales.cantidadJugadoresHumanos;
        Debug.Log("Iniciando nivel con " + humanos + " jugadores humanos.");

        // 1. Apagamos TODAS las cámaras primero por seguridad
        foreach (Camera cam in camaras)
        {
            if (cam != null) cam.gameObject.SetActive(false);
        }

        // 2. Configuramos a los 4 personajes uno por uno
        for (int i = 0; i < personajesEnEscena.Length; i++)
        {
            GameObject personaje = personajesEnEscena[i];
            PlayerInput inputHumano = personaje.GetComponent<PlayerInput>();
            JugadorController scriptHumano = personaje.GetComponent<JugadorController>();
            AliadoBotController cerebroBot = personaje.GetComponent<AliadoBotController>();
            UnityEngine.AI.NavMeshAgent agente = personaje.GetComponent<UnityEngine.AI.NavMeshAgent>();

            // 3. Verificamos si alguien en el Lobby eligió a este personaje específico (índice 'i')
            bool esControladoPorHumano = false;
            int idDelHumano = -1; // Nos dirá si fue el P1 (0), P2 (1), etc.

            for (int j = 0; j < humanos; j++)
            {
                if (DatosGlobales.personajesSeleccionados[j] == i)
                {
                    esControladoPorHumano = true;
                    idDelHumano = j; // Guardamos qué jugador lo eligió
                    break;
                }
            }

            // 4. Asignamos los roles
            if (esControladoPorHumano)
            {
                // --- ES UN HUMANO ---
                if (inputHumano != null) inputHumano.enabled = true;
                if (scriptHumano != null) scriptHumano.enabled = true;
                if (cerebroBot != null) cerebroBot.enabled = false;
                if (agente != null) agente.enabled = false;

                personaje.name += " (Humano P" + (idDelHumano + 1) + ")";

                // Le damos su cámara correspondiente (P1 recibe Camara_P1, P2 recibe Camara_P2...)
                if (camaras.Length > idDelHumano && camaras[idDelHumano] != null)
                {
                    camaras[idDelHumano].gameObject.SetActive(true); // La encendemos
                    camaras[idDelHumano].transform.SetParent(personaje.transform);
                    camaras[idDelHumano].transform.localPosition = new Vector3(0, 0, -10);
                }
            }
            else
            {
                // --- ES UN BOT ---
                if (inputHumano != null) inputHumano.enabled = false;
                if (scriptHumano != null) scriptHumano.enabled = false;
                if (cerebroBot != null) cerebroBot.enabled = true;
                if (agente != null) agente.enabled = true;

                personaje.name += " (Bot)";
            }
        }

        // 5. Cortamos la pantalla según la cantidad de humanos reales
        ConfigurarPantallaDividida(humanos);
    }

    private void ConfigurarPantallaDividida(int cantidadHumanos)
    {
        switch (cantidadHumanos)
        {
            case 1:
                camaras[0].rect = new Rect(0f, 0f, 1f, 1f);
                break;
            case 2:
                camaras[0].rect = new Rect(0f, 0f, 0.5f, 1f);
                camaras[1].rect = new Rect(0.5f, 0f, 0.5f, 1f);
                break;
            case 3:
                camaras[0].rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
                camaras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                camaras[2].rect = new Rect(0.25f, 0f, 0.5f, 0.5f);
                break;
            case 4:
                camaras[0].rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
                camaras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                camaras[2].rect = new Rect(0f, 0f, 0.5f, 0.5f);
                camaras[3].rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
                break;
        }
    }
}