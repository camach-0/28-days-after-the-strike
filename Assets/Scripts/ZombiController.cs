using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombiController : Entidad
{
    public enum EstadoZombi { Deambulando, Persiguiendo }

    [Header("Cerebro de IA")]
    public EstadoZombi estadoActual = EstadoZombi.Deambulando;
    public bool esDeHorda = false;

    [Header("Variantes de Zombi")]
    [Tooltip("Si se marca, el zombi se quedará quieto hasta ver al jugador.")]
    public bool esEstatico = false;
    public float multiplicadorVelocidadPatrulla = 0.3f; // 0.3 es el 30% de su velocidad normal

    [Header("Sensores")]
    public float radioVision = 5f;
    private Transform objetivoJugador;

    [Header("Patrullaje")]
    public float radioPatrullaje = 8f;
    private float tiempoEsperaPatrulla;
    [Header("Ataque")]
    public float danoAlJugador = 10f;
    public float velocidadAtaque = 1.0f; // Tiempo en segundos entre golpes
    private float tiempoSiguienteAtaque = 0f;

    private NavMeshAgent agente;

    public override void Start()
    {
        base.Start();
        agente = GetComponent<NavMeshAgent>();

        agente.updateRotation = false;
        agente.updateUpAxis = false;

        if (esDeHorda)
        {
            estadoActual = EstadoZombi.Persiguiendo;
        }
        else
        {
            tiempoEsperaPatrulla = Random.Range(1f, 3f);
        }
    }

    private void Update()
    {
        if (estaMuerto)
        {
            agente.isStopped = true;
            return;
        }

        switch (estadoActual)
        {
            case EstadoZombi.Deambulando:
                ComportamientoDeambular();
                BuscarJugadorCercano();
                break;

            case EstadoZombi.Persiguiendo:
                ComportamientoPerseguir();
                break;
        }
    }

    private void BuscarJugadorCercano()
    {
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject jugador in jugadores)
        {
            float distancia = Vector2.Distance(transform.position, jugador.transform.position);
            if (distancia <= radioVision)
            {
                objetivoJugador = jugador.transform;
                estadoActual = EstadoZombi.Persiguiendo;
                break;
            }
        }
    }

    private void ComportamientoDeambular()
    {
        // 1. Si marcamos a este zombi como ESTÁTICO en Unity, cancelamos su patrullaje aquí mismo
        if (esEstatico)
        {
            agente.isStopped = true; // Se queda congelado en su sitio
            return;
        }

        // 2. Si no es estático, le aplicamos la velocidad lenta y lo dejamos caminar
        agente.isStopped = false;
        agente.speed = velocidadMovimiento * multiplicadorVelocidadPatrulla;

        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            tiempoEsperaPatrulla -= Time.deltaTime;

            if (tiempoEsperaPatrulla <= 0f)
            {
                Vector2 puntoAleatorio = (Vector2)transform.position + Random.insideUnitCircle * radioPatrullaje;
                NavMeshHit hit;

                if (NavMesh.SamplePosition(puntoAleatorio, out hit, radioPatrullaje, NavMesh.AllAreas))
                {
                    agente.SetDestination(hit.position);
                    tiempoEsperaPatrulla = Random.Range(2f, 5f);
                }
            }
        }
    }

    private void ComportamientoPerseguir()
    {
        if (objetivoJugador == null)
        {
            estadoActual = EstadoZombi.Deambulando;
            return;
        }

        // Medimos la distancia exacta
        float distanciaAlJugador = Vector2.Distance(transform.position, objetivoJugador.position);

        // 1. EL TRUCO: Si está en rango de ataque (1.2m), APAGAMOS EL MOTOR A LA FUERZA
        if (distanciaAlJugador <= 1.2f)
        {
            agente.isStopped = true; // Corta el cálculo de rutas
            agente.velocity = Vector3.zero; // Elimina cualquier inercia o micro-ajuste

            // Lógica de daño
            if (Time.time >= tiempoSiguienteAtaque)
            {
                Entidad vidaJugador = objetivoJugador.GetComponent<Entidad>();

                if (vidaJugador != null && !vidaJugador.estaMuerto)
                {
                    vidaJugador.RecibirDano(danoAlJugador);
                    tiempoSiguienteAtaque = Time.time + velocidadAtaque;
                    Debug.Log("¡El zombi atacó por cercanía! Siguiente ataque en: " + velocidadAtaque + "s");
                }
            }
        }
        // 2. Si el jugador se aleja, ENCENDEMOS EL MOTOR de nuevo
        else
        {
            agente.isStopped = false;
            agente.speed = velocidadMovimiento;
            agente.SetDestination(objetivoJugador.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioVision);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioPatrullaje);
    }
    // El cambio está aquí adentro de los paréntesis: (Collision2D collision)
    
}