using UnityEngine;
using UnityEngine.UI;

public class UIInventario : MonoBehaviour
{
    [Header("Iconos del Inventario")]
    [Tooltip("Arrastra aquí las 5 imágenes en orden (0 al 4)")]
    public Image[] iconosSlots = new Image[5];

    [Header("Configuración de Colores")]
    public Color colorActivo = Color.white; // Brillante y 100% visible
    public Color colorInactivo = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gris oscuro y semitransparente

    // Esta función la llamará el jugador cada vez que cambie de arma
    public void IluminarSlot(int indiceActivo)
    {
        for (int i = 0; i < iconosSlots.Length; i++)
        {
            if (iconosSlots[i] != null)
            {
                // Si es el índice que tenemos en la mano, lo pintamos brillante. Si no, lo opacamos.
                iconosSlots[i].color = (i == indiceActivo) ? colorActivo : colorInactivo;
            }
        }
    }
}