using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObstaculoValla : MonoBehaviour
{
    [Header("Configuración del Salto")]
    [Tooltip("Tiempo en segundos que tarda el jugador en cruzar")]
    public float duracionSalto = 0.4f;

    [Tooltip("Punto de aterrizaje Lado A (Hijo de la valla)")]
    public Transform puntoA;
    [Tooltip("Punto de aterrizaje Lado B (Hijo de la valla)")]
    public Transform puntoB;

    [Header("Impacto en Infectados")]
    [Tooltip("30% = 0.3f. Qué tan lento irá el zombi al cruzar.")]
    [Range(0.1f, 0.9f)]
    public float multiplicadorVelocidadZombi = 0.3f;

    private void OnTriggerStay2D(Collider2D colision)
    {
        if (colision.CompareTag("Player"))
        {
            JugadorInput input = colision.GetComponent<JugadorInput>();

            if (input != null && input.IntentoSalto)
            {
                JugadorController controlador = colision.GetComponent<JugadorController>();
                if (controlador != null && !controlador.estaSaltando)
                {
                    // Lógica para saltar SIEMPRE al lado opuesto del que estás parado
                    float distanciaA = Vector2.Distance(colision.transform.position, puntoA.position);
                    float distanciaB = Vector2.Distance(colision.transform.position, puntoB.position);

                    Vector3 destinoFinal = (distanciaA > distanciaB) ? puntoA.position : puntoB.position;

                    controlador.IniciarSaltoValla(destinoFinal, duracionSalto);
                    Debug.Log("<color=cyan>VALLA:</color> Jugador inició salto (I-Frames activos).");
                }

                input.IntentoSalto = false; // Consumir input
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        if (colision.CompareTag("Enemy"))
        {
            ZombiController zombi = colision.GetComponent<ZombiController>();
            if (zombi != null)
            {
                zombi.Ralentizar(multiplicadorVelocidadZombi);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D colision)
    {
        if (colision.CompareTag("Enemy"))
        {
            ZombiController zombi = colision.GetComponent<ZombiController>();
            if (zombi != null)
            {
                zombi.RestaurarVelocidad();
            }
        }
    }
}