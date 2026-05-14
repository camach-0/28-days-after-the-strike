using UnityEngine;

// Clase abstracta: No se puede poner directamente en un objeto, sirve de molde.
public abstract class ControladorArma : MonoBehaviour
{
    [Header("Referencias Base")]
    public Transform puntoDisparo;

    protected float tiempoProximoAtaque = 0f;

    // Obligamos a todos los hijos (armas de fuego, melee, etc) a tener este método
    public abstract void IntentarAtaque(Vector2 direccion);
}