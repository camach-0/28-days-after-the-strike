using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioReciclable : MonoBehaviour
{
    [Header("Configuración del Pool")]
    public string etiquetaPool = "EfectoSonido";

    private AudioSource fuenteAudio;

    private void Awake()
    {
        fuenteAudio = GetComponent<AudioSource>();
        // Configuraciones base para que suene en 3D (para que los zombis lejanos suenen lejos)
        fuenteAudio.spatialBlend = 1f;
        fuenteAudio.rolloffMode = AudioRolloffMode.Linear;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    // El arma llamará a esta función pasándole su sonido
    public void Reproducir(AudioClip clip, float volumen = 1f)
    {
        if (clip == null) return;

        fuenteAudio.clip = clip;
        fuenteAudio.volume = volumen;
        fuenteAudio.Play();

        // Empezamos a contar el tiempo que dura el sonido para apagarlo justo al final
        StartCoroutine(DesactivarAlTerminar(clip.length));
    }

    private IEnumerator DesactivarAlTerminar(float duracion)
    {
        yield return new WaitForSeconds(duracion);

        // ¡ADIÓS DESTROY! Devolvemos el altavoz a la piscina
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }
}