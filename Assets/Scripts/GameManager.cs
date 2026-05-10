using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar el nivel

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia; // Singleton

    [Header("Estado de la Partida")]
    public List<Entidad> jugadoresEnEscena = new List<Entidad>();
    public bool juegoTerminado = false;

    private void Awake()
    {
        // Configuramos el Singleton
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public void RegistrarJugador(Entidad nuevoJugador)
    {
        if (!jugadoresEnEscena.Contains(nuevoJugador))
        {
            jugadoresEnEscena.Add(nuevoJugador);
            Debug.Log("Un nuevo jugador ha entrado a la partida. Total: " + jugadoresEnEscena.Count);
        }
    }

    // El jugador llamará a este método cuando su vida llegue a 0
    public void VerificarEstadoJugadores()
    {
        if (juegoTerminado) return;

        bool alguienVivo = false;

        foreach (Entidad jugador in jugadoresEnEscena)
        {
            if (!jugador.estaMuerto)
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

        // Aquí le avisamos al UIManager que muestre la pantalla negra
        // UIManager.Instancia.MostrarPantallaGameOver();
    }

    // Este método lo llamaremos desde un botón en la pantalla de Game Over
    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}