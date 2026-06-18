using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class GestorPausa : MonoBehaviour
{
    public static GestorPausa Instancia;

    [Header("Interfaz de Pausa")]
    public GameObject panelPausa;
    public GameObject botonReanudar;

    [HideInInspector]
    public bool juegoPausado = false;

    private PlayerInput jugadorQuePauso;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
    }

    public void PausarJuego(PlayerInput inputDelJugador)
    {
        juegoPausado = true;
        Time.timeScale = 0f;
        panelPausa.SetActive(true);

        jugadorQuePauso = inputDelJugador;
        if (jugadorQuePauso != null)
        {
            jugadorQuePauso.SwitchCurrentActionMap("UI");
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(botonReanudar);
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f;
        panelPausa.SetActive(false);

 
        if (jugadorQuePauso != null)
        {
            jugadorQuePauso.SwitchCurrentActionMap("Jugador");
            jugadorQuePauso = null;
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        DatosGlobales.cantidadJugadoresHumanos = 0;
        SceneManager.LoadScene("Escena_1_Menu");
    }
}