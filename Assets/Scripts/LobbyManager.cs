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
        // 1. Apagamos todos los paneles
        for (int i = 0; i < slotsVisuales.Length; i++)
        {
            slotsVisuales[i].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f); // Gris
            Transform objetoTexto = slotsVisuales[i].transform.Find("STAR");
            if (objetoTexto != null)
            {
                TMP_Text texto = objetoTexto.GetComponent<TMP_Text>();
                if (texto != null) texto.text = "VACÍO";
            }
        }

        // 2. Pintamos los paneles donde haya un jugador
        foreach (ControladorLobby jug in jugadoresConectados)
        {
            GameObject panelActual = slotsVisuales[jug.indicePersonajeActual];

            // Amarillo = Eligiendo, Verde = Listo
            panelActual.GetComponent<Image>().color = jug.estaListo ? Color.green : Color.yellow;

            Transform objetoTexto = panelActual.transform.Find("STAR");
            if (objetoTexto != null)
            {
                TMP_Text texto = objetoTexto.GetComponent<TMP_Text>();
                if (texto != null)
                {
                    string nombreJugador = "P" + (jug.miIDJugador + 1);
                    texto.text = jug.estaListo ? nombreJugador + " LISTO" : nombreJugador + " ELIGIENDO...";
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