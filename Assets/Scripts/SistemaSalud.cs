using UnityEngine;
using System;
using System.Collections;

public class SistemaSalud : MonoBehaviour, IReceptorDano
{
    [Header("Identificación")]
    public bool esSuperviviente = false;

    [Header("Estadísticas de Vida Base")]
    public float vidaMaxima = 100f;
    public float vidaActual { get; private set; }

    public float vidaTemporal { get; private set; } = 0f;

    [Header("Incapacidad (Solo Supervivientes)")]
    public float vidaIncapacidadMax = 300f;
    public float desangradoPorSegundo = 2f;
    public float vidaActualIncapacitado { get; private set; }
    public bool estaIncapacitado { get; private set; } = false;
    private bool estaSiendoEmpujado = false;

    [Header("I-Frames (Inmunidad)")]
    public float tiempoInmunidad = 0.2f;
    private float ultimoTiempoDano = -100f;

    private Rigidbody2D rb;

    // ¡CORRECCIÓN! Para estar 100% muerto no debes tener ni vida real ni temporal.
    public bool estaMuertoDefinitivo => vidaActual <= 0 && vidaTemporal <= 0 && !estaIncapacitado;

    // EVENTOS
    public event Action OnMuerte;
    public event Action OnIncapacitado;
    public event Action<float> OnVidaCambiada;
    public event Action<float> OnVidaTemporalCambiada;

    private Coroutine rutinaDecaimiento;

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
        estaIncapacitado = false;
    }

    private void OnDisable()
    {
        estaSiendoEmpujado = false;
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
        if (estaMuertoDefinitivo) return;
        if (Time.time - ultimoTiempoDano < tiempoInmunidad) return;
        ultimoTiempoDano = Time.time;

        if (estaIncapacitado)
        {
            vidaActualIncapacitado -= cantidad;
            OnVidaCambiada?.Invoke(vidaActualIncapacitado / vidaIncapacidadMax);

            if (vidaActualIncapacitado <= 0)
            {
                vidaActualIncapacitado = 0;
                estaIncapacitado = false;
                OnMuerte?.Invoke();
            }
            else
            {
                AplicarFuerzaEmpuje(direccion, fuerza);
            }
            return;
        }

        // LÓGICA DE DAÑO: PRIMERO ABSORBE LA VIDA TEMPORAL
        if (vidaTemporal > 0)
        {
            if (cantidad <= vidaTemporal)
            {
                vidaTemporal -= cantidad;
                cantidad = 0;
            }
            else
            {
                cantidad -= vidaTemporal;
                vidaTemporal = 0;
            }
            OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);
        }

        if (cantidad > 0)
        {
            vidaActual -= cantidad;
            if (vidaActual < 0) vidaActual = 0;
            OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
        }

        if (vidaActual <= 0 && vidaTemporal <= 0)
        {
            vidaActual = 0;
            vidaTemporal = 0;
            OnVidaTemporalCambiada?.Invoke(0f);
            OnVidaCambiada?.Invoke(0f);

            if (esSuperviviente)
            {
                AplicarFuerzaEmpuje(direccion, fuerza);
                EntrarEnIncapacitado();
            }
            else
            {
                StopAllCoroutines();
                OnMuerte?.Invoke();
            }
        }
        else
        {
            bool ignorarEmpuje = false;
            JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
            if (jugMov != null && jugMov.tieneAdrenalinaActiva) ignorarEmpuje = true;

            if (!ignorarEmpuje)
            {
                AplicarFuerzaEmpuje(direccion, fuerza);
            }
        }
    }

    private void AplicarFuerzaEmpuje(Vector2 direccion, float fuerza)
    {
        if (fuerza > 0 && rb != null && !estaSiendoEmpujado)
        {
            StartCoroutine(RutinaCorteMovimiento(0.15f, direccion, fuerza));
        }
    }

    private IEnumerator RutinaCorteMovimiento(float tiempo, Vector2 direccion, float fuerza)
    {
        estaSiendoEmpujado = true;
        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        ZombiController zomCtrl = GetComponent<ZombiController>();

        if (jugMov != null) jugMov.enabled = false;

        if (zomCtrl != null && zomCtrl.agente != null)
        {
            zomCtrl.agente.updatePosition = false;
            zomCtrl.agente.velocity = Vector2.zero;
        }

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = direccion * fuerza;

        yield return new WaitForSeconds(tiempo);

        if (rb != null) rb.linearVelocity = Vector2.zero;
        estaSiendoEmpujado = false;

        if (!estaIncapacitado && !estaMuertoDefinitivo)
        {
            if (jugMov != null) jugMov.enabled = true;

            if (zomCtrl != null && zomCtrl.agente != null)
            {
                zomCtrl.agente.Warp(transform.position);
                zomCtrl.agente.updatePosition = true;
            }
        }
    }

    // ====================================================================
    // --- SISTEMA DE CURACIÓN Y VIDA TEMPORAL ---
    // ====================================================================

    public void AñadirVidaTemporal(float cantidad)
    {
        // ¡CORRECCIÓN! Usamos estaMuertoDefinitivo para saber si realmente es un cadáver
        if (estaMuertoDefinitivo || estaIncapacitado) return;

        float espacioDisponible = vidaMaxima - vidaActual;

        vidaTemporal += cantidad;
        if (vidaTemporal > espacioDisponible) vidaTemporal = espacioDisponible;

        OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);

        if (rutinaDecaimiento != null) StopCoroutine(rutinaDecaimiento);
        rutinaDecaimiento = StartCoroutine(RutinaDecaimientoTemporal());
    }

    public void Curar(float cantidad)
    {
        // ¡CORRECCIÓN! Evitamos que rechace curar si tiene 1 de vida o vida temporal
        if (estaMuertoDefinitivo || estaIncapacitado) return;

        vidaActual += cantidad;
        if (vidaActual > vidaMaxima) vidaActual = vidaMaxima;

        // Al curar con botiquín, la vida temporal desaparece porque ya tienes vida real
        vidaTemporal = 0;

        OnVidaTemporalCambiada?.Invoke(0f);
        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
    }

    private IEnumerator RutinaDecaimientoTemporal()
    {
        while (vidaTemporal > 0 && !estaIncapacitado && !estaMuertoDefinitivo)
        {
            yield return new WaitForSeconds(4f);
            vidaTemporal -= 1f;
            if (vidaTemporal < 0) vidaTemporal = 0;

            OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);
        }
    }

    // ====================================================================
    // --- SISTEMA DE ESTADOS: INCAPACITADO / LEVANTAR / REVIVIR ---
    // ====================================================================

    private void EntrarEnIncapacitado()
    {
        estaIncapacitado = true;
        vidaActualIncapacitado = vidaIncapacidadMax;

        vidaTemporal = 0;
        OnVidaTemporalCambiada?.Invoke(0f);

        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = false;

        UnityEngine.AI.NavMeshAgent agente = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agente != null) agente.enabled = false;

        MonoBehaviour botCtrl = GetComponent("AliadoBotController") as MonoBehaviour;
        if (botCtrl != null) botCtrl.enabled = false;

        InventarioJugador inventario = GetComponent<InventarioJugador>();
        if (inventario != null)
        {
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

    private void ReactivarCuerpo()
    {
        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = true;

        MonoBehaviour inputHumano = GetComponent("JugadorInput") as MonoBehaviour;
        bool esHumano = (inputHumano != null && inputHumano.enabled);

        if (!esHumano)
        {
            UnityEngine.AI.NavMeshAgent agente = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agente != null)
            {
                agente.enabled = true;
                agente.isStopped = false;
            }

            MonoBehaviour botCtrl = GetComponent("AliadoBotController") as MonoBehaviour;
            if (botCtrl != null) botCtrl.enabled = true;
        }
    }

    // ESTADO 1: Levantar de estar abatido en el suelo
    public void LevantarRescatado(float vidaAlLevantar = 30f)
    {
        if (!estaIncapacitado) return;

        estaIncapacitado = false;
        StopCoroutine(RutinaDesangrado());

        // ¡EL ARREGLO CRÍTICO DEL FANTASMA!
        vidaActual = 1f;  // Te damos 1 HP real para que no te consideren muerto.
        vidaTemporal = vidaAlLevantar;

        ReactivarCuerpo();

        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
        OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);

        if (rutinaDecaimiento != null) StopCoroutine(rutinaDecaimiento);
        rutinaDecaimiento = StartCoroutine(RutinaDecaimientoTemporal());
    }

    // ESTADO 2: Revivir de la muerte absoluta
    public void Revivir()
    {
        estaIncapacitado = false;

        vidaActual = 50f;
        vidaTemporal = 0f;

        ReactivarCuerpo();

        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
        OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);
    }
}