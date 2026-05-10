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
        base.Start(); // Llama al Start de Entidad (para la vida y color)

        agente = GetComponent<NavMeshAgent>();
        agente.updateRotation = false;
        agente.updateUpAxis = false;

        // NUEVA LÓGICA DE HORDA:
        if (esDeHorda)
        {
            esEstatico = false; // Un zombi de horda jamás es estático
            BuscarJugadorCercano(); // Busca inmediatamente a su presa

            // Si encontró a alguien, arranca a correr
            if (objetivoJugador != null)
            {
                estadoActual = EstadoZombi.Persiguiendo;
                agente.isStopped = false;
                agente.speed = velocidadMovimiento;
            }
            else
            {
                estadoActual = EstadoZombi.Deambulando;
            }
        }
        else
        {
            estadoActual = EstadoZombi.Deambulando;
            tiempoEsperaPatrulla = Random.Range(0f, 2f);
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        switch (estadoActual)
        {
            case EstadoZombi.Deambulando:
                // Si es horda y está deambulando, significa que perdió a su presa o acaba de nacer.
                // Le pedimos que busque otra vez sin descanso.
                if (esDeHorda) BuscarJugadorCercano();

                ComportamientoDeambular();
                // Si no es horda, solo busca si entra en su visión normal
                if (!esDeHorda) BuscarJugadorCercano();
                break;

            case EstadoZombi.Persiguiendo:
                ComportamientoPerseguir();
                break;
        }
    }

    private void BuscarJugadorCercano()
    {
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        float distanciaCorta = Mathf.Infinity;
        GameObject jugadorMasCercano = null;

        foreach (GameObject jugador in jugadores)
        {
            Entidad vidaJugador = jugador.GetComponent<Entidad>();

            // Solo buscamos jugadores que sigan vivos
            if (vidaJugador != null && !vidaJugador.estaMuerto)
            {
                float distancia = Vector2.Distance(transform.position, jugador.transform.position);

                // LA CLAVE: Si es de horda, no importa la distancia (olfato infinito).
                // Si es ambiental, solo lo ve si está dentro del radio de visión.
                if (esDeHorda || distancia <= radioVision)
                {
                    if (distancia < distanciaCorta)
                    {
                        distanciaCorta = distancia;
                        jugadorMasCercano = jugador;
                    }
                }
            }
        }

        if (jugadorMasCercano != null)
        {
            objetivoJugador = jugadorMasCercano.transform;
            estadoActual = EstadoZombi.Persiguiendo;
        }
        else
        {
            // Si no hay nadie vivo a quien perseguir, deambula por las calles
            estadoActual = EstadoZombi.Deambulando;
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

        if (!agente.pathPending && (agente.remainingDistance <= agente.stoppingDistance + 0.1f || !agente.hasPath))
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

        Entidad vidaObjetivo = objetivoJugador.GetComponent<Entidad>();

        // REVISAMOS SI LA PRESA ACABA DE MORIR
        if (vidaObjetivo != null && vidaObjetivo.estaMuerto)
        {
            objetivoJugador = null;
            estadoActual = EstadoZombi.Deambulando;

            // EL ARREGLO DEFINITIVO:
            agente.ResetPath(); // Borramos la ruta vieja hacia el cadáver
            agente.isStopped = false;
            tiempoEsperaPatrulla = 0f;

            return;
        }

        // Medimos la distancia exacta
        float distanciaAlJugador = Vector2.Distance(transform.position, objetivoJugador.position);

        // Si está en rango de ataque (1.2m), APAGAMOS EL MOTOR
        if (distanciaAlJugador <= 1.2f)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;

            // Lógica de daño
            if (Time.time >= tiempoSiguienteAtaque)
            {
                if (vidaObjetivo != null && !vidaObjetivo.estaMuerto)
                {
                    vidaObjetivo.RecibirDano(danoAlJugador);
                    tiempoSiguienteAtaque = Time.time + velocidadAtaque;
                }
            }
        }
        else // Si se aleja, encendemos el motor
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