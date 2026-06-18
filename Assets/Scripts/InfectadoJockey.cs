using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SistemaSalud), typeof(ZombiController), typeof(Rigidbody2D))]
public class InfectadoJockey : MonoBehaviour
{
    [Header("Configuración de Salto (Estilo Hunter)")]
    public float distanciaMinimaSalto = 2f;
    public float distanciaMaximaSalto = 5f;
    public float tiempoPreparacion = 0.35f;
    public float fuerzaSalto = 12f;
    public float cooldownSalto = 4f;
    public float distanciaFinalizacionSalto = 0.6f;

    [Header("Configuración de Montura")]
    public float danoMontura = 3f;
    public float intervaloDano = 0.7f;
    public float velocidadArrastre = 3.5f;

    public LayerMask capaJugador;
    public LayerMask capaZombis;

    [HideInInspector] public ZombiController miCerebro;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D miCollider;

    [HideInInspector] public bool enModoEspecial = false;
    [HideInInspector] public bool estaSaltando = false;
    [HideInInspector] public Transform jugadorMontado;
    [HideInInspector] public float ultimoSalto = -10f;
    [HideInInspector] public Vector2 posicionBloqueada;

    public EstadoSaltarJockey estadoSaltar = new EstadoSaltarJockey();
    public EstadoMontadoJockey estadoMontado = new EstadoMontadoJockey();

    private void Awake()
    {
        miCerebro = GetComponent<ZombiController>();
        rb = GetComponent<Rigidbody2D>();
        miCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (enModoEspecial) return;
        if (miCerebro.objetivoJugador == null) return;
        if (!miCerebro.agente.isActiveAndEnabled || !miCerebro.agente.isOnNavMesh) return;

        float distancia = Vector2.Distance(transform.position, miCerebro.objetivoJugador.position);

        bool rangoCorrecto = distancia >= distanciaMinimaSalto && distancia <= distanciaMaximaSalto;
        bool cooldownListo = Time.time >= ultimoSalto + cooldownSalto;

        if (rangoCorrecto && cooldownListo)
        {
            posicionBloqueada = miCerebro.objetivoJugador.position;
            miCerebro.CambiarEstado(estadoSaltar);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!enModoEspecial || !estaSaltando || jugadorMontado != null) return;

        if (((1 << collision.gameObject.layer) & capaJugador.value) != 0)
        {
            jugadorMontado = collision.transform;
            miCerebro.CambiarEstado(estadoMontado);
        }
    }

    public Transform ObtenerZombiCercano()
    {
        Collider2D[] colisionadores = Physics2D.OverlapCircleAll(transform.position, 20f, capaZombis);
        float distanciaCorta = Mathf.Infinity;
        Transform zombiMasCercano = null;

        foreach (var col in colisionadores)
        {
            if (col.gameObject != gameObject)
            {
                float distancia = Vector2.Distance(transform.position, col.transform.position);
                if (distancia < distanciaCorta)
                {
                    distanciaCorta = distancia;
                    zombiMasCercano = col.transform;
                }
            }
        }
        return zombiMasCercano;
    }
}

// ========================================================================
// ESTADO SALTAR
// ========================================================================
public class EstadoSaltarJockey : IEstadoZombi
{
    private float tiempoInicio;
    private bool saltoRealizado;

    public void Entrar(ZombiController zombi)
    {
        InfectadoJockey jockey = zombi.GetComponent<InfectadoJockey>();
        jockey.enModoEspecial = true;
        jockey.estaSaltando = true;

        tiempoInicio = Time.time;
        saltoRealizado = false;

        zombi.agente.isStopped = true;
        zombi.agente.velocity = Vector2.zero;
        jockey.rb.linearVelocity = Vector2.zero;
    }

    public void Actualizar(ZombiController zombi)
    {
        InfectadoJockey jockey = zombi.GetComponent<InfectadoJockey>();

        if (!saltoRealizado)
        {
            if (Time.time >= tiempoInicio + jockey.tiempoPreparacion)
            {
                saltoRealizado = true;
                zombi.agente.enabled = false;

                Vector2 direccion = (jockey.posicionBloqueada - (Vector2)zombi.transform.position).normalized;
                jockey.rb.linearVelocity = direccion * jockey.fuerzaSalto;

                jockey.ultimoSalto = Time.time;
            }
            return;
        }

        float distanciaObjetivo = Vector2.Distance(zombi.transform.position, jockey.posicionBloqueada);
        if (distanciaObjetivo <= jockey.distanciaFinalizacionSalto)
        {
            TerminarSalto(zombi, jockey);
        }
    }

    public void Salir(ZombiController zombi) { }

    private void TerminarSalto(ZombiController zombi, InfectadoJockey jockey)
    {
        jockey.enModoEspecial = false;
        jockey.estaSaltando = false;

        jockey.rb.linearVelocity = Vector2.zero;
        jockey.rb.angularVelocity = 0f;

        if (!zombi.agente.enabled) zombi.agente.enabled = true;
        zombi.agente.isStopped = false;

        zombi.CambiarEstado(zombi.estadoDeambular);
    }
}

// ========================================================================
// ESTADO MONTADO
// ========================================================================
public class EstadoMontadoJockey : IEstadoZombi
{
    private float siguienteTickDano;

    public void Entrar(ZombiController zombi)
    {
        InfectadoJockey jockey = zombi.GetComponent<InfectadoJockey>();
        siguienteTickDano = Time.time;
        jockey.enModoEspecial = true;
        jockey.estaSaltando = false;
        jockey.rb.linearVelocity = Vector2.zero;

        Collider2D colliderJugador = jockey.jugadorMontado.GetComponent<Collider2D>();
        if (colliderJugador != null && jockey.miCollider != null)
        {
            Physics2D.IgnoreCollision(jockey.miCollider, colliderJugador, true);
        }

        zombi.transform.SetParent(jockey.jugadorMontado);
        zombi.transform.localPosition = new Vector3(0f, 0.25f, 0f);

        StunController stun = jockey.jugadorMontado.GetComponent<StunController>();
        if (stun != null)
        {
            Transform zombiCercano = jockey.ObtenerZombiCercano();
            stun.AplicarStunJockey(zombiCercano);
        }

        zombi.agente.enabled = false;
    }

    public void Actualizar(ZombiController zombi)
    {
        InfectadoJockey jockey = zombi.GetComponent<InfectadoJockey>();

        if (jockey.jugadorMontado == null)
        {
            TerminarMontura(zombi, jockey);
            return;
        }

        SistemaSalud saludJugador = jockey.jugadorMontado.GetComponent<SistemaSalud>();
        if (saludJugador != null && (saludJugador.estaMuertoDefinitivo || saludJugador.vidaActual <= 0))
        {
            TerminarMontura(zombi, jockey);
            return;
        }

        if (Time.time >= siguienteTickDano && saludJugador != null)
        {
            saludJugador.RecibirDano(jockey.danoMontura, Vector2.zero, 0f);
            siguienteTickDano = Time.time + jockey.intervaloDano;
        }
    }

    public void Salir(ZombiController zombi) { }

    private void TerminarMontura(ZombiController zombi, InfectadoJockey jockey)
    {
        if (jockey.jugadorMontado != null)
        {
            // Avisar al jugador liberado
            StunController stun = jockey.jugadorMontado.GetComponent<StunController>();
            if (stun != null)
            {
                stun.LiberarStunJockey();
            }

            zombi.transform.SetParent(null);

            jockey.jugadorMontado = null;
        }

        jockey.enModoEspecial = false;
        jockey.estaSaltando = false;

        if (!zombi.agente.enabled) zombi.agente.enabled = true;
        zombi.agente.isStopped = false;
    }
}


