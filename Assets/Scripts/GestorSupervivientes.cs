using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GestorSupervivientes : MonoBehaviour
{
    [Header("El Equipo (Debe coincidir con el orden del Lobby)")]
    [Tooltip("0=Cholo, 1=Colla, 2=Camba, 3=Chola")]
    public GameObject[] personajesEnEscena;

    [Header("Cámaras Cinemachine")]
    public CinemachineCamera[] camaras;

    private void Start()
    {
        AsignarControles();
    }

    private void AsignarControles()
    {
        int humanos = DatosGlobales.cantidadJugadoresHumanos;

        Debug.Log("Iniciando nivel con " + humanos + " jugadores humanos.");

        // APAGAMOS TODAS LAS CAMARAS
        foreach (CinemachineCamera cam in camaras)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(false);
            }
        }

        // CONFIGURAMOS LOS PERSONAJES
        for (int i = 0; i < personajesEnEscena.Length; i++)
        {
            GameObject personaje = personajesEnEscena[i];

            PlayerInput inputHumano = personaje.GetComponent<PlayerInput>();
            JugadorController scriptHumano = personaje.GetComponent<JugadorController>();
            AliadoBotController cerebroBot = personaje.GetComponent<AliadoBotController>();
            UnityEngine.AI.NavMeshAgent agente = personaje.GetComponent<UnityEngine.AI.NavMeshAgent>();

            bool esControladoPorHumano = false;
            int idDelHumano = -1;

            // VERIFICAMOS SI EL PERSONAJE FUE ELEGIDO
            for (int j = 0; j < humanos; j++)
            {
                if (DatosGlobales.personajesSeleccionados[j] == i)
                {
                    esControladoPorHumano = true;
                    idDelHumano = j;
                    break;
                }
            }

            // =====================================================
            // HUMANO
            // =====================================================
            if (esControladoPorHumano)
            {
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

                if (scriptHumano != null)
                    scriptHumano.enabled = true;

                if (cerebroBot != null)
                    cerebroBot.enabled = false;

                if (agente != null)
                    agente.enabled = false;

                personaje.name += " (Humano P" + (idDelHumano + 1) + ")";

                // =====================================================
                // CAMARA CINEMACHINE
                // =====================================================
                if (camaras.Length > idDelHumano && camaras[idDelHumano] != null)
                {
                    CinemachineCamera camaraJugador = camaras[idDelHumano];

                    camaraJugador.gameObject.SetActive(true);

                    // ASIGNAMOS FOLLOW Y LOOKAT
                    camaraJugador.Follow = personaje.transform;
                    camaraJugador.LookAt = personaje.transform;

                    // OBTENEMOS LA CAMARA REAL
                    Camera camaraReal = camaraJugador.GetComponent<Camera>();

                    if (scriptHumano != null && camaraReal != null)
                    {
                        scriptHumano.camaraPrincipal = camaraReal;
                    }

                    // CONFIGURAMOS EL CANVAS
                    Canvas canvasDelJugador = personaje.GetComponentInChildren<Canvas>();

                    if (canvasDelJugador != null && camaraReal != null)
                    {
                        canvasDelJugador.worldCamera = camaraReal;
                        canvasDelJugador.planeDistance = 1f;
                    }
                }
            }
            // =====================================================
            // BOT
            // =====================================================
            else
            {
                if (inputHumano != null)
                    inputHumano.enabled = false;

                if (scriptHumano != null)
                    scriptHumano.enabled = false;

                if (cerebroBot != null)
                    cerebroBot.enabled = true;

                if (agente != null)
                    agente.enabled = true;

                personaje.name += " (Bot)";

                Canvas canvasDelBot = personaje.GetComponentInChildren<Canvas>();

                if (canvasDelBot != null)
                {
                    canvasDelBot.gameObject.SetActive(false);
                }
            }
        }

        // SPLIT SCREEN
        ConfigurarPantallaDividida(humanos);
    }

    private void ConfigurarPantallaDividida(int cantidadHumanos)
    {
        switch (cantidadHumanos)
        {
            case 1:

                camaras[0].GetComponent<Camera>().rect =
                    new Rect(0f, 0f, 1f, 1f);

                break;

            case 2:

                camaras[0].GetComponent<Camera>().rect =
                    new Rect(0f, 0f, 0.5f, 1f);

                camaras[1].GetComponent<Camera>().rect =
                    new Rect(0.5f, 0f, 0.5f, 1f);

                break;

            case 3:

                camaras[0].GetComponent<Camera>().rect =
                    new Rect(0f, 0.5f, 0.5f, 0.5f);

                camaras[1].GetComponent<Camera>().rect =
                    new Rect(0.5f, 0.5f, 0.5f, 0.5f);

                camaras[2].GetComponent<Camera>().rect =
                    new Rect(0.25f, 0f, 0.5f, 0.5f);

                break;

            case 4:

                camaras[0].GetComponent<Camera>().rect =
                    new Rect(0f, 0.5f, 0.5f, 0.5f);

                camaras[1].GetComponent<Camera>().rect =
                    new Rect(0.5f, 0.5f, 0.5f, 0.5f);

                camaras[2].GetComponent<Camera>().rect =
                    new Rect(0f, 0f, 0.5f, 0.5f);

                camaras[3].GetComponent<Camera>().rect =
                    new Rect(0.5f, 0f, 0.5f, 0.5f);

                break;
        }
    }
}