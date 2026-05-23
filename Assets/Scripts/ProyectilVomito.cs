using System.Collections;
using UnityEngine;

public class ProyectilVomito : MonoBehaviour
{
    [Tooltip("Debe ser exactamente igual al del PoolManager")]
    public string etiquetaPool = "VomitoBoomer";
    public float tiempoDeVida = 0.5f; // El vómito es rápido, solo dura medio segundo

    private void OnEnable()
    {
        // En cuanto el Boomer lo escupe, empieza la cuenta regresiva para desaparecer
        StartCoroutine(DesactivarTrasTiempo());
    }

    private IEnumerator DesactivarTrasTiempo()
    {
        yield return new WaitForSeconds(tiempoDeVida);
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo nos interesa si golpea a un superviviente real
        if (collision.CompareTag("Player"))
        {
            SistemaSalud salud = collision.GetComponent<SistemaSalud>();

            // Validamos que sea un humano y que esté vivo
            if (salud != null && salud.esSuperviviente && salud.vidaActual > 0)
            {
                Debug.Log($"<color=green>¡El Boomer manchó a {collision.gameObject.name}!</color>");

                // --- NUEVO: Buscamos el efecto y lo activamos ---
                EfectoBilis bilis = collision.GetComponent<EfectoBilis>();
                if (bilis != null)
                {
                    bilis.RecibirVomito();
                }
            }
        }
    }
}