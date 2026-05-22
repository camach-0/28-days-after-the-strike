using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(SistemaSalud))]
public class ZombiController : MonoBehaviour
{
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

    private void Start()
    {
        moduloSalud.OnMuerte += Morir;

        if (esDeHorda) esEstatico = false;

        CambiarEstado(estadoDeambular);
    }

    private void Update()
    {
        if (moduloSalud.vidaActual <= 0) return;

        estadoActual?.Actualizar(this);
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

        // ¡LA CLAVE DE LAS HORDAS! Escanea la lista central de supervivientes vivos (tanto bots como humanos)
        foreach (SistemaSalud superviviente in GameManager.Instancia.supervivientesActivos)
        {
            if (superviviente != null && superviviente.vidaActual > 0)
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

    private void Morir()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioVision);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioPatrullaje);
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
        if (zombi.objetivoJugador == null || zombi.objetivoJugador.GetComponent<SistemaSalud>().vidaActual <= 0)
        {
            zombi.objetivoJugador = null;
            zombi.CambiarEstado(zombi.estadoDeambular);
            zombi.agente.ResetPath();
            return;
        }

        float distanciaAlJugador = Vector2.Distance(zombi.transform.position, zombi.objetivoJugador.position);

        if (distanciaAlJugador <= 1.2f)
        {
            zombi.agente.isStopped = true;
            zombi.agente.velocity = Vector3.zero;

            if (Time.time >= zombi.tiempoSiguienteAtaque)
            {
                zombi.objetivoJugador.GetComponent<SistemaSalud>().RecibirDano(zombi.danoAlJugador, Vector2.zero, 0f);
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