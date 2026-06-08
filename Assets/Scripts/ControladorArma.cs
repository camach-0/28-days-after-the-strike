using UnityEngine;

// Clase abstracta: No se puede poner directamente en un objeto, sirve de molde.
public abstract class ControladorArma : MonoBehaviour
{
    [Header("Referencias Base")]
    public Transform puntoDisparo;

    [Header("Sistema de Botín (Drop)")]
    [Tooltip("El Prefab visual que aparecerá en el suelo cuando tires esta arma")]
    public GameObject prefabSuelo;

    protected float tiempoProximoAtaque = 0f;
    public virtual float ModificadorVelocidad { get { return 1f; } }


    public abstract void IntentarAtaque(Vector2 direccion);

    public abstract void IntentarEmpujon(Vector2 direccion);
}