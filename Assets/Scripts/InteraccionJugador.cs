using UnityEngine;
using UnityEngine.InputSystem;

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
            Debug.Log("¡Arma cerca! Presiona Interactuar para recogerla.");
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

    // Se ejecuta al apretar el botón de Interactuar
    public void OnInteractuar(InputValue valor)
    {
        if (valor.isPressed && itemCercano != null)
        {
            itemCercano.SerRecogido(inventario, pivoteArma);
            itemCercano = null; // Vaciamos la referencia tras recogerlo
        }
    }
}