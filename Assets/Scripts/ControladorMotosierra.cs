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
 

    private float gasolinaActual;
    private bool botonPresionado = false;
    private InventarioJugador miInventario;
    private Vector2 direccionCorte = Vector2.right;

    public override float ModificadorVelocidad => 0.75f;

    private void Awake()
    {
        gasolinaActual = gasolinaMaxima;
    }

    private void Start()
    {
        miInventario = GetComponentInParent<InventarioJugador>();
    }

    private void Update()
    {
   
        if (botonPresionado && gasolinaActual > 0)
        {
      
            if (audioMotosierra != null && sonidoAtacando != null)
            {
                if (!audioMotosierra.isPlaying || audioMotosierra.clip != sonidoAtacando)
                {
                    audioMotosierra.clip = sonidoAtacando;
                    audioMotosierra.loop = true; 
                    audioMotosierra.Play();
                }
            }

            CortarZombis();

        
            gasolinaActual -= consumoPorSegundo * Time.deltaTime;

            if (gasolinaActual <= 0)
            {
                RomperMotosierra();
            }
        }
        else
        {
        
            if (audioMotosierra != null && audioMotosierra.isPlaying)
            {
                audioMotosierra.Stop();
            }
        }

        botonPresionado = false;
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (gasolinaActual > 0)
        {
            botonPresionado = true;
            direccionCorte = direccionApuntado;
        }
    }

    private void CortarZombis()
    {
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDisparo.position, radioDeCorte, capaEnemigos);

        foreach (Collider2D colision in enemigosGolpeados)
        {
            IReceptorDano[] receptores = colision.GetComponentsInParent<IReceptorDano>();
            Vector2 direccionEmpuje = (colision.transform.position - transform.position).normalized;

            foreach (IReceptorDano receptor in receptores)
            {
                float danoReal = danoPorSegundo * Time.deltaTime;
                receptor.RecibirDano(danoReal, direccionEmpuje, fuerzaEmpuje);
            }
        }
    }

    private void RomperMotosierra()
    {
        Debug.Log("¡La motosierra se quedó sin gasolina y se rompió!");

        if (sonidoRotura != null)
        {
            AudioSource.PlayClipAtPoint(sonidoRotura, transform.position);
        }

        if (miInventario != null)
        {
            for (int i = 0; i < miInventario.ranuras.Length; i++)
            {
                if (miInventario.ranuras[i] == this)
                {
                    miInventario.ranuras[i] = null;
                    break;
                }
            }
            miInventario.CambiarSlot(1);
        }

        Destroy(gameObject);
    }

    public override void IntentarEmpujon(Vector2 direccion) { }

    private void OnDrawGizmosSelected()
    {
        if (puntoDisparo == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoDisparo.position, radioDeCorte);
    }
}