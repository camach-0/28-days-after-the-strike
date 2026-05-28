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

    // NUEVO: Variables para el Algoritmo Boid (Efecto L4D)
    [Header("Comportamiento de Horda (Boids)")]
    [Tooltip("Distancia a la que empieza a empujar a otros zombis")]
    public float radioSeparacion = 0.8f;
    [Tooltip("Qué tan fuerte se empujan entre ellos")]
    public float fuerzaSeparacion = 3f;
    [Tooltip("La capa de Unity donde están los zombis")]
    public LayerMask capaZombis;

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

        // NUEVO: Aplicamos la separación de horda en cada frame
        AplicarSeparacionBoids();
    }

    // NUEVO: La magia matemática de la Separación
    private void AplicarSeparacionBoids()
    {
        // Lanzamos un radar circular para encontrar a otros zombis
        Collider2D[] vecinos = Physics2D.OverlapCircleAll(transform.position, radioSeparacion, capaZombis);
        Vector2 fuerzaRepulsion = Vector2.zero;
        int conteo = 0;

        foreach (Collider2D vecino in vecinos)
        {
            // Evitamos que el zombi se detecte y se empuje a sí mismo
            if (vecino.gameObject != this.gameObject)
            {
                // Calculamos la dirección opuesta al vecino
                Vector2 direccionAlejamiento = (Vector2)transform.position - (Vector2)vecino.transform.position;
                float distancia = direccionAlejamiento.magnitude;

                // Cuanto más cerca está el compañero, más fuerte es el empujón
                if (distancia > 0)
                {
                    fuerzaRepulsion += (direccionAlejamiento.normalized / distancia);
                    conteo++;
                }
            }
        }

        // Si hay zombis cerca, calculamos el empujón final y lo aplicamos
        if (conteo > 0)
        {
            fuerzaRepulsion /= conteo;
            // Deslizamos al zombi suavemente sin romper su persecución del NavMesh
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

        // NUEVO: Dibujamos la burbuja de repulsión en color azul para que la puedas ajustar visualmente
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioSeparacion);
    }
}

// ==========================================
// PATRÓN STATE: LAS CLASES DE COMPORTAMIENTO
// ==========================================
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