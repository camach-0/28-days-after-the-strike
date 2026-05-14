using UnityEngine;

public abstract class DatosArma : ScriptableObject
{
    [Header("Información General")]
    public string nombreArma;
    public Sprite iconoArma;
    public float danoBase;
    public float cadenciaAtaque;
    public AudioClip sonidoAtaque;
}
