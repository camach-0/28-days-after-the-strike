using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(ZombiController), typeof(SistemaSalud))]
public class InfectadoTank : MonoBehaviour
{
    [Header("Ataque: Puñetazo Masivo")]
    public float rangoPunetazo = 2.5f;
    public float danoPunetazo = 35f;
    public float fuerzaKnockback = 25f; // Knockback masivo del diagrama
    public float cooldownPunetazo = 1.5f;

    [Header("Ataque: Lanzar Roca")]
    [Tooltip("Etiqueta para usar con tu PoolManager")]
    public string etiquetaRocaPool = "RocaTank";
    public Transform puntoLanzamientoRoca;
    public float cooldownRoca = 6f;
    public float tiempoCasteoRoca = 1.2f; // Tiempo arrancando la roca antes de lanzarla

    [Header("Estado: Fuego")]
    public float multiplicadorVelocidadFuego = 1.4f;
    private bool estaEnLlamas = false;

    [Header("Audios Listos para Agregar")]
    public AudioClip sonidoPunetazo;
    public AudioClip sonidoArrancarRoca;
    public AudioClip sonidoLanzarRoca;
    public AudioClip sonidoFuriaFuego;
    public AudioClip sonidoMuertePesada;

    private ZombiController zombiController;
    private NavMeshAgent agente;
    private SistemaSalud salud;
    private Animator anim;

    private float timerPunetazo = 0f;
    private float timerRoca = 0f;
    private bool realizandoAccionPesada = false;

    private void Awake()
    {
        zombiController = GetComponent<ZombiController>();
        agente = GetComponent<NavMeshAgent>();
        salud = GetComponent<SistemaSalud>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        estaEnLlamas = false;
        realizandoAccionPesada = false;
        if (salud != null) salud.OnMuerte += EjecutarMuertePesada; // Evento Global de Daño
    }

    private void OnDisable()
    {
        if (salud != null) salud.OnMuerte -= EjecutarMuertePesada;
    }

    private void Update()
    {
        // Si está muerto o en medio de una animación de ataque, no toma nuevas decisiones
        if (salud.vidaActual <= 0 || realizandoAccionPesada) return;

        Transform objetivo = zombiController.objetivoJugador;

        if (objetivo != null)
        {
            float distancia = Vector2.Distance(transform.position, objetivo.position);

            // DIAGRAMA: ¿Jugador cerca y camino libre? -> Puñetazo con Knockback
            if (distancia <= rangoPunetazo && Time.time >= timerPunetazo)
            {
                StartCoroutine(RutinaPunetazo(objetivo));
            }
            // DIAGRAMA: ¿Roca recargada? -> Lanzar Roca (Si está lejos)
            else if (distancia > rangoPunetazo && Time.time >= timerRoca)
            {
                // Validación extra: Comprobar que no haya una pared entre el Tank y el jugador
                Vector2 direccionAlJugador = (objetivo.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador, distancia, LayerMask.GetMask("Obstaculos"));

                if (hit.collider == null) // Camino visual libre
                {
                    StartCoroutine(RutinaLanzarRoca(objetivo));
                }
            }
            // DIAGRAMA: Si no -> Perseguir Objetivo destructivamente 
            // (Tu ZombiController ya se encarga de mover el NavMeshAgent hacia el jugador)
        }
    }

    private IEnumerator RutinaPunetazo(Transform objetivo)
    {
        realizandoAccionPesada = true;
        agente.isStopped = true; // Se frena para golpear

        if (anim != null) anim.SetTrigger("Atacar");

        yield return new WaitForSeconds(0.3f); // Sincronización con la animación

        if (sonidoPunetazo != null) AudioSource.PlayClipAtPoint(sonidoPunetazo, transform.position);

        // Aplica el daño y el knockback si el jugador no lo esquivó
        if (objetivo != null && Vector2.Distance(transform.position, objetivo.position) <= rangoPunetazo + 1f)
        {
            SistemaSalud saludObjetivo = objetivo.GetComponent<SistemaSalud>();
            if (saludObjetivo != null)
            {
                Vector2 direccionGolpe = (objetivo.position - transform.position).normalized;
                saludObjetivo.RecibirDano(danoPunetazo, direccionGolpe, fuerzaKnockback);
            }
        }

        timerPunetazo = Time.time + cooldownPunetazo;
        realizandoAccionPesada = false;
        agente.isStopped = false;
    }

    private IEnumerator RutinaLanzarRoca(Transform objetivo)
    {
        realizandoAccionPesada = true;
        agente.isStopped = true;
        timerRoca = Time.time + cooldownRoca; // Inicia el cooldown

        if (anim != null) anim.SetTrigger("ArrancarRoca");
        if (sonidoArrancarRoca != null) AudioSource.PlayClipAtPoint(sonidoArrancarRoca, transform.position);

        yield return new WaitForSeconds(tiempoCasteoRoca);

        if (salud.vidaActual > 0 && objetivo != null)
        {
            if (anim != null) anim.SetTrigger("LanzarRoca");
            if (sonidoLanzarRoca != null) AudioSource.PlayClipAtPoint(sonidoLanzarRoca, transform.position);

            // Generar proyectil con PoolManager
            GameObject roca = PoolManager.Instancia.SolicitarObjeto(etiquetaRocaPool, puntoLanzamientoRoca.position, Quaternion.identity);
            if (roca != null)
            {
                ProyectilRoca scriptRoca = roca.GetComponent<ProyectilRoca>();
                if (scriptRoca != null)
                {
                    Vector2 direccionRoca = (objetivo.position - puntoLanzamientoRoca.position).normalized;
                    scriptRoca.Lanzar(direccionRoca);
                }
            }
        }

        realizandoAccionPesada = false;
        agente.isStopped = false;
    }

    // DIAGRAMA: Pierde HP continuamente + Aumento de velocidad
    // Conecta esta función a tu script "SensibilidadFuego" o al trigger del molotov
    public void EnfurecerPorFuego()
    {
        if (!estaEnLlamas)
        {
            estaEnLlamas = true;
            zombiController.velocidadMovimiento *= multiplicadorVelocidadFuego;
            zombiController.RestaurarVelocidad(); // Actualiza el NavMeshAgent

            if (sonidoFuriaFuego != null) AudioSource.PlayClipAtPoint(sonidoFuriaFuego, transform.position);
            // La pérdida de HP continua la debe seguir manejando tu componente "SensibilidadFuego"
        }
    }

    private void EjecutarMuertePesada()
    {
        if (sonidoMuertePesada != null) AudioSource.PlayClipAtPoint(sonidoMuertePesada, transform.position);
        // Desaparecer / Object Pool se ejecuta mediante tu ZombiController o el trigger del Animator
    }
}