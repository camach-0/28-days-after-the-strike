using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TriggerTank : MonoBehaviour
{
    [Header("Configuración de Aparición")]
    [Tooltip("El punto exacto donde nacerá el Tank (Arrastra aquí un objeto vacío)")]
    public Transform puntoDeAparicion;

    private bool yaActivado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si lo que pisó el trigger es un jugador
        if (!yaActivado && collision.CompareTag("Player"))
        {
            yaActivado = true; // Lo bloqueamos para que no invoque 50 Tanks si pasan varios jugadores

            if (GameDirector.Instancia != null && puntoDeAparicion != null)
            {
                // ¡Llamamos a la función que creamos en el Director!
                GameDirector.Instancia.GenerarTank(puntoDeAparicion.position);
            }
            else
            {
                Debug.LogWarning("Falta asignar el Punto de Aparición en el Trigger del Tank.");
            }
        }
    }

    // Esto dibuja una línea roja en el editor para que veas dónde va a aparecer
    private void OnDrawGizmos()
    {
        if (puntoDeAparicion != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, puntoDeAparicion.position);
            Gizmos.DrawWireCube(puntoDeAparicion.position, new Vector3(1, 1, 0));
        }
    }
}