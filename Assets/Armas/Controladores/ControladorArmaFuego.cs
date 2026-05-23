using UnityEngine;
using System.Collections;

public class ControladorArmaFuego : ControladorArma
{
    [Header("Configuración del Arma")]
    public DatosArmaFuego datosFuego;

    [Tooltip("Debe ser el mismo nombre que pusiste en la Etiqueta del PoolManager")]
    public string etiquetaBala = "BalaBase";

    [Header("Estado Actual")]
    public int municionActualCargador;
    public int municionActualReserva;
    public bool estaRecargando = false;

    private bool estaDisparandoRafaga = false;
    private bool estaDesplegando = false; // ¡NUEVO! Bloqueo al sacar el arma

    // ¡NUEVO! Variables dinámicas
    private float recoilActual = 0f;
    private float tiempoProximoEmpujon = 0f;
    private Rigidbody2D rbJugador; // Para saber si se está moviendo

    // Exponemos el peso del arma al Jugador
    public override float ModificadorVelocidad => datosFuego != null ? datosFuego.modificadorVelocidad : 1f;

    private void Awake()
    {
        rbJugador = GetComponentInParent<Rigidbody2D>();
    }

    void Start()
    {
        if (datosFuego != null)
        {
            municionActualCargador = datosFuego.tamanoCargador;
            municionActualReserva = datosFuego.municionMaxima;
        }
    }

    private void OnEnable()
    {
        estaRecargando = false;
        estaDisparandoRafaga = false;
        recoilActual = 0f;
        StartCoroutine(CorrutinaDespliegue());
    }

    private IEnumerator CorrutinaDespliegue()
    {
        estaDesplegando = true;
        if (datosFuego != null) yield return new WaitForSeconds(datosFuego.tiempoDespliegue);
        else yield return null;
        estaDesplegando = false;
    }

    private void Update()
    {
        // El cono de dispersión se enfría (se cierra) gradualmente cuando no disparas
        if (datosFuego != null && recoilActual > 0)
        {
            recoilActual -= datosFuego.velocidadRecuperacion * Time.deltaTime;
            if (recoilActual < 0) recoilActual = 0;
        }
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (datosFuego == null || estaRecargando || estaDisparandoRafaga || estaDesplegando) return;

        if (Time.time >= tiempoProximoAtaque && municionActualCargador > 0)
        {
            if (datosFuego.esRafaga)
            {
                StartCoroutine(CorrutinaRafaga(direccionApuntado));
            }
            else
            {
                tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
                GastarBalaYDisparar(direccionApuntado);
            }
        }
    }

    private void GastarBalaYDisparar(Vector2 direccionApuntado)
    {
        municionActualCargador--;
        EjecutarDisparo(direccionApuntado);

        if (municionActualCargador <= 0) IniciarRecarga();
    }

    // --- ¡MECÁNICA NUEVA: EL CULATAZO (SHOVE)! ---
    public override void IntentarEmpujon(Vector2 direccion)
    {
        if (datosFuego == null || Time.time < tiempoProximoEmpujon || estaDesplegando) return;

        tiempoProximoEmpujon = Time.time + datosFuego.cadenciaEmpujon;
        Debug.Log("¡Culatazo de Arma de Fuego!");

        // Buscamos zombis en un semicírculo frente al jugador
        Collider2D[] golpeados = Physics2D.OverlapCircleAll(puntoDisparo.position, datosFuego.alcanceEmpujon);
        foreach (Collider2D col in golpeados)
        {
            if (col.CompareTag("Enemy"))
            {
                IReceptorDano receptor = col.GetComponent<IReceptorDano>();
                // Enviamos 0 de daño, pero máxima fuerza de empuje para alejarlos
                if (receptor != null) receptor.RecibirDano(0f, direccion, datosFuego.fuerzaDelCulatazo);
            }
        }
    }

    IEnumerator CorrutinaRafaga(Vector2 direccionApuntado)
    {
        estaDisparandoRafaga = true;

        for (int i = 0; i < datosFuego.balasPorRafaga; i++)
        {
            if (municionActualCargador <= 0) break;

            GastarBalaYDisparar(direccionApuntado);

            if (municionActualCargador > 0 && i < datosFuego.balasPorRafaga - 1)
            {
                yield return new WaitForSeconds(datosFuego.tiempoEntreBalasRafaga);
            }
        }

        tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
        estaDisparandoRafaga = false;
    }

    void EjecutarDisparo(Vector2 direccionApuntado)
    {
        if (datosFuego.sonidoAtaque != null)
        {
            GameObject objSonido = PoolManager.Instancia.SolicitarObjeto("EfectoSonido", transform.position, Quaternion.identity);
            if (objSonido != null)
            {
                objSonido.GetComponent<AudioReciclable>().Reproducir(datosFuego.sonidoAtaque);
            }
        }

        // ¡MECÁNICA DE PRECISIÓN DINÁMICA!
        float baseDispersion = datosFuego.dispersionMinima;
        if (rbJugador != null && rbJugador.linearVelocity.sqrMagnitude > 0.5f)
        {
            baseDispersion = datosFuego.dispersionMaxima; // Si corres, se abre la mira
        }

        float dispersionTotal = baseDispersion + recoilActual;
        recoilActual += datosFuego.incrementoRecoil; // El arma patea al disparar

        for (int i = 0; i < datosFuego.perdigonesPorDisparo; i++)
        {
            float anguloBase = Mathf.Atan2(direccionApuntado.y, direccionApuntado.x) * Mathf.Rad2Deg;
            float anguloDispersion = Random.Range(-dispersionTotal, dispersionTotal);
            Quaternion rotacionFinalBala = Quaternion.Euler(0, 0, anguloBase + anguloDispersion);

            GameObject nuevaBala = PoolManager.Instancia.SolicitarObjeto(etiquetaBala, puntoDisparo.position, rotacionFinalBala);

            if (nuevaBala != null)
            {
                Bala scriptBala = nuevaBala.GetComponent<Bala>();

                // ¡AQUÍ LE PASAMOS ABSOLUTAMENTE TODO A LA BALA!
                scriptBala.ConfigurarBala(
                    rotacionFinalBala * Vector2.right, // Dirección
                    (int)datosFuego.danoBase,          // Daño
                    datosFuego.fuerzaEmpuje,           // Knockback
                    datosFuego.penetracionZombis,      // Penetración
                    datosFuego.alcance                 // Alcance (Tiempo de vida)
                );
            }
        }
    }

    public void IniciarRecarga()
    {
        if (!estaRecargando && municionActualCargador < datosFuego.tamanoCargador)
        {
            if (datosFuego.reservaInfinita || municionActualReserva > 0)
            {
                StartCoroutine(CorrutinaRecarga());
            }
        }
    }

    IEnumerator CorrutinaRecarga()
    {
        estaRecargando = true;
        yield return new WaitForSeconds(datosFuego.tiempoRecarga);

        int balasFaltantes = datosFuego.tamanoCargador - municionActualCargador;

        if (datosFuego.reservaInfinita)
        {
            municionActualCargador += balasFaltantes;
        }
        else
        {
            int balasATomar = Mathf.Min(balasFaltantes, municionActualReserva);
            municionActualCargador += balasATomar;
            municionActualReserva -= balasATomar;
        }

        estaRecargando = false;
    }
}