using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(SistemaSalud))]
public class ZombiController : MonoBehaviour
{
    [Header("Configuración del Pool")]
    public string etiquetaPool = "ZombiBase";

    [Header("Memoria Global")]
    public static int zombisActivosEnMapa = 0;

    [Header("Cerebro de IA")]
    public bool esDeHorda = false;
    public bool esEstatico = false;
    public float velocidadMovimiento = 4f;
    public float multiplicadorVelocidadPatrulla = 0.3f;

    [Header("Sensores")]
    public float radioVision = 5f;
    public float radioPatrullaje = 8f;

    [Header("Ataque")]
    public float danoAlJugador = 10f;
    public float velocidadAtaque = 1.0f;

    [Header("Comportamiento de Horda (Boids)")]
    public float radioSeparacion = 0.8f;
    public float fuerzaSeparacion = 3f;
    public LayerMask capaZombis;

    [Header("Efectos de Sonido")]
    public AudioClip sonidoAtaque;
    public AudioClip sonidoIdle;
    [HideInInspector] public float tiempoSiguienteSonidoIdle = 0f;

    [HideInInspector] public NavMeshAgent agente;
    [HideInInspector] public SistemaSalud moduloSalud;
    [HideInInspector] public Transform objetivoJugador;
    [HideInInspector] public float tiempoSiguienteAtaque = 0f;
    [HideInInspector] public float tiempoEsperaPatrulla = 0f;

    private IEstadoZombi estadoActual;
    public EstadoDeambularZombi estadoDeambular = new EstadoDeambularZombi();
    public EstadoPerseguirZombi estadoPerseguir = new EstadoPerseguirZombi();

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        moduloSalud = GetComponent<SistemaSalud>();

        agente.updateRotation = false;
        agente.updateUpAxis = false;
    }

    private void OnEnable()
    {
        zombisActivosEnMapa++;
        if (moduloSalud != null)
        {
            moduloSalud.Revivir();
            moduloSalud.OnMuerte += Morir;
        }

        objetivoJugador = null;
        CambiarEstado(estadoDeambular);
    }

    private void OnDisable()
    {
        zombisActivosEnMapa--;
        if (moduloSalud != null) moduloSalud.OnMuerte -= Morir;

        if (agente != null && agente.isActiveAndEnabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }
    }

    private void Update()
    {
        if (moduloSalud.vidaActual <= 0) return;

        estadoActual?.Actualizar(this);
        AplicarSeparacionBoids();
    }

    private void AplicarSeparacionBoids()
    {
        Collider2D[] vecinos = Physics2D.OverlapCircleAll(transform.position, radioSeparacion, capaZombis);
        Vector2 fuerzaRepulsion = Vector2.zero;
        int conteo = 0;

        foreach (Collider2D vecino in vecinos)
        {
            if (vecino.gameObject != this.gameObject)
            {
                Vector2 direccionAlejamiento = (Vector2)transform.position - (Vector2)vecino.transform.position;
                float distancia = direccionAlejamiento.magnitude;

                if (distancia > 0)
                {
                    fuerzaRepulsion += (direccionAlejamiento.normalized / distancia);
                    conteo++;
                }
            }
        }

        if (conteo > 0)
        {
            fuerzaRepulsion /= conteo;
            transform.position += (Vector3)(fuerzaRepulsion * fuerzaSeparacion * Time.deltaTime);
        }
    }

    public void CambiarEstado(IEstadoZombi nuevoEstado)
    {
        estadoActual?.Salir(this);
        estadoActual = nuevoEstado;
        estadoActual?.Entrar(this);
    }

    public Transform EscanearJugador()
    {
        if (GameManager.Instancia == null) return null;

        float distanciaCorta = Mathf.Infinity;
        Transform jugadorMasCercano = null;

        foreach (SistemaSalud superviviente in GameManager.Instancia.supervivientesActivos)
        {
            if (superviviente != null && !superviviente.estaMuertoDefinitivo)
            {
                float distancia = Vector2.Distance(transform.position, superviviente.transform.position);

                if (esDeHorda || distancia <= radioVision)
                {
                    if (distancia < distanciaCorta)
                    {
                        distanciaCorta = distancia;
                        jugadorMasCercano = superviviente.transform;
                    }
                }
            }
        }
        return jugadorMasCercano;
    }
    public void Ralentizar(float porcentaje)
    {
        if (agente != null) agente.speed = velocidadMovimiento * porcentaje;
    }

    public void RestaurarVelocidad()
    {
        if (agente != null) agente.speed = velocidadMovimiento;
    }
    private void Morir()
    {
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioVision);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioPatrullaje);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioSeparacion);
    }
}

public interface IEstadoZombi
{
    void Entrar(ZombiController zombi);
    void Actualizar(ZombiController zombi);
    void Salir(ZombiController zombi);
}

public class EstadoDeambularZombi : IEstadoZombi
{
    public void Entrar(ZombiController zombi)
    {
        if (zombi.esEstatico) zombi.agente.isStopped = true;
        // Reiniciamos el temporizador para que no gruñan apenas nacen
        zombi.tiempoSiguienteSonidoIdle = Time.time + Random.Range(3f, 8f);
    }

    public void Actualizar(ZombiController zombi)
    {
        Transform presa = zombi.EscanearJugador();
        if (presa != null)
        {
            zombi.objetivoJugador = presa;
            zombi.CambiarEstado(zombi.estadoPerseguir);
            return;
        }

        // Lógica de Sonido Reposo: Gruñidos aleatorios mientras patrullan
        if (Time.time >= zombi.tiempoSiguienteSonidoIdle && zombi.sonidoIdle != null)
        {
            AudioSource.PlayClipAtPoint(zombi.sonidoIdle, zombi.transform.position, 0.3f);
            zombi.tiempoSiguienteSonidoIdle = Time.time + Random.Range(5f, 12f);
        }

        if (!zombi.esDeHorda && !zombi.esEstatico)
        {
            zombi.agente.isStopped = false;
            zombi.agente.speed = zombi.velocidadMovimiento * zombi.multiplicadorVelocidadPatrulla;

            if (!zombi.agente.pathPending && (zombi.agente.remainingDistance <= zombi.agente.stoppingDistance + 0.1f || !zombi.agente.hasPath))
            {
                zombi.tiempoEsperaPatrulla -= Time.deltaTime;
                if (zombi.tiempoEsperaPatrulla <= 0f)
                {
                    Vector2 puntoAleatorio = (Vector2)zombi.transform.position + Random.insideUnitCircle * zombi.radioPatrullaje;
                    if (NavMesh.SamplePosition(puntoAleatorio, out NavMeshHit hit, zombi.radioPatrullaje, NavMesh.AllAreas))
                    {
                        zombi.agente.SetDestination(hit.position);
                        zombi.tiempoEsperaPatrulla = Random.Range(2f, 5f);
                    }
                }
            }
        }
    }

    public void Salir(ZombiController zombi) { }
}

public class EstadoPerseguirZombi : IEstadoZombi
{
    public void Entrar(ZombiController zombi)
    {
        zombi.agente.isStopped = false;
        zombi.agente.speed = zombi.velocidadMovimiento;
    }

    public void Actualizar(ZombiController zombi)
    {
        if (zombi.objetivoJugador == null || !zombi.objetivoJugador.gameObject.activeInHierarchy)
        {
            zombi.objetivoJugador = null;
            zombi.CambiarEstado(zombi.estadoDeambular);
            if (zombi.agente.isOnNavMesh) zombi.agente.ResetPath();
            return;
        }

        SistemaSalud saludObjetivo = zombi.objetivoJugador.GetComponent<SistemaSalud>();

        if (saludObjetivo != null && saludObjetivo.estaMuertoDefinitivo)
        {
            zombi.objetivoJugador = null;
            zombi.CambiarEstado(zombi.estadoDeambular);
            if (zombi.agente.isOnNavMesh) zombi.agente.ResetPath();
            return;
        }

        float distanciaAlJugador = Vector2.Distance(zombi.transform.position, zombi.objetivoJugador.position);

        if (distanciaAlJugador <= 1.2f)
        {
            zombi.agente.isStopped = true;
            zombi.agente.velocity = Vector3.zero;

            if (Time.time >= zombi.tiempoSiguienteAtaque)
            {
                if (saludObjetivo != null)
                {
                    saludObjetivo.RecibirDano(zombi.danoAlJugador, Vector2.zero, 0f);

                    // Lógica de Sonido Ataque: Suena justo al golpear al jugador
                    if (zombi.sonidoAtaque != null)
                    {
                        AudioSource.PlayClipAtPoint(zombi.sonidoAtaque, zombi.transform.position, 0.6f);
                    }
                }

                zombi.tiempoSiguienteAtaque = Time.time + zombi.velocidadAtaque;
            }
        }
        else
        {
            zombi.agente.isStopped = false;
            zombi.agente.SetDestination(zombi.objetivoJugador.position);
        }
    }

    public void Salir(ZombiController zombi) { }
}