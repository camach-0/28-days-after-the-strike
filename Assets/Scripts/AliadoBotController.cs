using UnityEngine;
using UnityEngine.AI;

public class AliadoBotController : MonoBehaviour
{
    [Header("Configuración de IA (Movimiento)")]
    public float distanciaParaSeguir = 2.5f;
    public float velocidadMovimiento = 4.5f;

    [Header("Configuración de IA (Combate)")]
    public float radioDeteccionZombis = 8f; // Qué tan lejos pueden "ver" a los zombis
    public float tiempoEntreDisparos = 0.8f; // Cadencia de tiro (disparan casi cada segundo)
    private float temporizadorDisparo = 0f;

    [Header("Armas y Referencias")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public Transform pivoteArma;

    private NavMeshAgent agente;
    private Transform liderActual;
    private Entidad miEntidad;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        miEntidad = GetComponent<Entidad>();

        if (agente != null)
        {
            agente.updateRotation = false;
            agente.updateUpAxis = false;
            agente.speed = velocidadMovimiento;
        }
    }

    private void Update()
    {
        if (miEntidad != null && miEntidad.estaMuerto) return;

        // 1. Movimiento: Seguir al líder
        BuscarLiderMasCercano();
        if (liderActual != null)
        {
            ComportamientoSeguirLider();
        }

        // 2. Combate: Buscar enemigos y disparar
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
        GameObject[] posiblesLideres = GameObject.FindGameObjectsWithTag("Player");
        float distanciaMasCorta = Mathf.Infinity;
        Transform liderMasCercano = null;

        foreach (GameObject jugador in posiblesLideres)
        {
            JugadorController controlHumano = jugador.GetComponent<JugadorController>();
            Entidad vidaJugador = jugador.GetComponent<Entidad>();

            // Solo seguimos a los humanos que estén vivos
            if (controlHumano != null && controlHumano.enabled && !vidaJugador.estaMuerto)
            {
                float distancia = Vector2.Distance(transform.position, jugador.transform.position);
                if (distancia < distanciaMasCorta)
                {
                    distanciaMasCorta = distancia;
                    liderMasCercano = jugador.transform;
                }
            }
        }
        liderActual = liderMasCercano;
    }

    // --- NUEVA LÓGICA DE COMBATE ---
    private void AtacarZombiMasCercano()
    {
        // El "Radar": Trazamos un círculo invisible para detectar colisiones cercanas
        Collider2D[] objetosEnRango = Physics2D.OverlapCircleAll(transform.position, radioDeteccionZombis);
        Transform zombiMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Collider2D obj in objetosEnRango)
        {
            // Verificamos si lo que tocó el radar es un zombi y si está vivo
            ZombiController zombi = obj.GetComponent<ZombiController>();
            if (zombi != null && !zombi.estaMuerto)
            {
                float distancia = Vector2.Distance(transform.position, zombi.transform.position);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    zombiMasCercano = zombi.transform;
                }
            }
        }

        // Si encontramos un zombi en el radar, le disparamos
        if (zombiMasCercano != null)
        {
            ApuntarYDisparar(zombiMasCercano);
        }
        else
        {
            // Si no hay zombis, que el arma mire hacia donde estamos caminando
            if (pivoteArma != null && agente.velocity.sqrMagnitude > 0.1f)
            {
                Vector2 direccionViaje = agente.velocity.normalized;
                float angulo = Mathf.Atan2(direccionViaje.y, direccionViaje.x) * Mathf.Rad2Deg;
                pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);
            }
        }
    }

    private void ApuntarYDisparar(Transform objetivo)
    {
        if (pivoteArma == null) return;

        // 1. Apuntar directo al zombi
        Vector2 direccionAlObjetivo = (objetivo.position - pivoteArma.position).normalized;
        float angulo = Mathf.Atan2(direccionAlObjetivo.y, direccionAlObjetivo.x) * Mathf.Rad2Deg;
        pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);

        // 2. Apretar el gatillo si el arma está recargada
        if (temporizadorDisparo <= 0f && balaPrefab != null && puntoDisparo != null)
        {
            GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);

            // Le decimos a la bala hacia dónde ir usando el script que ya tienes creado
            nuevaBala.GetComponent<Bala>().ConfigurarDireccion(direccionAlObjetivo);

            // Reiniciamos el temporizador para no ametrallar
            temporizadorDisparo = tiempoEntreDisparos;
        }
    }

    // Dibujamos el "Radar" en la escena para que tú (el programador) puedas verlo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccionZombis);
    }
}