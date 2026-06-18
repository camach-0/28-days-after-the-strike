using UnityEngine;

public class ControladorMotosierra : ControladorArma
{
    [Header("Configuración de la Motosierra")]
    public float gasolinaMaxima = 100f;
    [Tooltip("Cuánta gasolina gasta por cada segundo de uso")]
    public float consumoPorSegundo = 15f;

    [Header("Daño y Corte")]
    [Tooltip("Daño total infligido por segundo a los zombis en el área")]
    public float danoPorSegundo = 150f;
    public float radioDeCorte = 1.5f;
    public float fuerzaEmpuje = 1f;
    public LayerMask capaEnemigos;

    [Header("Sistema de Botín")]
    public string etiquetaSuelo = "Pickup_Motosierra";

    [Header("Audio")]
    public AudioSource audioMotosierra;
    [Tooltip("Sonido continuo mientras corta")]
    public AudioClip sonidoAtacando;
    [Tooltip("Sonido de motor ahogándose o rompiéndose")]
    public AudioClip sonidoRotura;
    [Tooltip("Velocidad a la que el sonido se apaga al soltar el botón")]
    public float velocidadFadeAudio = 5f; // NUEVO: Controla qué tan rápido se apaga

    private int miIDJugador = -1;
    private float gasolinaActual;
    private bool botonPresionado = false;
    private InventarioJugador miInventario;
    private Vector2 direccionCorte = Vector2.right;

    // NUEVO: Variable para guardar el volumen base configurado en el Inspector
    private float volumenOriginal;

    public override float ModificadorVelocidad => 0.75f;

    private void Awake()
    {
        gasolinaActual = gasolinaMaxima; //

        // Guardamos el volumen original al iniciar
        if (audioMotosierra != null)
        {
            volumenOriginal = audioMotosierra.volume;
        }
    }

    private void Start()
    {
        miInventario = GetComponentInParent<InventarioJugador>(); //[cite: 1]
        JugadorController miJugador = GetComponentInParent<JugadorController>(); //[cite: 1]
        if (miJugador != null) //[cite: 1]
        {
            miIDJugador = miJugador.idJugador; //[cite: 1]
        }
    }

    private void Update()
    {
        if (botonPresionado && gasolinaActual > 0) //[cite: 1]
        {
            if (audioMotosierra != null && sonidoAtacando != null) //[cite: 1]
            {
                // Restaurar el volumen original por si venía de un Fade Out a medias
                audioMotosierra.volume = volumenOriginal;

                if (!audioMotosierra.isPlaying || audioMotosierra.clip != sonidoAtacando) //[cite: 1]
                {
                    audioMotosierra.clip = sonidoAtacando; //[cite: 1]
                    audioMotosierra.loop = true;  //[cite: 1]
                    audioMotosierra.Play(); //[cite: 1]
                }
            }

            CortarZombis(); //[cite: 1]

            gasolinaActual -= consumoPorSegundo * Time.deltaTime; //[cite: 1]

            if (gasolinaActual <= 0) //[cite: 1]
            {
                RomperMotosierra(); //[cite: 1]
            }
        }
        else
        {
            // NUEVA LÓGICA DE AUDIO: Fade Out suave
            if (audioMotosierra != null && audioMotosierra.isPlaying) //[cite: 1]
            {
                // Restamos volumen gradualmente
                audioMotosierra.volume -= velocidadFadeAudio * Time.deltaTime;

                // Si el volumen llega a 0, paramos el audio de verdad
                if (audioMotosierra.volume <= 0f)
                {
                    audioMotosierra.Stop();
                    // Restauramos el volumen en silencio para la próxima vez que se use
                    audioMotosierra.volume = volumenOriginal;
                }
            }
        }

        botonPresionado = false; //[cite: 1]
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (gasolinaActual > 0) //[cite: 1]
        {
            botonPresionado = true; //[cite: 1]
            direccionCorte = direccionApuntado; //[cite: 1]
        }
    }

    private void CortarZombis()
    {
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDisparo.position, radioDeCorte, capaEnemigos); //[cite: 1]

        foreach (Collider2D colision in enemigosGolpeados) //[cite: 1]
        {
            IReceptorDano[] receptores = colision.GetComponentsInParent<IReceptorDano>(); //[cite: 1]
            Vector2 direccionEmpuje = (colision.transform.position - transform.position).normalized; //[cite: 1]

            foreach (IReceptorDano receptor in receptores) //[cite: 1]
            {
                float danoReal = danoPorSegundo * Time.deltaTime; //[cite: 1]
                receptor.RecibirDano(danoReal, direccionEmpuje, fuerzaEmpuje); //[cite: 1]
            }
        }
    }

    private void RomperMotosierra()
    {
        Debug.Log("¡La motosierra se quedó sin gasolina y se rompió!"); //[cite: 1]

        if (sonidoRotura != null) //[cite: 1]
        {
            AudioSource.PlayClipAtPoint(sonidoRotura, transform.position); //[cite: 1]
        }

        if (miInventario != null) //[cite: 1]
        {
            for (int i = 0; i < miInventario.ranuras.Length; i++) //[cite: 1]
            {
                if (miInventario.ranuras[i] == this) //[cite: 1]
                {
                    miInventario.ranuras[i] = null; //[cite: 1]
                    break; //[cite: 1]
                }
            }
            miInventario.CambiarSlot(1); //[cite: 1]
        }

        Destroy(gameObject); //[cite: 1]
    }

    public override void IntentarEmpujon(Vector2 direccion) { } //[cite: 1]

    private void OnDrawGizmosSelected()
    {
        if (puntoDisparo == null) return; //[cite: 1]
        Gizmos.color = Color.red; //[cite: 1]
        Gizmos.DrawWireSphere(puntoDisparo.position, radioDeCorte); //[cite: 1]
    }
}