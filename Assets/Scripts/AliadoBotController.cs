using UnityEngine;
using UnityEngine.AI;

public class AliadoBotController : MonoBehaviour
{
    [Header("Configuración de IA")]
    public float distanciaParaSeguir = 2.5f;
    public float velocidadMovimiento = 4.5f;

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

        // Ahora el bot escanea el mapa todo el tiempo para ver qué humano está más cerca
        BuscarLiderMasCercano();

        if (liderActual != null)
        {
            ComportamientoSeguirLider();
        }
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
            // TODO: Aquí pondremos el código de disparar más adelante
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

            // LA CLAVE: Solo seguimos a los humanos que estén vivos
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
}