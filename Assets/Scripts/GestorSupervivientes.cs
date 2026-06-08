using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // ¡NUEVO! Necesario para buscar las cámaras virtuales

public class GestorSupervivientes : MonoBehaviour
{
    [Header("El Equipo (Debe coincidir con el orden del Lobby)")]
    [Tooltip("0=Cholo, 1=Colla, 2=Camba, 3=Chola")]
    public GameObject[] personajesEnEscena;

    [Header("Cámaras Reales (Con Cinemachine Brain)")]
    public Camera[] camaras;

    // ¡NUEVO! Arrastra aquí tus 4 vCam desde el Inspector
    [Header("Cámaras Virtuales (Cinemachine)")]
    public CinemachineCamera[] camarasVirtuales;

    private void Start()
    {
        AsignarControles();
    }

    private void AsignarControles()
    {
        int humanos = DatosGlobales.cantidadJugadoresHumanos;
        Debug.Log("Iniciando nivel con " + humanos + " jugadores humanos.");

        // 1. Apagamos TODAS las cámaras primero por seguridad (Reales y Virtuales)
        foreach (Camera cam in camaras)
        {
            if (cam != null) cam.gameObject.SetActive(false);
        }
        foreach (CinemachineCamera vCam in camarasVirtuales)
        {
            if (vCam != null) vCam.gameObject.SetActive(false);
        }

        // 2. Configuramos a los 4 personajes uno por uno
        for (int i = 0; i < personajesEnEscena.Length; i++)
        {
            GameObject personaje = personajesEnEscena[i];
            PlayerInput inputHumano = personaje.GetComponent<PlayerInput>();
            JugadorController scriptHumano = personaje.GetComponent<JugadorController>();
            AliadoBotController cerebroBot = personaje.GetComponent<AliadoBotController>();
            UnityEngine.AI.NavMeshAgent agente = personaje.GetComponent<UnityEngine.AI.NavMeshAgent>();

            // 3. Verificamos si alguien en el Lobby eligió a este personaje
            bool esControladoPorHumano = false;
            int idDelHumano = -1; // Nos dirá si fue el P1 (0), P2 (1), etc.

            for (int j = 0; j < humanos; j++)
            {
                if (DatosGlobales.personajesSeleccionados[j] == i)
                {
                    esControladoPorHumano = true;
                    idDelHumano = j;
                    break;
                }
            }

            // 4. Asignamos los roles
            if (esControladoPorHumano)
            {
                // --- ES UN HUMANO ---
                if (inputHumano != null)
                {
                    inputHumano.enabled = true;
                    var mandosGuardados = DatosGlobales.dispositivosPorJugador[idDelHumano];
                    var esquemaGuardado = DatosGlobales.esquemasControlPorJugador[idDelHumano];

                    if (mandosGuardados != null && !string.IsNullOrEmpty(esquemaGuardado))
                    {
                        inputHumano.SwitchCurrentControlScheme(esquemaGuardado, mandosGuardados);
                    }
                }

                if (scriptHumano != null) scriptHumano.enabled = true;
                if (cerebroBot != null) cerebroBot.enabled = false;
                if (agente != null) agente.enabled = false;

                personaje.name += " (Humano P" + (idDelHumano + 1) + ")";

                // --- SISTEMA DE CÁMARAS CINEMACHINE ---
                if (camaras.Length > idDelHumano && camaras[idDelHumano] != null)
                {
                    // 1. Encendemos su Cámara Real (La pantalla)
                    camaras[idDelHumano].gameObject.SetActive(true);

                    // ¡ELIMINADO EL SETPARENT! Ahora la cámara real se queda donde está, el Brain la moverá.
                    // camaras[idDelHumano].transform.SetParent(personaje.transform);

                    // 2. Encendemos y Configuramos su Cámara Virtual (El Dron)
                    if (camarasVirtuales.Length > idDelHumano && camarasVirtuales[idDelHumano] != null)
                    {
                        CinemachineCamera miDron = camarasVirtuales[idDelHumano];
                        miDron.gameObject.SetActive(true);
                        // Le decimos al dron que siga y mire a ESTE jugador
                        miDron.Follow = personaje.transform;
                        //miDron.LookAt = personaje.transform; // (Opcional, en 2D a veces no se usa LookAt)
                    }

                    scriptHumano.camaraPrincipal = camaras[idDelHumano];

                    Canvas canvasDelJugador = personaje.GetComponentInChildren<Canvas>();
                    if (canvasDelJugador != null)
                    {
                        canvasDelJugador.worldCamera = camaras[idDelHumano];
                        canvasDelJugador.planeDistance = 1f;
                    }
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

                Canvas canvasDelBot = personaje.GetComponentInChildren<Canvas>();
                if (canvasDelBot != null)
                {
                    canvasDelBot.gameObject.SetActive(false);
                }
            }
        }

        // 5. Cortamos la pantalla
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