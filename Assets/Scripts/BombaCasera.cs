using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BombaCasera : MonoBehaviour
{
    [Header("Configuración General")]
    public string etiquetaPool = "BombaCasera";
    public float tiempoParaExplotar = 4f;
    public float radioAtraccionZombis = 15f;
    public float radioExplosion = 4f;
    public float fuerzaEmpuje = 15f;
    public string etiquetaExplosion = "ExplosionBoomer";

    [Header("Identificación de Capas (Layers)")]
    public LayerMask capaComunes;    // ZombiBase 
    public LayerMask capaEspeciales; // ZombiEspecial, Tank 
    public LayerMask capaJugadores;  // Player 

    [Header("Daño por tipo de entidad")]
    public float danoComunes = 1000f;
    public float danoEspeciales = 250f;
    public float danoJugadores = 20f;

    private void OnEnable()
    {
        StartCoroutine(RutinaBomba());
    }

    private IEnumerator RutinaBomba()
    {
        Debug.Log("<color=yellow>Bomba Casera: ¡Bip... Bip... Bip!</color>");
        AtraerZombisCercanos();
        yield return new WaitForSeconds(tiempoParaExplotar);
        Explotar();
    }

    private void AtraerZombisCercanos()
    {
        // Usamos .value para extraer el número real de la capa
        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, radioAtraccionZombis, capaComunes.value);
        foreach (Collider2D col in cercanos)
        {
            ZombiController zombi = col.GetComponent<ZombiController>();
            if (zombi != null)
            {
                zombi.objetivoJugador = this.transform;
            }
        }
    }

    private void Explotar()
    {
        PoolManager.Instancia.SolicitarObjeto(etiquetaExplosion, transform.position, Quaternion.identity);

        // ¡CORRECCIÓN! Sumamos las máscaras usando su valor real (.value) para que Unity no se confunda
        int todasLasCapas = capaComunes.value | capaEspeciales.value | capaJugadores.value;

        Collider2D[] afectados = Physics2D.OverlapCircleAll(transform.position, radioExplosion, todasLasCapas);
        foreach (Collider2D col in afectados)
        {
            IReceptorDano receptor = col.GetComponent<IReceptorDano>();
            if (receptor != null)
            {
                float danoFinal = 0f;
                // Calculamos el valor en bits de la capa del objeto golpeado
                int capaObjeto = 1 << col.gameObject.layer;

                // Evaluamos las capas de forma 100% precisa
                if ((capaObjeto & capaComunes.value) != 0) danoFinal = danoComunes;
                else if ((capaObjeto & capaEspeciales.value) != 0) danoFinal = danoEspeciales;
                else if ((capaObjeto & capaJugadores.value) != 0) danoFinal = danoJugadores;

                if (danoFinal > 0)
                {
                    Vector2 direccionEmpuje = (col.transform.position - transform.position).normalized;
                    receptor.RecibirDano(danoFinal, direccionEmpuje, fuerzaEmpuje);
                }
            }
        }

        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioAtraccionZombis);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}