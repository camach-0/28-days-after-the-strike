using UnityEngine;

public class ClimaController : MonoBehaviour
{
    [Header("Asignar el ParticleSystem de lluvia")]
    public ParticleSystem lluvia;

    [Header("Duración en segundos antes de detener la lluvia")]
    public float duracionLluvia = 40f;

    void Start()
    {
        // Siempre inicia lloviendo
        if (lluvia != null)
        {
            lluvia.Play();
            // Después de X segundos se detiene
            Invoke(nameof(DetenerLluvia), duracionLluvia);
        }
        else
        {
            Debug.LogWarning("No se asignó el ParticleSystem de lluvia en el inspector.");
        }
    }

    void DetenerLluvia()
    {
        if (lluvia != null)
        {
            lluvia.Stop();
        }
    }
}
