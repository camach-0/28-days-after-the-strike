using UnityEngine;

[RequireComponent(typeof(SistemaSalud))]
public class MuerteBoomer : MonoBehaviour
{
    [Header("Configuración de la Explosión")]
    public string etiquetaExplosion = "ExplosionBoomer";
    public float radioExplosion = 4f;
    public float fuerzaEmpuje = 12f;
    public float danoJugadores = 5f;

    [Header("Capas Afectadas")]
    public LayerMask capaJugadores; // Player
    public LayerMask capaZombis;    // ZombiBase, ZombiEspecial

    private SistemaSalud miSalud;

    private void Awake()
    {
        miSalud = GetComponent<SistemaSalud>();
    }

    private void OnEnable()
    {
        if (miSalud != null) miSalud.OnMuerte += Detonar;
    }

    private void OnDisable()
    {
        if (miSalud != null) miSalud.OnMuerte -= Detonar;
    }

    private void Detonar()
    {
        // 1. Instanciar el efecto visual/sonoro
        if (PoolManager.Instancia != null)
        {
            PoolManager.Instancia.SolicitarObjeto(etiquetaExplosion, transform.position, Quaternion.identity);
        }

        // ¡CORRECCIÓN! Sumamos las máscaras usando su valor real (.value)
        int todasLasCapas = capaJugadores.value | capaZombis.value;
        Collider2D[] afectados = Physics2D.OverlapCircleAll(transform.position, radioExplosion, todasLasCapas);

        foreach (Collider2D col in afectados)
        {
            IReceptorDano receptor = col.GetComponent<IReceptorDano>();
            if (receptor != null)
            {
                Vector2 direccionEmpuje = (col.transform.position - transform.position).normalized;

                // Calculamos el valor en bits de la capa del objeto golpeado
                int capaObjeto = 1 << col.gameObject.layer;

                // --- SI ES UN JUGADOR ---
                if ((capaObjeto & capaJugadores.value) != 0)
                {
                    // Lo empujamos y le hacemos el daño
                    receptor.RecibirDano(danoJugadores, direccionEmpuje, fuerzaEmpuje);

                    // Activamos la bilis
                    EfectoBilis bilis = col.gameObject.GetComponent<EfectoBilis>();
                    if (bilis != null)
                    {
                        bilis.RecibirVomito();
                    }
                }
                // --- SI ES UN ZOMBI ---
                else if ((capaObjeto & capaZombis.value) != 0)
                {
                    // Solo lo empujamos, sin daño
                    receptor.RecibirDano(0f, direccionEmpuje, fuerzaEmpuje);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}