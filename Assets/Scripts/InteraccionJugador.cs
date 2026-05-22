using UnityEngine;

[RequireComponent(typeof(InventarioJugador))]
public class InteraccionJugador : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto PivoteArma de tu jerarquía")]
    public Transform pivoteArma;

    private InventarioJugador inventario;
    private ItemRecogible itemCercano; // Guarda el item que tenemos pisando

    private void Awake()
    {
        inventario = GetComponent<InventarioJugador>();
    }

    // Cuando el jugador (su Collider2D) toca el área de un item
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Recogible"))
        {
            itemCercano = collision.GetComponent<ItemRecogible>();
            Debug.Log("¡Objeto cerca! Presiona Interactuar para recogerlo.");
        }
    }

    // Cuando el jugador se aleja del item
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Recogible"))
        {
            ItemRecogible itemSalida = collision.GetComponent<ItemRecogible>();
            if (itemCercano == itemSalida)
            {
                itemCercano = null; // Ya no podemos recogerlo
            }
        }
    }

    // ====================================================================
    // --- ¡NUEVO! Método público para ser invocado por el Cerebro ---
    // ====================================================================
    public void IntentarRecoger()
    {
        // Si el Cerebro da la orden y tenemos algo bajo los pies...
        if (itemCercano != null)
        {
            itemCercano.SerRecogido(inventario, pivoteArma);
            itemCercano = null; // Vaciamos la referencia tras recogerlo
        }
    }
}