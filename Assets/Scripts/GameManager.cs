using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Estado de la Partida")]
    public List<SistemaSalud> supervivientesActivos = new List<SistemaSalud>();
    public bool juegoTerminado = false;

    [Header("Interfaz Visual")]
    public GameObject panelGameOver;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Monitorizamos constantemente el estado de la partida mientras no sea Game Over
        if (!juegoTerminado)
        {
            VerificarEstadoJugadores();
        }
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

        int humanosTotales = 0;
        int humanosCaidos = 0;

        foreach (SistemaSalud superviviente in supervivientesActivos)
        {
            if (superviviente != null)
            {
                // ¡CORRECCIÓN! Un humano real es el que tiene el Input (Mando/Teclado) encendido
                UnityEngine.InputSystem.PlayerInput humano = superviviente.GetComponent<UnityEngine.InputSystem.PlayerInput>();

                if (humano != null && humano.enabled)
                {
                    humanosTotales++;

                    if (superviviente.estaIncapacitado || superviviente.vidaActual <= 0)
                    {
                        humanosCaidos++;
                    }
                }
            }
        }

        if (humanosTotales > 0 && humanosCaidos == humanosTotales)
        {
            EjecutarGameOver();
        }
    }

    private void EjecutarGameOver()
    {
        juegoTerminado = true;
        Debug.Log("<color=red>¡GAME OVER! Los jugadores reales han sido abatidos.</color>");

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        // Nota: En L4D la acción física no se congela de inmediato para dar dramatismo,
        // pero si prefieres congelar el juego por completo al perder, descomenta la siguiente línea:
        // Time.timeScale = 0f;
    }

    public void ReiniciarNivel()
    {
        // Si congelaste el tiempo en EjecutarGameOver, asegúrate de restaurarlo al reiniciar:
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}