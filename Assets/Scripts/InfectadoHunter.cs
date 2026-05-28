using UnityEngine;

[RequireComponent(typeof(SistemaSalud))]
[RequireComponent(typeof(ZombiController))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class InfectadoHunter : MonoBehaviour
{
    [Header("Configuración de Salto")]
    public float distanciaMinimaSalto = 3f;
    public float distanciaMaximaSalto = 8f;
    [Tooltip("Distancia donde el Hunter fija la posición del jugador")]
    public float radioBloqueoSalto = 5f;
    [Tooltip("Distancia mínima para considerar terminado el salto")]
    public float distanciaFinalizacionSalto = 0.8f;
    public float tiempoPreparacion = 0.35f;
    public float fuerzaSalto = 18f;
    public float cooldownSalto = 5f;

    [Header("Configuración de Montura")]
    public float danoMontura = 8f;
    public float intervaloDano = 0.5f;
    public float duracionMontura = 4f;

    [Header("Detección de Impacto")]
    public LayerMask capaJugador;

    [HideInInspector] public ZombiController miCerebro;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D miCollider;
    [HideInInspector] public bool enModoEspecial = false;
    [HideInInspector] public bool estaSaltando = false;
    [HideInInspector] public float ultimoSalto = -10f;
    [HideInInspector] public Transform jugadorMontado;
    [HideInInspector] public Vector2 posicionBloqueada;

    // Estados
    public EstadoSaltarHunter estadoSaltar = new EstadoSaltarHunter();
    public EstadoMontadoHunter estadoMontado = new EstadoMontadoHunter();

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

        float distancia = Vector2.Distance(transform.position, miCerebro.objetivoJugador.position);

        bool rangoCorrecto = distancia >= distanciaMinimaSalto && distancia <= distanciaMaximaSalto;
        bool dentroDelBloqueo = distancia <= radioBloqueoSalto;
        bool cooldownListo = Time.time >= ultimoSalto + cooldownSalto;

        if (rangoCorrecto && dentroDelBloqueo && cooldownListo)
        {
            posicionBloqueada = miCerebro.objetivoJugador.position;
            miCerebro.CambiarEstado(estadoSaltar);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!enModoEspecial) return;
        if (!estaSaltando) return;

        if (((1 << collision.gameObject.layer) & capaJugador) != 0)
        {
            jugadorMontado = collision.transform;
            miCerebro.CambiarEstado(estadoMontado);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaMaximaSalto);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioBloqueoSalto);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(posicionBloqueada, 0.2f);
    }
}

// ========================================================================
// ESTADO SALTAR
// ========================================================================

public class EstadoSaltarHunter : IEstadoZombi
{
    private float tiempoInicio;
    private bool saltoRealizado;

    public void Entrar(ZombiController zombi)
    {
        InfectadoHunter hunter = zombi.GetComponent<InfectadoHunter>();

        hunter.enModoEspecial = true;
        hunter.estaSaltando = true;

        tiempoInicio = Time.time;
        saltoRealizado = false;

        zombi.agente.isStopped = true;
        zombi.agente.velocity = Vector2.zero;
        hunter.rb.linearVelocity = Vector2.zero;
    }

    public void Actualizar(ZombiController zombi)
    {
        InfectadoHunter hunter = zombi.GetComponent<InfectadoHunter>();

        if (!saltoRealizado)
        {
            if (Time.time >= tiempoInicio + hunter.tiempoPreparacion)
            {
                saltoRealizado = true;

                Vector2 direccion = (hunter.posicionBloqueada - (Vector2)zombi.transform.position).normalized;

                zombi.agente.enabled = false;

                hunter.rb.linearVelocity = direccion * hunter.fuerzaSalto;

                hunter.ultimoSalto = Time.time;
            }
            return;
        }

        float distanciaObjetivo = Vector2.Distance(zombi.transform.position, hunter.posicionBloqueada);

        if (distanciaObjetivo <= hunter.distanciaFinalizacionSalto)
        {
            TerminarSalto(zombi, hunter);
        }
    }

    public void Salir(ZombiController zombi) { }

    private void TerminarSalto(ZombiController zombi, InfectadoHunter hunter)
    {
        hunter.enModoEspecial = false;
        hunter.estaSaltando = false;
        hunter.rb.linearVelocity = Vector2.zero;
        hunter.rb.angularVelocity = 0f;

        if (!zombi.agente.enabled)
        {
            zombi.agente.enabled = true;
        }

        zombi.agente.isStopped = false;
        zombi.CambiarEstado(zombi.estadoDeambular);
    }
}

// ========================================================================
// ESTADO MONTADO
// ========================================================================

public class EstadoMontadoHunter : IEstadoZombi
{
    private float tiempoInicio;
    private float siguienteTickDano;
    private Collider2D colliderJugador;

    public void Entrar(ZombiController zombi)
    {
        InfectadoHunter hunter = zombi.GetComponent<InfectadoHunter>();

        tiempoInicio = Time.time;
        siguienteTickDano = Time.time;

        hunter.enModoEspecial = true;
        hunter.estaSaltando = false;

        hunter.rb.linearVelocity = Vector2.zero;

        zombi.agente.enabled = false;

        colliderJugador = hunter.jugadorMontado.GetComponent<Collider2D>();
        if (colliderJugador != null)
        {
            Physics2D.IgnoreCollision(hunter.miCollider, colliderJugador, true);
        }

        // Avisar al jugador atrapado
        StunController stun = hunter.jugadorMontado.GetComponent<StunController>();
        if (stun != null)
        {
            stun.AplicarStunHunter();
        }
    }

    public void Actualizar(ZombiController zombi)
    {
        InfectadoHunter hunter = zombi.GetComponent<InfectadoHunter>();

        if (hunter.jugadorMontado == null)
        {
            TerminarMontura(zombi, hunter);
            return;
        }

        Vector3 offset = new Vector3(0f, 0.5f, 0f);
        zombi.transform.position = hunter.jugadorMontado.position + offset;

        IReceptorDano receptor = hunter.jugadorMontado.GetComponent<IReceptorDano>();
        if (receptor != null && Time.time >= siguienteTickDano)
        {
            receptor.RecibirDano(hunter.danoMontura, Vector2.zero, 0f);
            siguienteTickDano = Time.time + hunter.intervaloDano;
        }

        if (Time.time >= tiempoInicio + hunter.duracionMontura)
        {
            TerminarMontura(zombi, hunter);
        }
    }

    public void Salir(ZombiController zombi) { }

    private void TerminarMontura(ZombiController zombi, InfectadoHunter hunter)
    {
        if (colliderJugador != null)
        {
            Physics2D.IgnoreCollision(hunter.miCollider, colliderJugador, false);

            // Avisar al jugador que ya fue liberado
            StunController stun = colliderJugador.GetComponent<StunController>();
            if (stun != null)
            {
                stun.LiberarStunHunter();
            }
        }

        hunter.enModoEspecial = false;
        hunter.jugadorMontado = null;

        zombi.agente.enabled = true;
        zombi.agente.isStopped = false;
        zombi.CambiarEstado(zombi.estadoDeambular);
    }
}


