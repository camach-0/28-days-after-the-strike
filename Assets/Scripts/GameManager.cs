using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Estado de la Partida")]
    // Ahora guardamos el SistemaSalud de todos los supervivientes en escena
    public List<SistemaSalud> supervivientesActivos = new List<SistemaSalud>();
    public bool juegoTerminado = false;

    [Header("Interfaz Visual")]
    public GameObject panelGameOver;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public void RegistrarSuperviviente(SistemaSalud nuevoSuperviviente)
    {
        if (!supervivientesActivos.Contains(nuevoSuperviviente))
        {
            supervivientesActivos.Add(nuevoSuperviviente);
        }
    }

    public void DesregistrarSuperviviente(SistemaSalud superviviente)
    {
        if (supervivientesActivos.Contains(superviviente))
        {
            supervivientesActivos.Remove(superviviente);
        }
    }

    public void VerificarEstadoJugadores()
    {
        if (juegoTerminado) return;

        bool alguienVivo = false;

        foreach (SistemaSalud superviviente in supervivientesActivos)
        {
            if (superviviente != null && superviviente.vidaActual > 0)
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

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
    }

    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}