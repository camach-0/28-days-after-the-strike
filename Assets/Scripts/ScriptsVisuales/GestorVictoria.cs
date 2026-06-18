using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;

public class GestorVictoria : MonoBehaviour
{
    public static GestorVictoria Instancia;

    [Header("Puertas del Ascensor")]
    public RectTransform puertaIzquierda;
    public RectTransform puertaDerecha;
    public float velocidadCierre = 1500f; 

    [Header("Interfaz Final")]
    public GameObject panelEstadisticas;
    public Button botonVolver;

    public TMP_Text textoZombiesMuertos;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
    }

    public void IniciarSecuenciaVictoria()
    {
       
        foreach (PlayerInput jugador in PlayerInput.all)
        {
            jugador.SwitchCurrentActionMap("UI");
        }

        StartCoroutine(RutinaVictoria());
    }

    private IEnumerator RutinaVictoria()
    {
       
        while (puertaIzquierda.anchoredPosition.x < 0 || puertaDerecha.anchoredPosition.x > 0)
        {
            puertaIzquierda.anchoredPosition = Vector2.MoveTowards(puertaIzquierda.anchoredPosition, new Vector2(0, 0), velocidadCierre * Time.deltaTime);
            puertaDerecha.anchoredPosition = Vector2.MoveTowards(puertaDerecha.anchoredPosition, new Vector2(0, 0), velocidadCierre * Time.deltaTime);
            yield return null;
        }

      
        yield return new WaitForSeconds(1f);

      
        panelEstadisticas.SetActive(true);

        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(botonVolver.gameObject);

    
        Time.timeScale = 0f;
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f; //
        DatosGlobales.cantidadJugadoresHumanos = 0;
        SceneManager.LoadScene("Escena_1_Menu"); 
    }
}