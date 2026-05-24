using UnityEngine;

public abstract class DatosArma : ScriptableObject
{
    [Header("Información General")]
    public string nombreArma;
    public Sprite iconoArma;
    public float danoBase;
    public float cadenciaAtaque;
    public AudioClip sonidoAtaque;

    [Header("Sistema de Botín (Drop)")]
    [Tooltip("El nombre exacto en el PoolManager para la versión de SUELO de esta arma. Ej: ItemPistola")]
    public string etiquetaPoolSuelo;
}
