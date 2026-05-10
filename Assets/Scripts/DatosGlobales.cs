using UnityEngine;

// Al no heredar de MonoBehaviour, esta clase es puramente de datos.
// Al ser "static", su memoria sobrevive a los cambios de escena.
public static class DatosGlobales
{
    public static int cantidadJugadoresHumanos = 1; // Por defecto asumimos que juega 1 persona

    // Más adelante aquí guardaremos quién eligió a qué personaje (ej. si el P1 eligió al de rojo o al de azul)
}