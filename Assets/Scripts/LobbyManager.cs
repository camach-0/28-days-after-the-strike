using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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


    [Header("Sistema de Carga")]
    public CargadorEscenas sistemaCarga;

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
        for (int i = 0; i < slotsVisuales.Length; i++)
        {
            GameObject panel = slotsVisuales[i];
            Transform objetoFoto = panel.transform.Find("FOTO");
            Transform objetoTexto = panel.transform.Find("STAR");

            Image imagenFoto = objetoFoto != null ? objetoFoto.GetComponent<Image>() : null;
            TMP_Text textoStar = objetoTexto != null ? objetoTexto.GetComponent<TMP_Text>() : null;

            bool hayAlguienAqui = false;
            bool estanListos = false;
            string nombresJugadores = "";

            foreach (ControladorLobby jug in jugadoresConectados)
            {
                if (jug.indicePersonajeActual == i)
                {
                    hayAlguienAqui = true;
                    if (jug.estaListo) estanListos = true;
                    nombresJugadores += "P" + (jug.miIDJugador + 1) + " ";
                }
            }

            if (imagenFoto != null)
            {
                if (hayAlguienAqui)
                {
                    if (fotosColor.Length > i) imagenFoto.sprite = fotosColor[i];
                    imagenFoto.color = Color.white;
                }
                else
                {
                    if (fotosBlancoYNegro.Length > i) imagenFoto.sprite = fotosBlancoYNegro[i];
                    imagenFoto.color = new Color(0.7f, 0.7f, 0.7f);
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
                    textoStar.text = "";
                }
            }
        }
    }

    public void ComprobarTodosListos()
    {
        if (jugadoresConectados.Count == 0) return;

        bool todosListos = true;
        foreach (ControladorLobby jug in jugadoresConectados)
        {
            if (!jug.estaListo) todosListos = false;
        }

        botonEmpezar.interactable = todosListos;

        if (todosListos)
        {
            StartCoroutine(MoverFocoAlBoton());
        }
        else
        {
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == botonEmpezar.gameObject)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private IEnumerator MoverFocoAlBoton()
    {
        yield return new WaitForSeconds(0.2f);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(botonEmpezar.gameObject);
    }

    public void ConfirmarYJugar()
    {
      
        sistemaCarga.IniciarCarga("Escena_3_Juego");
    }

    public bool EstaPersonajeLibre(int indiceFoto)
    {
        foreach (ControladorLobby jug in jugadoresConectados)
        {
            if (jug.indicePersonajeActual == indiceFoto && jug.estaListo)
            {
                return false;
            }
        }
        return true;
    }

    public void VolverAlMenu()
    {
        DatosGlobales.cantidadJugadoresHumanos = 0;
        SceneManager.LoadScene("Escena_1_Menu");
    }
}