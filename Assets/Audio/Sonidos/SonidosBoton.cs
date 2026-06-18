using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class SonidosBoton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip sonidoHover; // Para cuando el ratón pasa por encima
    public AudioClip sonidoClick; // Para cuando haces clic

    private AudioSource fuenteAudio;

    void Awake()
    {
        fuenteAudio = GetComponent<AudioSource>();
        fuenteAudio.playOnAwake = false; // Evita que suenen todos de golpe al iniciar
    }

    // Se activa cuando el ratón entra en el botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (sonidoHover != null) fuenteAudio.PlayOneShot(sonidoHover);
    }

    // Se activa cuando haces clic en el botón
    public void OnPointerClick(PointerEventData eventData)
    {
        if (sonidoClick != null) fuenteAudio.PlayOneShot(sonidoClick);
    }
}