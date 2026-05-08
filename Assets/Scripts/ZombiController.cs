using UnityEngine;
using UnityEngine.AI; // Necesario para el NavMesh

// Heredamos de Entidad para que tenga vida y pueda recibir daño de las balas
[RequireComponent(typeof(NavMeshAgent))]
public class ZombiController : Entidad
{
    // Definimos los posibles "estados mentales" del zombi
    public enum EstadoZombi { Deambulando, Persiguiendo }

    [Header("Cerebro de IA")]
    public EstadoZombi estadoActual = EstadoZombi.Deambulando;
    public bool esDeHorda = false; // El Director marcará esto como TRUE al crear hordas

    [Header("Sensores")]
    public float radioVision = 5f;
    private Transform objetivoJugador;

    // Herramienta de navegación
    private NavMeshAgent agente;

    public override void Start()
    {
        base.Start(); // Inicializa la vida de Entidad.cs

        agente = GetComponent<NavMeshAgent>();

        // Configuraciones vitales para que el NavMesh 3D funcione en nuestro juego 2D
        agente.updateRotation = false;
        agente.updateUpAxis = false;

        // Si el Director genera este zombi como Horda, pasa a persecución de inmediato
        if (esDeHorda)
        {
            estadoActual = EstadoZombi.Persiguiendo;
        }
    }

    private void Update()
    {
        if (estaMuerto)
        {
            agente.isStopped = true; // Si muere, deja de caminar
            return;
        }

        // El cerebro toma decisiones cada frame dependiendo de su estado
        switch (estadoActual)
        {
            case EstadoZombi.Deambulando:
                ComportamientoDeambular();
                BuscarJugadorCercano(); // Mientras camina, usa sus "ojos"
                break;

            case EstadoZombi.Persiguiendo:
                ComportamientoPerseguir();
                break;
        }
    }

    private void BuscarJugadorCercano()
    {
        // Buscamos a todos los jugadores en la escena usando la etiqueta
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject jugador in jugadores)
        {
            // Medimos la distancia entre el zombi y este jugador
            float distancia = Vector2.Distance(transform.position, jugador.transform.position);

            // Si el jugador entra en el círculo de visión del zombi...
            if (distancia <= radioVision)
            {
                objetivoJugador = jugador.transform; // Fija el objetivo
                estadoActual = EstadoZombi.Persiguiendo; // ¡Se alerta y empieza a correr!
                break; // Deja de buscar
            }
        }
    }

    private void ComportamientoDeambular()
    {
        // (Aquí programaremos que elija puntos al azar en el NavMesh)
        agente.speed = velocidadMovimiento * 0.5f; // Camina lento cuando está tranquilo
    }

    private void ComportamientoPerseguir()
    {
        agente.speed = velocidadMovimiento; // Corre a máxima velocidad

        // Si tiene un objetivo válido, le decimos al NavMesh que calcule la ruta esquivando paredes
        if (objetivoJugador != null)
        {
            agente.SetDestination(objetivoJugador.position);
        }
    }

    // Método especial para dibujar el "radio de visión" en la pantalla de Unity (útil para ti como programador)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioVision);
    }
}