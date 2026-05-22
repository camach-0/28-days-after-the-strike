using UnityEngine;
using System;

public class SistemaSalud : MonoBehaviour, IReceptorDano
{
    [Header("Identificación")]
    [Tooltip("¡IMPORTANTE! Marca esta casilla TRUE solo en tus prefabs de personajes (Cholo, Camba, etc.). En los zombis déjala en FALSE.")]
    public bool esSuperviviente = false;

    [Header("Estadísticas de Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual { get; private set; }

    [Header("I-Frames (Inmunidad)")]
    public float tiempoInmunidad = 0.2f;
    private float ultimoTiempoDano = -100f;

    // PATRÓN OBSERVER: Eventos globales
    public event Action OnMuerte;
    public event Action<float> OnVidaCambiada;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    private void Start()
    {
        // Solución definitiva: El registro se hace aquí porque este script NUNCA se deshabilita
        if (esSuperviviente && GameManager.Instancia != null)
        {
            GameManager.Instancia.RegistrarSuperviviente(this);
        }
    }

    private void OnDestroy()
    {
        if (esSuperviviente && GameManager.Instancia != null)
        {
            GameManager.Instancia.DesregistrarSuperviviente(this);
        }
    }

    public void RecibirDano(float cantidad, Vector2 direccion, float fuerza)
    {
        if (vidaActual <= 0) return;

        if (Time.time - ultimoTiempoDano < tiempoInmunidad) return;

        vidaActual -= cantidad;
        ultimoTiempoDano = Time.time;

        // Avisamos a la UI mandando el porcentaje (ej. 0.8f para 80%)
        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            OnMuerte?.Invoke();
        }
    }
    // ¡NUEVO MÉTODO! Para que funcionen los botiquines
    public void Curar(float cantidad)
    {
        if (vidaActual <= 0) return; // Los muertos no se curan

        vidaActual += cantidad;

        // Evitamos que la vida pase del límite máximo
        if (vidaActual > vidaMaxima) vidaActual = vidaMaxima;

        // Gritamos a la UI que la vida subió
        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
    }
}