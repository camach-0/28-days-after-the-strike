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

    [Tooltip("El texto largo donde se imprimirá todo el reporte")]
    public TMP_Text textoReporte;

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

        GenerarReporteFinal();

        panelEstadisticas.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(botonVolver.gameObject);

        Time.timeScale = 0f;
    }

    private void GenerarReporteFinal()
    {
        string reporte = "<color=yellow>¡NIVEL COMPLETADO!</color>\n\n";

        int idIntocable = -1; float minDano = 99999f;
        int idCarnicero = -1; int maxMelee = 0;
        int idPacifista = -1; int minKills = 99999;

        for (int i = 0; i < 4; i++)
        {
         
            if (DatosGlobales.personajesSeleccionados[i] != -1)
            {
                reporte += $"<b>JUGADOR {i + 1} ({DatosGlobales.nombresPersonajes[DatosGlobales.personajesSeleccionados[i]]})</b>\n";
                reporte += $"Infectados: {DatosGlobales.statsZombiesMuertos[i]} | Especiales: {DatosGlobales.statsEspecialesMuertos[i]} | Melee: {DatosGlobales.statsBajasMelee[i]}\n";
                reporte += $"Daño Recibido: {DatosGlobales.statsDanoRecibido[i]} | Caídas: {DatosGlobales.statsVecesMuerto[i]}\n\n";

              
                if (DatosGlobales.statsDanoRecibido[i] < minDano) { minDano = DatosGlobales.statsDanoRecibido[i]; idIntocable = i; }
                if (DatosGlobales.statsBajasMelee[i] > maxMelee) { maxMelee = DatosGlobales.statsBajasMelee[i]; idCarnicero = i; }
                if (DatosGlobales.statsZombiesMuertos[i] < minKills) { minKills = DatosGlobales.statsZombiesMuertos[i]; idPacifista = i; }
            }
        }

    
        reporte += "<color=orange>--- RECONOCIMIENTOS DESTACADOS ---</color>\n";

        if (idIntocable != -1)
            reporte += $"<b>EL INTOCABLE:</b> Jugador {idIntocable + 1} (Solo {minDano} pts de daño)\n";

        if (idCarnicero != -1 && maxMelee > 0)
            reporte += $"<b>EL CARNICERO:</b> Jugador {idCarnicero + 1} ({maxMelee} muertes cuerpo a cuerpo)\n";

        if (idPacifista != -1)
            reporte += $"<b>EL PACIFISTA:</b> Jugador {idPacifista + 1} (Solo mató a {minKills} infectados)\n";

       
        if (textoReporte != null) textoReporte.text = reporte;
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        DatosGlobales.cantidadJugadoresHumanos = 0;
        DatosGlobales.LimpiarEstadisticas(); 
        SceneManager.LoadScene("Escena_1_Menu");
    }
}