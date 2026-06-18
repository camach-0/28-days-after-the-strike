using UnityEngine;
using UnityEngine.AI;
using Unity.Cinemachine;

public class StunController : MonoBehaviour
{
    public enum TipoStun { Ninguno, Hunter, Jockey }

    private JugadorController jugadorController;
    private AliadoBotController botController;
    private NavMeshAgent agente;
    private TipoStun estadoActual = TipoStun.Ninguno;

    private Transform objetivoZombi;
    private float tiempoPatrulla = 0f;
    public float radioDeambular = 6f;

    private CinemachineImpulseSource impulseSource;

    void Start()
    {
        jugadorController = GetComponent<JugadorController>();
        botController = GetComponent<AliadoBotController>();
        agente = GetComponent<NavMeshAgent>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // 🔹 Stun Hunter → bloqueo total
    public void AplicarStunHunter()
    {
        estadoActual = TipoStun.Hunter;

        if (jugadorController != null) jugadorController.enabled = false;
        if (botController != null) botController.enabled = false;

        if (agente != null)
        {
            agente.isStopped = true;
            agente.enabled = false;
        }

        // Vibración de cámara
        impulseSource?.GenerateImpulse();
    }

    public void LiberarStunHunter()
    {
        if (estadoActual == TipoStun.Hunter)
        {
            if (jugadorController != null) jugadorController.enabled = true;
            if (botController != null) botController.enabled = true;

            estadoActual = TipoStun.Ninguno;
        }
    }

    // 🔹 Stun Jockey → redirigir movimiento
    public void AplicarStunJockey(Transform zombiObjetivo)
    {
        estadoActual = TipoStun.Jockey;

        if (jugadorController != null) jugadorController.enabled = false;
        if (botController != null) botController.enabled = false;

        if (agente != null)
        {
            agente.enabled = true;
            agente.isStopped = false;
            objetivoZombi = zombiObjetivo;
        }

        // Vibración de cámara
        impulseSource?.GenerateImpulse();
    }

    void Update()
    {
        if (estadoActual == TipoStun.Jockey && agente != null)
        {
            if (objetivoZombi != null)
            {
                // Seguir al zombi más cercano
                agente.SetDestination(objetivoZombi.position);
            }
            else
            {
                // Deambular errático si no hay zombis
                if (!agente.pathPending && (agente.remainingDistance <= agente.stoppingDistance + 0.1f || !agente.hasPath))
                {
                    tiempoPatrulla -= Time.deltaTime;
                    if (tiempoPatrulla <= 0f)
                    {
                        Vector2 puntoAleatorio = (Vector2)transform.position + Random.insideUnitCircle * radioDeambular;
                        if (NavMesh.SamplePosition(puntoAleatorio, out NavMeshHit hit, radioDeambular, NavMesh.AllAreas))
                        {
                            agente.SetDestination(hit.position);
                            tiempoPatrulla = Random.Range(1f, 3f);
                        }
                    }
                }
            }
        }
    }

    public void LiberarStunJockey()
    {
        if (estadoActual == TipoStun.Jockey)
        {
            if (jugadorController != null) jugadorController.enabled = true;
            if (botController != null) botController.enabled = true;

            if (agente != null)
            {
                agente.isStopped = true;
                agente.enabled = false;
            }

            objetivoZombi = null;
            estadoActual = TipoStun.Ninguno;
        }
    }
}



