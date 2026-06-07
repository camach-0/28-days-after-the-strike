using UnityEngine;
using System;
using System.Collections;

public class SistemaSalud : MonoBehaviour, IReceptorDano
{
    [Header("Identificación")]
    [Tooltip("Marca esta casilla TRUE solo en tus prefabs de personajes (Jugador). En los zombis déjala en FALSE.")]
    public bool esSuperviviente = false;

    [Header("Estadísticas de Vida Base")]
    public float vidaMaxima = 100f;
    public float vidaActual { get; private set; }

    [Header("Incapacidad (Solo Supervivientes)")]
    public float vidaIncapacidadMax = 300f; // L4D: 300 puntos de vida en el suelo
    public float desangradoPorSegundo = 2f;  // Cuánta vida pierde solo por segundo
    public float vidaActualIncapacitado { get; private set; }
    public bool estaIncapacitado { get; private set; } = false;
    private bool estaSiendoEmpujado = false;

    [Header("I-Frames (Inmunidad)")]
    public float tiempoInmunidad = 0.2f;
    private float ultimoTiempoDano = -100f;

    private Rigidbody2D rb;

    // Propiedad rápida para saber si está MUERTO DE VERDAD
    public bool estaMuertoDefinitivo => vidaActual <= 0 && !estaIncapacitado;

    // PATRÓN OBSERVER: Eventos globales
    public event Action OnMuerte;
    public event Action OnIncapacitado;
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

    private void OnEnable()
    {
        // Limpiamos los estados por si el objeto acaba de salir de la piscina
        estaIncapacitado = false;
    }

    private void OnDisable()
    {
        estaSiendoEmpujado = false;
        // ¡NUEVO Y VITAL! Si se apaga (muere el zombi), cortamos todas las corrutinas de empuje
        // para que no nazca con fuerzas fantasma en su próxima vida.
        StopAllCoroutines();
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
        // Debug más claro para que no nos confunda
        Debug.Log($"[SISTEMA SALUD] {gameObject.name} recibe daño. ¿Es Superviviente?: {esSuperviviente} | Vida: {vidaActual}");

        if (estaMuertoDefinitivo) return;

        if (Time.time - ultimoTiempoDano < tiempoInmunidad) return;
        ultimoTiempoDano = Time.time;

        // --- CASO 1: YA ESTÁ INCAPACITADO (Solo jugadores en el piso) ---
        if (estaIncapacitado)
        {
            vidaActualIncapacitado -= cantidad;
            OnVidaCambiada?.Invoke(vidaActualIncapacitado / vidaIncapacidadMax);

            if (vidaActualIncapacitado <= 0)
            {
                vidaActualIncapacitado = 0;
                estaIncapacitado = false;
                OnMuerte?.Invoke(); // Muerte definitiva del jugador
            }
            else
            {
                AplicarFuerzaEmpuje(direccion, fuerza);
            }
            return;
        }

        // --- CASO 2: DAÑO NORMAL (ESTÁ DE PIE) ---
        vidaActual -= cantidad;
        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);

        if (vidaActual <= 0)
        {
            vidaActual = 0;

            if (esSuperviviente)
            {
                AplicarFuerzaEmpuje(direccion, fuerza);
                EntrarEnIncapacitado();
            }
            else
            {
                // ¡Si es Zombi, muere instantáneamente! No lo empujamos, solo lo destruimos.
                StopAllCoroutines();
                OnMuerte?.Invoke();
            }
        }
        else
        {
            // Aún sigue vivo, le aplicamos el empuje (Knockback)
            AplicarFuerzaEmpuje(direccion, fuerza);
        }
    }

    private void AplicarFuerzaEmpuje(Vector2 direccion, float fuerza)
    {
        if (fuerza > 0 && rb != null && !estaSiendoEmpujado)
        {
            // Le bajamos el tiempo a 0.15f para que la motosierra cause un tropiezo rápido y constante
            StartCoroutine(RutinaCorteMovimiento(0.15f, direccion, fuerza));
        }
    }

    private IEnumerator RutinaCorteMovimiento(float tiempo, Vector2 direccion, float fuerza)
    {
        estaSiendoEmpujado = true; // CERRAMOS EL CANDADO
        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        ZombiController zomCtrl = GetComponent<ZombiController>();

        // 1. Apagamos el script de movimiento SOLO si es el jugador
        if (jugMov != null) jugMov.enabled = false;

        // 2. Si es Zombi, ¡NUNCA apagamos su script! (Para que no se cure ni pierda la memoria)
        // En su lugar, desconectamos temporalmente su "cerebro" del NavMesh
        if (zomCtrl != null && zomCtrl.agente != null)
        {
            zomCtrl.agente.updatePosition = false;
            zomCtrl.agente.velocity = Vector2.zero;
        }

        // 3. EL GOLPE FÍSICO
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = direccion * fuerza;

        // 4. Esperamos a que termine de volar por los aires
        yield return new WaitForSeconds(tiempo);

        // 5. Frenamos el derrape físico
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 6. Devolvemos el control
        estaSiendoEmpujado = false; // ABRIMOS EL CANDADO

        if (!estaIncapacitado && !estaMuertoDefinitivo)
        {
            if (jugMov != null) jugMov.enabled = true;

            if (zomCtrl != null && zomCtrl.agente != null)
            {
                // Teletransportamos el cerebro invisible a donde voló el cuerpo y los reconectamos
                zomCtrl.agente.Warp(transform.position);
                zomCtrl.agente.updatePosition = true;
            }
        }
    }

    private void EntrarEnIncapacitado()
    {
        estaIncapacitado = true;
        vidaActualIncapacitado = vidaIncapacidadMax;

        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = false;

        InventarioJugador inventario = GetComponent<InventarioJugador>();
        if (inventario != null)
        {
            // Saca la pistola obligatoriamente al caer
            inventario.CambiarSlot(1, true);
        }

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

        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = true;
    }

    public void Curar(float cantidad)
    {
        if (vidaActual <= 0 || estaIncapacitado) return;

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