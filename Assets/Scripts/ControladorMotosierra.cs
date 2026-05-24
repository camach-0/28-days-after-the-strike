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
    public float fuerzaEmpuje = 1f; // Empuje leve pero constante para trabar a los zombis
    public LayerMask capaEnemigos;

    [Header("Sistema de Botín")]
    public string etiquetaSuelo = "Pickup_Motosierra";

    private float gasolinaActual;
    private bool botonPresionado = false;
    private InventarioJugador miInventario;
    private Vector2 direccionCorte = Vector2.right;

    // Cumplimos el contrato de la clase abstracta
    public override string EtiquetaPoolSuelo => etiquetaSuelo;

    // La motosierra pesa mucho, frena un poco al jugador
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
        // Si el jugador mantiene el gatillo apretado y tenemos gasolina
        if (botonPresionado && gasolinaActual > 0)
        {
            CortarZombis();

            // Drenamos la gasolina basada en el tiempo real
            gasolinaActual -= consumoPorSegundo * Time.deltaTime;

            if (gasolinaActual <= 0)
            {
                RomperMotosierra();
            }
        }

        // Reseteamos el estado. Si el jugador sigue presionando el clic, 
        // el JugadorCombate lo volverá a poner en 'true' en el siguiente fotograma.
        botonPresionado = false;
    }

    // El Cerebro (JugadorCombate) llama a esta función cuando presionas clic
    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (gasolinaActual > 0)
        {
            botonPresionado = true;
            direccionCorte = direccionApuntado; // <-- ¡GUARDAMOS EL APUNTADO!
        }
    }

    private void CortarZombis()
    {
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDisparo.position, radioDeCorte, capaEnemigos);

        foreach (Collider2D colision in enemigosGolpeados)
        {
            IReceptorDano[] receptores = colision.GetComponentsInParent<IReceptorDano>();

            // =========================================================
            // ¡LA MATEMÁTICA INFALIBLE! 
            // Dirección = (Posición del Zombi) menos (Posición del Jugador)
            // Esto garantiza que el zombi SIEMPRE salga empujado hacia atrás,
            // alejándose de ti de forma radial.
            // =========================================================
            Vector2 direccionEmpuje = (colision.transform.position - transform.position).normalized;

            foreach (IReceptorDano receptor in receptores)
            {
                float danoReal = danoPorSegundo * Time.deltaTime;

                // Le pasamos esta nueva dirección perfecta
                receptor.RecibirDano(danoReal, direccionEmpuje, fuerzaEmpuje);
            }
        }
    }

    private void RomperMotosierra()
    {
        Debug.Log("¡La motosierra se quedó sin gasolina y se rompió!");

        if (miInventario != null)
        {
            // Vaciamos la ranura principal (Slot 0)
            for (int i = 0; i < miInventario.ranuras.Length; i++)
            {
                if (miInventario.ranuras[i] == this)
                {
                    miInventario.ranuras[i] = null;
                    break;
                }
            }

            // Obligamos al jugador a sacar su arma secundaria (La pistola)
            miInventario.CambiarSlot(1);
        }

        // Destruimos el objeto visual de la mano
        Destroy(gameObject);
    }

    // Obligatorio por herencia, pero puedes dejarlo vacío o darle un culatazo extra
    public override void IntentarEmpujon(Vector2 direccion) { }

    private void OnDrawGizmosSelected()
    {
        if (puntoDisparo == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoDisparo.position, radioDeCorte);
    }
}