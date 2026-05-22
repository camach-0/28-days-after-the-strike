using UnityEngine;
using UnityEngine.UI;

public class UIInventario : MonoBehaviour
{
    [Header("Iconos del Inventario")]
    public Image[] iconosSlots = new Image[5];

    [Header("Configuración de Colores")]
    public Color colorActivo = Color.white;
    public Color colorInactivo = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private InventarioJugador inventarioJugador;

    private void Awake()
    {
        // Busca al jugador padre para conectarse a su inventario
        inventarioJugador = GetComponentInParent<InventarioJugador>();

        if (inventarioJugador != null)
        {
            // Nos suscribimos al evento del inventario
            inventarioJugador.OnArmaCambiada += IluminarSlot;
        }
    }

    private void OnDestroy()
    {
        // Buena práctica: Desuscribirse al morir para evitar errores
        if (inventarioJugador != null)
        {
            inventarioJugador.OnArmaCambiada -= IluminarSlot;
        }
    }

    public void IluminarSlot(int indiceActivo)
    {
        for (int i = 0; i < iconosSlots.Length; i++)
        {
            if (iconosSlots[i] != null)
            {
                iconosSlots[i].color = (i == indiceActivo) ? colorActivo : colorInactivo;
            }
        }
    }
}