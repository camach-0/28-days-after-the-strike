using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(SistemaSalud))]
[RequireComponent(typeof(JugadorCombate))]
public class AliadoBotController : MonoBehaviour
{
    [Header("Configuración de IA (Movimiento)")]
    public float distanciaParaSeguir = 2.5f;
    public float velocidadMovimiento = 4.5f;

    [Header("Configuración de IA (Combate)")]
    public float radioDeteccionZombis = 8f;

    [Header("Armas y Referencias")]
    public Transform pivoteArma;

    private NavMeshAgent agente;
    private Transform liderActual;
    private JugadorCombate moduloCombate; // Lo mantenemos para saber qué arma tiene sujeta
    public SistemaSalud moduloSalud { get; private set; }

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        moduloSalud = GetComponent<SistemaSalud>();
        moduloCombate = GetComponent<JugadorCombate>();

        if (agente != null)
        {
            agente.updateRotation = false;
            agente.updateUpAxis = false;
            agente.speed = velocidadMovimiento;
        }
    }

    private void Start()
    {
        if (moduloSalud != null) moduloSalud.OnMuerte += ApagarCerebro;
    }

    private void OnDestroy()
    {
        if (moduloSalud != null) moduloSalud.OnMuerte -= ApagarCerebro;
    }

    private void ApagarCerebro()
    {
        if (agente != null && agente.isActiveAndEnabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
        }

        this.enabled = false;
    }

    private void Update()
    {
        if (moduloSalud.vidaActual <= 0) return;

        BuscarLiderMasCercano();
        if (liderActual != null) ComportamientoSeguirLider();

        AtacarZombiMasCercano();
    }

    private void ComportamientoSeguirLider()
    {
        float distanciaAlLider = Vector2.Distance(transform.position, liderActual.position);

        if (distanciaAlLider > distanciaParaSeguir)
        {
            agente.isStopped = false;
            agente.SetDestination(liderActual.position);
        }
        else
        {
            agente.isStopped = true;
        }
    }

    private void BuscarLiderMasCercano()
    {
        if (GameManager.Instancia == null) return;

        float distanciaMasCorta = Mathf.Infinity;
        Transform liderMasCercano = null;

        foreach (SistemaSalud superviviente in GameManager.Instancia.supervivientesActivos)
        {
            if (superviviente != null && superviviente.vidaActual > 0)
            {
                JugadorController humano = superviviente.GetComponent<JugadorController>();
                if (humano != null && humano.enabled)
                {
                    float distancia = Vector2.Distance(transform.position, superviviente.transform.position);
                    if (distancia < distanciaMasCorta)
                    {
                        distanciaMasCorta = distancia;
                        liderMasCercano = superviviente.transform;
                    }
                }
            }
        }
        liderActual = liderMasCercano;
    }

    private void AtacarZombiMasCercano()
    {
        Collider2D[] objetosEnRango = Physics2D.OverlapCircleAll(transform.position, radioDeteccionZombis);
        Transform zombiMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Collider2D obj in objetosEnRango)
        {
            ZombiController zombi = obj.GetComponent<ZombiController>();

            if (zombi != null && zombi.moduloSalud != null && zombi.moduloSalud.vidaActual > 0)
            {
                float distancia = Vector2.Distance(transform.position, zombi.transform.position);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    zombiMasCercano = zombi.transform;
                }
            }
        }

        if (zombiMasCercano != null)
        {
            ApuntarYDisparar(zombiMasCercano);
        }
        else if (pivoteArma != null && agente.velocity.sqrMagnitude > 0.1f)
        {
            Vector2 direccionViaje = agente.velocity.normalized;
            float angulo = Mathf.Atan2(direccionViaje.y, direccionViaje.x) * Mathf.Rad2Deg;
            pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);
        }
    }

    private void ApuntarYDisparar(Transform objetivo)
    {
        if (pivoteArma == null) return;

        Vector2 direccionAlObjetivo = (objetivo.position - pivoteArma.position).normalized;
        float angulo = Mathf.Atan2(direccionAlObjetivo.y, direccionAlObjetivo.x) * Mathf.Rad2Deg;
        pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);

        // ====================================================================
        // --- ¡LA SOLUCIÓN DE SOBERANÍA BALÍSTICA! ---
        // ====================================================================
        // En lugar de pasar por el filtro de clicks de humanos, atacamos directo.
        // El arma misma decidirá si puede disparar o no según su cadencia interna.
        if (moduloCombate != null && moduloCombate.armaEquipada != null)
        {
            moduloCombate.armaEquipada.IntentarAtaque(direccionAlObjetivo);
        }
    }
}