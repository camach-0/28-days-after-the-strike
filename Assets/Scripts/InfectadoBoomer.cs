using UnityEngine;

[RequireComponent(typeof(SistemaSalud), typeof(ZombiController))]
public class InfectadoBoomer : MonoBehaviour
{
    [Header("Configuración de Explosión (Muerte)")]
    public float radioExplosion = 3f;
    public float danoExplosion = 25f;
    public float fuerzaEmpuje = 10f;
    public LayerMask capaObjetivos;
    public string etiquetaExplosion = "ExplosionBoomer";
    public AudioClip sonidoExplosion;

    [Header("Configuración de Vómito (Ataque)")]
    public float distanciaVomito = 4.5f; // Desde qué distancia lanza el vómito
    public float tiempoPreparacion = 0.5f; // Medio segundo de arcada antes de escupir
    public float tiempoHuida = 3.5f; // Cuánto tiempo huye antes de volver a atacar
    public float cooldownVomito = 6f; // Segundos a esperar para vomitar de nuevo
    public string etiquetaVomito = "VomitoBoomer"; // Nombre del proyectil en el PoolManager

    [HideInInspector] public ZombiController miCerebro;
    [HideInInspector] public float ultimoVomito = -10f;
    [HideInInspector] public bool enModoEspecial = false; // Candado para no interrumpir sus acciones

    private SistemaSalud miSalud;

    // Nuestros dos estados personalizados para hackear la IA del zombi normal
    public EstadoVomitarBoomer estadoVomitar = new EstadoVomitarBoomer();
    public EstadoHuirBoomer estadoHuir = new EstadoHuirBoomer();

    private void Awake()
    {
        miCerebro = GetComponent<ZombiController>();
        miSalud = GetComponent<SistemaSalud>();
    }

    private void OnEnable()
    {
        if (miSalud != null) miSalud.OnMuerte += Explotar;
        enModoEspecial = false;
    }

    private void OnDisable()
    {
        if (miSalud != null) miSalud.OnMuerte -= Explotar;
    }

    private void Update()
    {
        if (miSalud.vidaActual <= 0 || miCerebro.objetivoJugador == null || enModoEspecial) return;

        float distancia = Vector2.Distance(transform.position, miCerebro.objetivoJugador.position);

        // Si está lo suficientemente cerca y ya tiene el vómito recargado... ¡Ataca!
        if (distancia <= distanciaVomito && Time.time >= ultimoVomito + cooldownVomito)
        {
            miCerebro.CambiarEstado(estadoVomitar);
        }
    }

    private void Explotar()
    {
        // 1. Efecto Visual/Sonoro de la explosión al morir
        GameObject efecto = PoolManager.Instancia.SolicitarObjeto(etiquetaExplosion, transform.position, Quaternion.identity);
        if (efecto != null && sonidoExplosion != null)
        {
            AudioReciclable audio = efecto.GetComponent<AudioReciclable>();
            if (audio != null) audio.Reproducir(sonidoExplosion);
        }

        // 2. Físicas de la explosión
        Collider2D[] afectados = Physics2D.OverlapCircleAll(transform.position, radioExplosion, capaObjetivos);
        foreach (Collider2D col in afectados)
        {
            if (col.gameObject == this.gameObject) continue;

            IReceptorDano receptor = col.GetComponent<IReceptorDano>();
            if (receptor != null)
            {
                Vector2 direccionEmpuje = (col.transform.position - transform.position).normalized;
                receptor.RecibirDano(danoExplosion, direccionEmpuje, fuerzaEmpuje);
            }
            // --- ¡NUEVO! Si estabas en el radio de explosión, te manchas y llamas a la horda ---
            EfectoBilis bilis = col.GetComponent<EfectoBilis>();
            if (bilis != null)
            {
                bilis.RecibirVomito();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);

        Gizmos.color = Color.yellow; // Círculo amarillo para ver su rango de vómito
        Gizmos.DrawWireSphere(transform.position, distanciaVomito);
    }
}

// =========================================================================
// LAS NUEVAS NEURONAS DEL BOOMER (Estados Personalizados)
// =========================================================================

public class EstadoVomitarBoomer : IEstadoZombi
{
    private float tiempoInicio;

    public void Entrar(ZombiController zombi)
    {
        zombi.agente.isStopped = true; // Se queda quieto
        zombi.agente.velocity = Vector3.zero;
        tiempoInicio = Time.time;

        InfectadoBoomer boomer = zombi.GetComponent<InfectadoBoomer>();
        boomer.enModoEspecial = true; // Ponemos el candado

        // ¡Aquí iría la animación de inflarse y el sonido de arcada!
    }

    public void Actualizar(ZombiController zombi)
    {
        InfectadoBoomer boomer = zombi.GetComponent<InfectadoBoomer>();

        // Una vez que termina de prepararse, lanza el vómito
        if (Time.time >= tiempoInicio + boomer.tiempoPreparacion)
        {
            if (zombi.objetivoJugador != null)
            {
                // Calcula el ángulo hacia el jugador
                Vector2 direccion = (zombi.objetivoJugador.position - zombi.transform.position).normalized;
                float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

                // Instancia el vómito apuntando directamente al jugador
                PoolManager.Instancia.SolicitarObjeto(boomer.etiquetaVomito, zombi.transform.position, Quaternion.Euler(0, 0, angulo));
            }

            boomer.ultimoVomito = Time.time; // Reiniciamos el cooldown
            zombi.CambiarEstado(boomer.estadoHuir); // Pasamos inmediatamente a la retirada
        }
    }

    public void Salir(ZombiController zombi) { }
}

public class EstadoHuirBoomer : IEstadoZombi
{
    private float tiempoInicio;

    public void Entrar(ZombiController zombi)
    {
        zombi.agente.isStopped = false;
        zombi.agente.speed = zombi.velocidadMovimiento;
        tiempoInicio = Time.time;
    }

    public void Actualizar(ZombiController zombi)
    {
        InfectadoBoomer boomer = zombi.GetComponent<InfectadoBoomer>();

        if (zombi.objetivoJugador != null)
        {
            // Busca un punto a espaldas del Boomer, alejado del jugador
            Vector2 direccionLejos = (zombi.transform.position - zombi.objetivoJugador.position).normalized;
            Vector2 puntoHuida = (Vector2)zombi.transform.position + (direccionLejos * 5f);
            zombi.agente.SetDestination(puntoHuida);
        }

        // Si se acaba el tiempo de huida, su cerebro vuelve a la normalidad
        if (Time.time >= tiempoInicio + boomer.tiempoHuida)
        {
            boomer.enModoEspecial = false; // Quitamos el candado
            zombi.CambiarEstado(zombi.estadoDeambular); // El zombi normal retomará la cacería
        }
    }

    public void Salir(ZombiController zombi)
    {
        zombi.agente.speed = zombi.velocidadMovimiento; // Restaura su lentitud original al terminar
    }
}