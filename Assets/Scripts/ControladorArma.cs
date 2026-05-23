using UnityEngine;

// Clase abstracta: No se puede poner directamente en un objeto, sirve de molde.
public abstract class ControladorArma : MonoBehaviour
{
    [Header("Referencias Base")]
    public Transform puntoDisparo;

    protected float tiempoProximoAtaque = 0f;

    // ¡NUEVO! Propiedad que el jugador leerá para saber si debe caminar más lento
    public virtual float ModificadorVelocidad { get { return 1f; } }

    public abstract string EtiquetaPoolSuelo { get; }

    // Obligamos a todos los hijos (armas de fuego, melee, etc) a tener este método
    public abstract void IntentarAtaque(Vector2 direccion);

    // ¡NUEVO! Obligamos a todas las armas a tener un botón de empujar (Clic Derecho)
    public abstract void IntentarEmpujon(Vector2 direccion);
}