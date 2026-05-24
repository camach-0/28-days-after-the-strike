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

    [Header("Configuración de Rescate")]
    public float distanciaParaRescatar = 1.5f;
    public float tiempoParaLevantar = 3f;
    public float vidaAlLevantar = 30f;

    [Header("Armas y Referencias")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public Transform pivoteArma;

    private NavMeshAgent agente;
    private Transform liderActual;
    public SistemaSalud moduloSalud { get; private set; }

    // Memoria de rescate
    private SistemaSalud objetivoCaido;
    private float temporizadorRescate = 0f;
    private bool estaRescatando = false;

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
        if (agente != null && agente.isActiveAndEnabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
        }
        this.enabled = false;
    }

    private void Update()
    {
        if (moduloSalud.vidaActual <= 0 || moduloSalud.estaIncapacitado) return;

        temporizadorDisparo -= Time.deltaTime;

        Transform zombiPeligroso = ObtenerZombiMasCercano();
        BuscarCompaneroCaido();

        if (zombiPeligroso != null)
        {
            CancelarRescate();
            ApuntarYDisparar(zombiPeligroso);

            BuscarLiderMasCercano();
            if (liderActual != null)
            {
                ComportamientoMoverHacia(liderActual.position, distanciaParaSeguir);
            }
            else
            {
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
            }
        }
        else
        {
            if (objetivoCaido != null)
            {
                ComportamientoRescate();
            }
            else
            {
                BuscarLiderMasCercano();
                if (liderActual != null) ComportamientoMoverHacia(liderActual.position, distanciaParaSeguir);
                AjustarRotacionArmaAlMoverse();
            }
        }
    }

    private void ComportamientoMoverHacia(Vector3 destino, float distanciaParada)
    {
        float distancia = Vector2.Distance(transform.position, destino);
        if (distancia > distanciaParada)
        {
            agente.isStopped = false;
            agente.SetDestination(destino);
        }
        else
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }
    }

    private void ComportamientoRescate()
    {
        float distancia = Vector2.Distance(transform.position, objetivoCaido.transform.position);

        if (distancia > distanciaParaRescatar)
        {
            agente.isStopped = false;
            agente.SetDestination(objetivoCaido.transform.position);
            estaRescatando = false;
            temporizadorRescate = 0f;
        }
        else
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;

            if (!estaRescatando)
            {
                Debug.Log($"<color=cyan>Bot {gameObject.name} curando a {objetivoCaido.gameObject.name} libre de peligro.</color>");
                estaRescatando = true;
            }

            temporizadorRescate += Time.deltaTime;
            AjustarRotacionArmaAlMoverse();

            if (temporizadorRescate >= tiempoParaLevantar)
            {
                objetivoCaido.LevantarRescatado(vidaAlLevantar);
                Debug.Log($"<color=green>¡Bot {gameObject.name} levantó a {objetivoCaido.gameObject.name}!</color>");
                CancelarRescate();
            }
        }
    }

    private void CancelarRescate()
    {
        estaRescatando = false;
        temporizadorRescate = 0f;
    }

    private Transform ObtenerZombiMasCercano()
    {
        Collider2D[] objetosEnRango = Physics2D.OverlapCircleAll(transform.position, radioDeteccionZombis);
        Transform zombiMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Collider2D obj in objetosEnRango)
        {
            ZombiController zombi = obj.GetComponent<ZombiController>();

            if (zombi != null && zombi.moduloSalud != null && !zombi.moduloSalud.estaMuertoDefinitivo)
            {
                float distancia = Vector2.Distance(transform.position, zombi.transform.position);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    zombiMasCercano = zombi.transform;
                }
            }
        }
        return zombiMasCercano;
    }

    private void BuscarCompaneroCaido()
    {
        if (GameManager.Instancia == null) return;

        float distanciaMasCorta = Mathf.Infinity;
        SistemaSalud caidoMasCercano = null;

        foreach (SistemaSalud superviviente in GameManager.Instancia.supervivientesActivos)
        {
            if (superviviente != null && superviviente != moduloSalud && superviviente.estaIncapacitado)
            {
                float distancia = Vector2.Distance(transform.position, superviviente.transform.position);
                if (distancia < distanciaMasCorta)
                {
                    distanciaMasCorta = distancia;
                    caidoMasCercano = superviviente;
                }
            }
        }
        objetivoCaido = caidoMasCercano;
    }

    private void BuscarLiderMasCercano()
    {
        if (GameManager.Instancia == null) return;

        float distanciaMasCorta = Mathf.Infinity;
        Transform liderMasCercano = null;

        foreach (SistemaSalud superviviente in GameManager.Instancia.supervivientesActivos)
        {
            if (superviviente != null && !superviviente.estaMuertoDefinitivo && !superviviente.estaIncapacitado)
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

    private void AjustarRotacionArmaAlMoverse()
    {
        if (pivoteArma != null && agente.velocity.sqrMagnitude > 0.1f)
        {
            Vector2 direccionViaje = agente.velocity.normalized;
            float angulo = Mathf.Atan2(direccionViaje.y, direccionViaje.x) * Mathf.Rad2Deg;
            pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);
        }
    }
}