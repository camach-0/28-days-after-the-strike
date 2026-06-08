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

    // --- NUEVO: VIDA TEMPORAL ---
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

    public bool estaMuertoDefinitivo => vidaActual <= 0 && !estaIncapacitado;

    // EVENTOS
    public event Action OnMuerte;
    public event Action OnIncapacitado;
    public event Action<float> OnVidaCambiada;
    public event Action<float> OnVidaTemporalCambiada; // NUEVO EVENTO PARA LA UI

    private Coroutine rutinaDecaimiento; // Para controlar la pérdida de vida temporal

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

        // --- NUEVA LÓGICA DE DAÑO: PRIMERO ABSORBE LA VIDA TEMPORAL ---
        if (vidaTemporal > 0)
        {
            if (cantidad <= vidaTemporal)
            {
                vidaTemporal -= cantidad;
                cantidad = 0; // El daño fue absorbido por completo
            }
            else
            {
                cantidad -= vidaTemporal; // Sobró daño
                vidaTemporal = 0;
            }
            OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);
        }

        // Si todavía quedó daño (o no había vida temporal), restamos de la vida real
        if (cantidad > 0)
        {
            vidaActual -= cantidad;
            OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
        }

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            vidaTemporal = 0; // Por si acaso
            OnVidaTemporalCambiada?.Invoke(0f);

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
            // Solo empujamos si sigue vivo y NO tiene Adrenalina activa
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

    // --- NUEVO: AGREGAR VIDA TEMPORAL (Píldoras/Adrenalina) ---
    public void AñadirVidaTemporal(float cantidad)
    {
        if (vidaActual <= 0 || estaIncapacitado) return;

        // No podemos pasar de 100 en total (Vida Real + Vida Temporal)
        float espacioDisponible = vidaMaxima - vidaActual;

        vidaTemporal += cantidad;
        if (vidaTemporal > espacioDisponible) vidaTemporal = espacioDisponible;

        OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);

        // Reiniciamos el decaimiento para que empiece a bajar
        if (rutinaDecaimiento != null) StopCoroutine(rutinaDecaimiento);
        rutinaDecaimiento = StartCoroutine(RutinaDecaimientoTemporal());
    }

    private IEnumerator RutinaDecaimientoTemporal()
    {
        // Pierde 1 punto cada 4 segundos mientras no esté en el suelo
        while (vidaTemporal > 0 && !estaIncapacitado && !estaMuertoDefinitivo)
        {
            yield return new WaitForSeconds(4f);
            vidaTemporal -= 1f;
            if (vidaTemporal < 0) vidaTemporal = 0;

            OnVidaTemporalCambiada?.Invoke(vidaTemporal / vidaMaxima);
        }
    }

    private void EntrarEnIncapacitado()
    {
        estaIncapacitado = true;
        vidaActualIncapacitado = vidaIncapacidadMax;

        // Al caer borramos la vida temporal sobrante
        vidaTemporal = 0;
        OnVidaTemporalCambiada?.Invoke(0f);

        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = false;

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

    public void LevantarRescatado(float vidaAlLevantar)
    {
        if (!estaIncapacitado) return;

        estaIncapacitado = false;
        StopCoroutine(RutinaDesangrado());

        vidaActual = 0; // En L4D te levantan con vida temporal, pero respetaremos tu lógica actual
        AñadirVidaTemporal(vidaAlLevantar); // ¡Se levanta con salud temporal!

        JugadorMovimiento jugMov = GetComponent<JugadorMovimiento>();
        if (jugMov != null) jugMov.enabled = true;
    }

    public void Curar(float cantidad)
    {
        if (vidaActual <= 0 || estaIncapacitado) return;

        vidaActual += cantidad;
        if (vidaActual > vidaMaxima) vidaActual = vidaMaxima;

        // Si nos curamos con botiquín, limpiamos la vida temporal porque ya es vida sólida
        vidaTemporal = 0;
        OnVidaTemporalCambiada?.Invoke(0f);
        OnVidaCambiada?.Invoke(vidaActual / vidaMaxima);
    }

    public void Revivir()
    {
        estaIncapacitado = false;
        vidaActual = vidaMaxima;
        vidaTemporal = 0;
        OnVidaTemporalCambiada?.Invoke(0f);
        OnVidaCambiada?.Invoke(1f);
    }
}