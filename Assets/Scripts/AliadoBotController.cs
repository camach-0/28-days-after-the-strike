using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(SistemaSalud))]
public class AliadoBotController : MonoBehaviour
{
    [Header("Configuración de IA (Movimiento)")]
    public float distanciaParaSeguir = 2.5f;
    public float velocidadMovimiento = 4.5f;

    [Header("Configuración de IA (Combate)")]
    public float radioDeteccionZombis = 8f;
    public float tiempoEntreDisparos = 0.8f;
    private float temporizadorDisparo = 0f;

    [Header("Armas y Referencias")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public Transform pivoteArma;

    private NavMeshAgent agente;
    private Transform liderActual;
    public SistemaSalud moduloSalud { get; private set; }

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        moduloSalud = GetComponent<SistemaSalud>();

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
        agente.isStopped = true;
        this.enabled = false;
    }

    private void Update()
    {
        if (moduloSalud.vidaActual <= 0) return;

        BuscarLiderMasCercano();
        if (liderActual != null) ComportamientoSeguirLider();

        temporizadorDisparo -= Time.deltaTime;
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

        // Recorremos todos los supervivientes activos registrados de forma directa
        foreach (SistemaSalud superviviente in GameManager.Instancia.supervivientesActivos)
        {
            if (superviviente != null && superviviente.vidaActual > 0)
            {
                JugadorController humano = superviviente.GetComponent<JugadorController>();
                // Si tiene el script humano activo, es un jugador real al que debemos proteger
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

        if (temporizadorDisparo <= 0f && balaPrefab != null && puntoDisparo != null)
        {
            GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);
            nuevaBala.GetComponent<Bala>().ConfigurarDireccion(direccionAlObjetivo);
            temporizadorDisparo = tiempoEntreDisparos;
        }
    }
}