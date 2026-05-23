using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EfectoBilis : MonoBehaviour
{
    [Header("Interfaz de Ceguera")]
    [Tooltip("Arrastra aquí la imagen verde de tu Canvas")]
    public Image imagenBilisUI;
    public float duracionEfecto = 5f;

    private Coroutine corrutinaActual;

    private void Start()
    {
        if (imagenBilisUI != null)
        {
            // Nos aseguramos de que empiece 100% transparente
            Color c = imagenBilisUI.color;
            c.a = 0f;
            imagenBilisUI.color = c;
        }
    }

    public void RecibirVomito()
    {
        if (imagenBilisUI == null) return;

        // Si ya estábamos manchados y nos vuelven a vomitar, reiniciamos el tiempo
        if (corrutinaActual != null) StopCoroutine(corrutinaActual);
        corrutinaActual = StartCoroutine(RutinaCeguera());

        Debug.Log($"¡{gameObject.name} está cubierto de bilis! Preparando para llamar horda...");

        // TODO: En el siguiente paso, aquí le avisaremos al GameDirector
        if (GameDirector.Instancia != null)
        {
            GameDirector.Instancia.DesatarHordaPorVomito(this.transform);
        }
    }

    private IEnumerator RutinaCeguera()
    {
        // 1. La pantalla se mancha de golpe
        Color colorBilis = imagenBilisUI.color;
        colorBilis.a = 0.85f; // 85% de opacidad (casi no deja ver, pero no es negro total)
        imagenBilisUI.color = colorBilis;

        // 2. Esperamos casi todo el tiempo del efecto
        yield return new WaitForSeconds(duracionEfecto - 2f);

        // 3. La bilis va escurriendo (Fade out de 2 segundos)
        float tiempoFade = 2f;
        float tiempoPasado = 0f;

        while (tiempoPasado < tiempoFade)
        {
            tiempoPasado += Time.deltaTime;
            // Lerp baja el valor progresivamente de 0.85 a 0
            colorBilis.a = Mathf.Lerp(0.85f, 0f, tiempoPasado / tiempoFade);
            imagenBilisUI.color = colorBilis;
            yield return null;
        }

        // 4. Limpiamos por completo
        colorBilis.a = 0f;
        imagenBilisUI.color = colorBilis;
    }
}