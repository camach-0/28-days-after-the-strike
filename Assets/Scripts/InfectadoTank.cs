using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(ZombiController), typeof(SistemaSalud))]
public class InfectadoTank : MonoBehaviour
{
    [Header("Ataque: Puñetazo Masivo")]
    public float rangoPunetazo = 2.5f;
    public float danoPunetazo = 35f;
    public float fuerzaKnockback = 25f;
    public float cooldownPunetazo = 1.5f;

    [Header("Ataque: Lanzar Roca")]
    public string etiquetaRocaPool = "RocaTank";
    public Transform puntoLanzamientoRoca;
    public float cooldownRoca = 6f;
    public float tiempoCasteoRoca = 1.2f;

    [Header("Estado: Fuego")]
    public float multiplicadorVelocidadFuego = 1.4f;
    private bool estaEnLlamas = false;

    [Header("Audios y Música")]
    public AudioClip musicaTank;
    public AudioClip sonidoGruñidoConstante;
    public AudioClip sonidoPunetazo;
    public AudioClip sonidoArrancarRoca;
    public AudioClip sonidoLanzarRoca;
    public AudioClip sonidoFuriaFuego;
    public AudioClip sonidoMuertePesada;

    private ZombiController zombiController;
    private NavMeshAgent agente;
    private SistemaSalud salud;
    private Animator anim;

    private AudioSource fuenteMusica;
    private AudioSource fuenteGruñido;

    private float timerPunetazo = 0f;
    private float timerRoca = 0f;
    private bool realizandoAccionPesada = false;

    private void Awake()
    {
        zombiController = GetComponent<ZombiController>();
        agente = GetComponent<NavMeshAgent>();
        salud = GetComponent<SistemaSalud>();
        anim = GetComponent<Animator>();

        // Configuramos los altavoces constantes del Tank
        fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.spatialBlend = 0f; // 2D (Se escucha en toda la pantalla)
        fuenteMusica.loop = true;

        fuenteGruñido = gameObject.AddComponent<AudioSource>();
        fuenteGruñido.spatialBlend = 1f; // 3D (Se escucha más fuerte si el Tank está cerca)
        fuenteGruñido.loop = true;
    }

    private void OnEnable()
    {
        estaEnLlamas = false;
        realizandoAccionPesada = false;
        if (salud != null) salud.OnMuerte += EjecutarMuertePesada;

        // Iniciar Música y Gruñidos
        if (musicaTank != null) { fuenteMusica.clip = musicaTank; fuenteMusica.Play(); }
        if (sonidoGruñidoConstante != null) { fuenteGruñido.clip = sonidoGruñidoConstante; fuenteGruñido.Play(); }
    }

    private void OnDisable()
    {
        if (salud != null) salud.OnMuerte -= EjecutarMuertePesada;
        fuenteMusica.Stop();
        fuenteGruñido.Stop();
    }

    private void Update()
    {
        if (salud.vidaActual <= 0 || realizandoAccionPesada) return;

        Transform objetivo = zombiController.objetivoJugador;

        if (objetivo != null)
        {
            // ¡MECÁNICA NUEVA! Si el objetivo cae al suelo, el Tank lo ignora y busca otro.
            SistemaSalud saludObjetivo = objetivo.GetComponent<SistemaSalud>();
            if (saludObjetivo != null && (saludObjetivo.estaIncapacitado || saludObjetivo.estaMuertoDefinitivo))
            {
                zombiController.objetivoJugador = null; // Forza al ZombiController a escanear de nuevo
                return;
            }

            float distancia = Vector2.Distance(transform.position, objetivo.position);

            if (distancia <= rangoPunetazo && Time.time >= timerPunetazo)
            {
                StartCoroutine(RutinaPunetazo(objetivo));
            }
            else if (distancia > rangoPunetazo && Time.time >= timerRoca)
            {
                Vector2 direccionAlJugador = (objetivo.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador, distancia, LayerMask.GetMask("Obstaculos"));

                if (hit.collider == null)
                {
                    StartCoroutine(RutinaLanzarRoca(objetivo));
                }
            }
        }
    }

    private IEnumerator RutinaPunetazo(Transform objetivo)
    {
        realizandoAccionPesada = true;
        agente.isStopped = true;

        if (anim != null) anim.SetTrigger("Atacar");
        yield return new WaitForSeconds(0.3f);

        if (sonidoPunetazo != null) AudioSource.PlayClipAtPoint(sonidoPunetazo, transform.position);

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
        timerRoca = Time.time + cooldownRoca;

        if (anim != null) anim.SetTrigger("ArrancarRoca");
        if (sonidoArrancarRoca != null) AudioSource.PlayClipAtPoint(sonidoArrancarRoca, transform.position);

        yield return new WaitForSeconds(tiempoCasteoRoca);

        if (salud.vidaActual > 0 && objetivo != null)
        {
            if (anim != null) anim.SetTrigger("LanzarRoca");
            if (sonidoLanzarRoca != null) AudioSource.PlayClipAtPoint(sonidoLanzarRoca, transform.position);

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

    public void EnfurecerPorFuego()
    {
        if (!estaEnLlamas)
        {
            estaEnLlamas = true;
            zombiController.velocidadMovimiento *= multiplicadorVelocidadFuego;
            zombiController.RestaurarVelocidad();

            if (sonidoFuriaFuego != null) AudioSource.PlayClipAtPoint(sonidoFuriaFuego, transform.position);
        }
    }

    private void EjecutarMuertePesada()
    {
        if (sonidoMuertePesada != null) AudioSource.PlayClipAtPoint(sonidoMuertePesada, transform.position);
        fuenteMusica.Stop();
        fuenteGruñido.Stop();
    }
}