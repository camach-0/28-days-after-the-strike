using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuOpciones : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelOpciones;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider sliderVolumenGeneral;
    public Slider sliderMusica;
    public Slider sliderJugador;
    public Slider sliderEfectos;

    [Header("Toggle")]
    public Toggle toggleFullscreen;
    void Start()
    {
        CargarOpciones();
        sliderVolumenGeneral.onValueChanged.AddListener(CambiarVolumenGeneral);
        sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);
        sliderJugador.onValueChanged.AddListener(CambiarVolumenJugador);
        sliderEfectos.onValueChanged.AddListener(CambiarVolumenEfectos);
        toggleFullscreen.onValueChanged.AddListener(CambiarFullscreen);
    }

    public void AbrirOpciones()
    {
        panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        panelOpciones.SetActive(false);
    }
    public void CambiarVolumenGeneral(float valor)
    {
        audioMixer.SetFloat("VolumenGeneral", Mathf.Log10(valor) * 20);

        PlayerPrefs.SetFloat("VolumenGeneral", valor);
        PlayerPrefs.Save();
    }
    public void CambiarVolumenMusica(float valor)
    {
        audioMixer.SetFloat("VolumenMusica", Mathf.Log10(valor) * 20);

        PlayerPrefs.SetFloat("VolumenMusica", valor);
        PlayerPrefs.Save();
    }
    public void CambiarVolumenJugador(float valor)
    {
        audioMixer.SetFloat("VolumenJugador", Mathf.Log10(valor) * 20);

        PlayerPrefs.SetFloat("VolumenJugador", valor);
        PlayerPrefs.Save();
    }
    public void CambiarVolumenEfectos(float valor)
    {
        audioMixer.SetFloat("VolumenEfectos", Mathf.Log10(valor) * 20);

        PlayerPrefs.SetFloat("VolumenEfectos", valor);
        PlayerPrefs.Save();
    }
    public void CambiarFullscreen(bool activar)
    {
        Screen.fullScreen = activar;

        PlayerPrefs.SetInt("Fullscreen", activar ? 1 : 0);
        PlayerPrefs.Save();
    }
    void CargarOpciones()
    {
        float volumenGeneral = PlayerPrefs.GetFloat("VolumenGeneral", 1f);
        sliderVolumenGeneral.value = volumenGeneral;
        audioMixer.SetFloat("VolumenGeneral", Mathf.Log10(volumenGeneral) * 20);

        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        sliderMusica.value = volumenMusica;
        audioMixer.SetFloat("VolumenMusica", Mathf.Log10(volumenMusica) * 20);

        float volumenJugador = PlayerPrefs.GetFloat("VolumenJugador", 1f);
        sliderJugador.value = volumenJugador;
        audioMixer.SetFloat("VolumenJugador", Mathf.Log10(volumenJugador) * 20);

        float volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
        sliderEfectos.value = volumenEfectos;
        audioMixer.SetFloat("VolumenEfectos", Mathf.Log10(volumenEfectos) * 20);

        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        toggleFullscreen.isOn = fullscreen;
        Screen.fullScreen = fullscreen;
    }
    public void IrAlLobby()
    { 
        SceneManager.LoadScene("Escena_2_Lobby"); 
    }
    public void SalirDelJuego()
    {
        Debug.Log("Cerrando juego...");
        Application.Quit();
    }
}