using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GestorSupervivientes : MonoBehaviour
{
    [Header("El Equipo (Debe coincidir con el orden del Lobby)")]
    public GameObject[] personajesEnEscena;

    [Header("Cámaras Reales (Con Cinemachine Brain)")]
    public Camera[] camaras;

    [Header("Cámaras Virtuales (Cinemachine)")]
    public CinemachineCamera[] camarasVirtuales;

    private void Start()
    {
        AsignarControles();
    }

    private void AsignarControles()
    {
        int humanos = DatosGlobales.cantidadJugadoresHumanos;

        foreach (Camera cam in camaras) if (cam != null) cam.gameObject.SetActive(false);
        foreach (CinemachineCamera vCam in camarasVirtuales) if (vCam != null) vCam.gameObject.SetActive(false);

        for (int i = 0; i < personajesEnEscena.Length; i++)
        {
            GameObject personaje = personajesEnEscena[i];
            PlayerInput inputHumano = personaje.GetComponent<PlayerInput>();
            JugadorController scriptJugador = personaje.GetComponent<JugadorController>();

            bool esControladoPorHumano = false;
            int idDelHumano = -1;

            for (int j = 0; j < humanos; j++)
            {
                if (DatosGlobales.personajesSeleccionados[j] == i)
                {
                    esControladoPorHumano = true;
                    idDelHumano = j;
                    break;
                }
            }

            if (esControladoPorHumano)
            {
                if (scriptJugador != null)
                {
                    scriptJugador.ConfigurarRol(true);
                    // ¡AQUÍ ESTÁ LA MAGIA! Le inyectamos su ID oficial
                    scriptJugador.idJugador = idDelHumano;
                }

                if (inputHumano != null)
                {
                    var mandosGuardados = DatosGlobales.dispositivosPorJugador[idDelHumano];
                    var esquemaGuardado = DatosGlobales.esquemasControlPorJugador[idDelHumano];

                    if (mandosGuardados != null && !string.IsNullOrEmpty(esquemaGuardado))
                    {
                        inputHumano.SwitchCurrentControlScheme(esquemaGuardado, mandosGuardados);
                    }
                }

                personaje.name = DatosGlobales.nombresPersonajes[i];

                if (camaras.Length > idDelHumano && camaras[idDelHumano] != null)
                {
                    camaras[idDelHumano].gameObject.SetActive(true);

                    if (camarasVirtuales.Length > idDelHumano && camarasVirtuales[idDelHumano] != null)
                    {
                        CinemachineCamera miDron = camarasVirtuales[idDelHumano];
                        miDron.gameObject.SetActive(true);
                        miDron.Follow = personaje.transform;
                    }

                    if (scriptJugador != null) scriptJugador.camaraPrincipal = camaras[idDelHumano];

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
                if (scriptJugador != null)
                {
                    scriptJugador.ConfigurarRol(false);
                    scriptJugador.idJugador = -1; // Nos aseguramos de que el bot no robe puntos
                }

                personaje.name = DatosGlobales.nombresPersonajes[i] + " (Bot)";

                Canvas canvasDelBot = personaje.GetComponentInChildren<Canvas>();
                if (canvasDelBot != null)
                {
                    canvasDelBot.gameObject.SetActive(false);
                }
            }
        }

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