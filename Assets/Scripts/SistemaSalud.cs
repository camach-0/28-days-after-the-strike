using UnityEngine;
using System;
using System.Collections;

public class SistemaSalud : MonoBehaviour, IReceptorDano
{
    [Header("Identificación")]
    [Tooltip("¡IMPORTANTE! Marca esta casilla TRUE solo en tus prefabs de personajes (Cholo, Camba, etc.). En los zombis déjala en FALSE.")]
    public bool esSuperviviente = false;

    [Header("Estadísticas de Vida Base")]
    public float vidaMaxima = 100f;
    public float vidaActual { get; private set; }

    [Header("Incapacidad (Solo Supervivientes)")]
    public float vidaIncapacidadMax = 300f; // L4D: 300 puntos de vida en el suelo
    public float desangradoPorSegundo = 2f;  // Cuánta vida pierde solo por segundo
    public float vidaActualIncapacitado { get; private set; }
    public bool estaIncapacitado { get; private set; } = false;

    [Header("I-Frames (Inmunidad)")]
    public float tiempoInmunidad = 0.2f;
    private float ultimoTiempoDano = -100f;

    private Rigidbody2D rb;

    // Propiedad rápida para saber si está MUERTO DE VERDAD
    public bool estaMuertoDefinitivo => vidaActual <= 0 && !estaIncapacitado;

    // PATRÓN OBSERVER: Eventos globales
    public event Action OnMuerte;
    public event Action OnIncapacitado; // ¡NUEVO! Avisa al juego que caíste al suelo
    public event Action<float> OnVidaCambiada;

    private void Awake()
    {
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
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
        // Si ya está muerto de verdad, ignoramos
        if (!esSuperviviente && vidaActual <= 0) return;

        if (Time.time - ultimoTiempoDano < tiempoInmunidad) return;
        ultimoTiempoDano = Time.time;

        // --- CASO 1: YA ESTÁ INCAPACITADO ---
        if (estaIncapacitado)
        {
            vidaActualIncapacitado -= cantidad;

            // Actualizamos la UI con la barra de desangrado
            OnVidaCambiada?.Invoke(vidaActualIncapacitado / vidaIncapacidadMax);

            if (vidaActualIncapacitado <= 0)
            {
                vidaActualIncapacitado = 0;
                estaIncapacitado = false;
                OnMuerte?.Invoke(); // Muerte definitiva
            }

            AplicarFuerzaEmpuje(direccion, fuerza);
            return;
        }

        // --- CASO 2: DAÑO NORMAL (ESTÁ DE PIE) ---
        vidaActual -= cantidad;
        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);

        AplicarFuerzaEmpuje(direccion, fuerza);

        if (vidaActual <= 0)
        {
            vidaActual = 0;

            if (esSuperviviente)
            {
                EntrarEnIncapacitado();
            }
            else
            {
                OnMuerte?.Invoke(); // Los zombis mueren directo
            }
        }
    }

    // ==========================================
    // --- NUEVO SISTEMA DE EMPUJE AGRESIVO ---
    // ==========================================
    private void AplicarFuerzaEmpuje(Vector2 direccion, float fuerza)
    {
        
        if (!esSuperviviente && vidaActual <= 0) return;

        if (fuerza > 0 && rb != null)
        {
            // Le pasamos la dirección y fuerza a la rutina para que ella aplique el golpe
            StartCoroutine(RutinaCorteMovimiento(0.2f, direccion, fuerza));
        }
    }

    private IEnumerator RutinaCorteMovimiento(float tiempo, Vector2 direccion, float fuerza)
    {
        // Buscamos si es jugador o zombi
        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        ZombiController zomCtrl = GetComponent<ZombiController>();

        // 1. Apagamos el dictador del movimiento
        if (jugMov != null) jugMov.enabled = false;
        if (zomCtrl != null) zomCtrl.enabled = false;

        // 2. EL GOLPE DEFINITIVO: Sobrescribimos la velocidad de la física directamente
        rb.linearVelocity = Vector2.zero; // Frenamos cualquier caminar previo
        rb.linearVelocity = direccion * fuerza; // ¡Saldrá volando a esta velocidad!

        // 3. Esperamos a que termine de volar
        yield return new WaitForSeconds(tiempo);

        // 4. Frenamos el derrape y le devolvemos el control
        rb.linearVelocity = Vector2.zero;

        if (!estaIncapacitado)
        {
            if (jugMov != null) jugMov.enabled = true;
            if (zomCtrl != null) zomCtrl.enabled = true;
        }
    }

    private void EntrarEnIncapacitado()
    {
        estaIncapacitado = true;
        vidaActualIncapacitado = vidaIncapacidadMax;

        // 1. Apagamos su script de movimiento (ya no puede caminar)
        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = false;

        // 2. Forzamos que saque la pistola (Slot 2 / Índice 1)
        InventarioJugador inventario = GetComponent<InventarioJugador>();
        if (inventario != null)
        {
            // Le pasamos 'true' para saltarnos el candado que acabamos de crear
            inventario.CambiarSlot(1, true);
        }

        // 3. Avisamos a los sistemas del juego y empezamos el desangrado
        OnIncapacitado?.Invoke();
        StartCoroutine(RutinaDesangrado());
    }

    private IEnumerator RutinaDesangrado()
    {
        while (estaIncapacitado && vidaActualIncapacitado > 0)
        {
            yield return new WaitForSeconds(1f);

            if (estaIncapacitado)
            {
                vidaActualIncapacitado -= desangradoPorSegundo;
                OnVidaCambiada?.Invoke(vidaActualIncapacitado / vidaIncapacidadMax);

                if (vidaActualIncapacitado <= 0)
                {
                    vidaActualIncapacitado = 0;
                    estaIncapacitado = false;
                    OnMuerte?.Invoke();
                }
            }
        }
    }

    public void LevantarRescatado(float vidaAlLevantar)
    {
        if (!estaIncapacitado) return;

        estaIncapacitado = false;
        StopCoroutine(RutinaDesangrado());

        vidaActual = vidaAlLevantar;
        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);

        // Le devolvemos la capacidad de caminar
        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = true;
    }

    public void Curar(float cantidad)
    {
        if (vidaActual <= 0 || estaIncapacitado) return; // Los incapacitados necesitan ser levantados primero

        vidaActual += cantidad;
        if (vidaActual > vidaMaxima) vidaActual = vidaMaxima;

        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
    }

    public void Revivir()
    {
        estaIncapacitado = false;
        vidaActual = vidaMaxima;
        OnVidaCambiada?.Invoke(1f);
    }
}