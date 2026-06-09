using UnityEngine;
using UnityEngine.AI;

public class Cadaver : MonoBehaviour
{
    private JugadorController jugadorOriginal;

    public void ConfigurarCadaver(JugadorController jugador)
    {
        jugadorOriginal = jugador;
    }

    public void AplicarDesfibrilador()
    {
        if (jugadorOriginal != null)
        {
            jugadorOriginal.RevivirDesdeCadaver(transform.position);

            // ¡NUEVA PROTECCIÓN! Solo reactivamos el NavMesh y la IA si NO es un humano
            JugadorInput inputHumano = jugadorOriginal.GetComponent<JugadorInput>();

            if (inputHumano == null || !inputHumano.enabled)
            {
                NavMeshAgent agente = jugadorOriginal.GetComponent<NavMeshAgent>();
                if (agente != null) agente.enabled = true;

                AliadoBotController botController = jugadorOriginal.GetComponent<AliadoBotController>();
                if (botController != null)
                {
                    botController.enabled = true;
                    Debug.Log("<color=green>Cerebro del Bot reactivado tras desfibrilador.</color>");
                }
            }
        }

        Destroy(gameObject);
    }
}