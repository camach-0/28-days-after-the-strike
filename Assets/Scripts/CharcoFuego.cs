using System.Collections;
using UnityEngine;

public class CharcoFuego : MonoBehaviour
{
    [Header("Configuración General")]
    public string etiquetaPool = "FuegoMolotov";
    public float tiempoDuracion = 15f;
    public float radioFuego = 3f;

    [Header("Capas (Layers)")]
    public LayerMask capaComunes; // ZombiBase
    public LayerMask capaEspeciales; // ZombiEspecial, Tank, Witch
    public LayerMask capaJugadores; // Player

    [Header("Daños base")]
    public float danoInstaKill = 1000f; // Comunes
    public float residualJugadores = 5f; // Jugadores se queman un rato y se apagan

    private void OnEnable()
    {
        StartCoroutine(QuemarYApagar());
        StartCoroutine(HacerDanoContinuo());
    }

    private IEnumerator QuemarYApagar()
    {
        yield return new WaitForSeconds(tiempoDuracion);
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }

    private IEnumerator HacerDanoContinuo()
    {
        LayerMask todas = capaComunes | capaEspeciales | capaJugadores;

        while (true)
        {
            Collider2D[] quemados = Physics2D.OverlapCircleAll(transform.position, radioFuego, todas);
            foreach (Collider2D col in quemados)
            {
                IReceptorDano receptor = col.GetComponent<IReceptorDano>();
                if (receptor != null)
                {
                    // 1. ZOMBI COMÚN
                    if (((1 << col.gameObject.layer) & capaComunes) != 0)
                    {
                        receptor.RecibirDano(danoInstaKill, Vector2.zero, 0f);
                    }
                    // 2. ZOMBI ESPECIAL (Busca su sensibilidad)
                    else if (((1 << col.gameObject.layer) & capaEspeciales) != 0)
                    {
                        SensibilidadFuego sensibilidad = col.GetComponent<SensibilidadFuego>();
                        float dps = sensibilidad != null ? sensibilidad.danoPorSegundo : 5f; // 5 por defecto

                        receptor.RecibirDano(dps, Vector2.zero, 0f);
                        AplicarEfecto(col.gameObject, dps, 0f, true);
                    }
                    // 3. JUGADOR
                    else if (((1 << col.gameObject.layer) & capaJugadores) != 0)
                    {
                        receptor.RecibirDano(residualJugadores, Vector2.zero, 0f);
                        AplicarEfecto(col.gameObject, residualJugadores, 4f, false);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void AplicarEfecto(GameObject obj, float dano, float tiempo, bool permanente)
    {
        EfectoQuemadura efecto = obj.GetComponent<EfectoQuemadura>();
        if (efecto == null)
        {
            efecto = obj.AddComponent<EfectoQuemadura>();
            efecto.IniciarQuemadura(dano, tiempo, permanente);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radioFuego);
    }
}