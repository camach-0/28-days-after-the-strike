using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Estado de la Partida")]
    public List<Entidad> jugadoresEnEscena = new List<Entidad>();
    public bool juegoTerminado = false;

    [Header("Interfaz Visual")]
    public GameObject panelGameOver; // ¡NUEVO! Conexión directa a la pantalla negra

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public void RegistrarJugador(Entidad nuevoJugador)
    {
        if (!jugadoresEnEscena.Contains(nuevoJugador))
        {
            jugadoresEnEscena.Add(nuevoJugador);
        }
    }

    public void VerificarEstadoJugadores()
    {
        if (juegoTerminado) return;

        bool alguienVivo = false;
        foreach (Entidad jugador in jugadoresEnEscena)
        {
            // Verificamos que el jugador exista y siga vivo
            if (jugador != null && !jugador.estaMuerto)
            {
                alguienVivo = true;
                break;
            }
        }

        if (!alguienVivo)
        {
            EjecutarGameOver();
        }
    }

    private void EjecutarGameOver()
    {
        juegoTerminado = true;
        Debug.Log("<color=red>¡GAME OVER! Todos los jugadores han caído.</color>");

        // Encendemos la pantalla negra directamente, sin depender de otros scripts
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
        else
        {
            Debug.LogError("ERROR: ¡Falta arrastrar el Panel de Game Over al GameManager en el Inspector!");
        }
    }

    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}