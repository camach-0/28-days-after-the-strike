using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instancia;

    [Header("Configuración de Slots")]
    public GameObject[] slotsVisuales;
    public Button botonEmpezar;

    [Header("Estilo Metal Slug (Fotos)")]
    [Tooltip("Las 4 fotos en BLANCO Y NEGRO (Orden: Cholo, Colla, Camba, Chola)")]
    public Sprite[] fotosBlancoYNegro;
    [Tooltip("Las 4 fotos a TODO COLOR (Orden: Cholo, Colla, Camba, Chola)")]
    public Sprite[] fotosColor;

    private List<ControladorLobby> jugadoresConectados = new List<ControladorLobby>();

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
    }

    private void Start()
    {
        botonEmpezar.interactable = false;
        DatosGlobales.cantidadJugadoresHumanos = 0;
        ActualizarVisuales();
    }

    public int RegistrarNuevoJugador(ControladorLobby nuevoJugador)
    {
        int nuevoID = jugadoresConectados.Count;
        jugadoresConectados.Add(nuevoJugador);
        DatosGlobales.cantidadJugadoresHumanos = jugadoresConectados.Count;
        return nuevoID;
    }

    public void ActualizarVisuales()
    {
        // Revisamos los 4 paneles (0 al 3)
        for (int i = 0; i < slotsVisuales.Length; i++)
        {
            GameObject panel = slotsVisuales[i];

            // Buscamos la foto y el texto dentro de este panel
            Transform objetoFoto = panel.transform.Find("FOTO");
            Transform objetoTexto = panel.transform.Find("STAR");

            Image imagenFoto = objetoFoto != null ? objetoFoto.GetComponent<Image>() : null;
            TMP_Text textoStar = objetoTexto != null ? objetoTexto.GetComponent<TMP_Text>() : null;

            // Variables para saber qué pasa en este panel específico
            bool hayAlguienAqui = false;
            bool estanListos = false;
            string nombresJugadores = "";

            // Preguntamos a todos los jugadores si están parados en este panel
            foreach (ControladorLobby jug in jugadoresConectados)
            {
                if (jug.indicePersonajeActual == i)
                {
                    hayAlguienAqui = true;
                    if (jug.estaListo) estanListos = true;
                    nombresJugadores += "P" + (jug.miIDJugador + 1) + " "; // Ej: "P1 " o "P1 P2 "
                }
            }

            // APLICAMOS EL EFECTO METAL SLUG
            if (imagenFoto != null)
            {
                if (hayAlguienAqui)
                {
                    // Alguien está encima: PONEMOS LA FOTO A COLOR
                    if (fotosColor.Length > i) imagenFoto.sprite = fotosColor[i];
                    imagenFoto.color = Color.white; // Color puro
                }
                else
                {
                    // Nadie está encima: PONEMOS LA FOTO EN BLANCO Y NEGRO
                    if (fotosBlancoYNegro.Length > i) imagenFoto.sprite = fotosBlancoYNegro[i];
                    imagenFoto.color = new Color(0.7f, 0.7f, 0.7f); // Un poco oscurecido para dar contraste
                }
            }

            if (textoStar != null)
            {
                if (hayAlguienAqui)
                {
                    textoStar.text = estanListos ? nombresJugadores + "LISTO" : nombresJugadores;
                    textoStar.color = estanListos ? Color.green : Color.yellow;
                }
                else
                {
                    textoStar.text = ""; // Si no hay nadie, ocultamos el texto de "P1"
                }
            }
        }
    }

    public void ComprobarTodosListos()
    {
        bool todosListos = true;
        foreach (ControladorLobby jug in jugadoresConectados)
        {
            if (!jug.estaListo) todosListos = false;
        }

        if (jugadoresConectados.Count > 0 && todosListos)
        {
            botonEmpezar.interactable = true;
        }
    }

    public void ConfirmarYJugar()
    {
        SceneManager.LoadScene("Escena_3_Juego");
    }
}