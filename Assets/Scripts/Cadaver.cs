using UnityEngine;

public class Cadaver : MonoBehaviour
{
    private JugadorController jugadorOriginal;

    // El jugador, antes de desaparecer, le pasa su alma a este cadáver
    public void ConfigurarCadaver(JugadorController jugador)
    {
        jugadorOriginal = jugador;
    }

    // El desfibrilador llama a esta función
    public void AplicarDesfibrilador()
    {
        if (jugadorOriginal != null)
        {
            // Revivir al jugador exactamente donde está este cadáver
            jugadorOriginal.RevivirDesdeCadaver(transform.position);
        }

        // Destruimos el cadáver de utilería del suelo
        Destroy(gameObject);
    }
}