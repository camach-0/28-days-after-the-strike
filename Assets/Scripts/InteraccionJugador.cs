using UnityEngine;

[RequireComponent(typeof(InventarioJugador))]
public class InteraccionJugador : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto PivoteArma de tu jerarquía")]
    public Transform pivoteArma;

    private InventarioJugador inventario;
    private ItemRecogible itemCercano;

    private void Awake()
    {
        inventario = GetComponent<InventarioJugador>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Recogible"))
        {
            itemCercano = collision.GetComponent<ItemRecogible>();
            Debug.Log("¡Objeto cerca! Presiona Interactuar para recogerlo.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Recogible"))
        {
            ItemRecogible itemSalida = collision.GetComponent<ItemRecogible>();
            if (itemCercano == itemSalida)
            {
                itemCercano = null;
            }
        }
    }

    public void IntentarRecoger()
    {
        if (itemCercano != null)
        {
            // Sistema universal de Jorge sin interrupciones
            itemCercano.SerRecogido(inventario, pivoteArma);
            itemCercano = null;
        }
    }
}