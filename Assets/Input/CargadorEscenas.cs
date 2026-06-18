using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CargadorEscenas : MonoBehaviour
{
    [Header("Componentes Visuales")]
    public CanvasGroup grupoCanvas;
    public Slider barraDeCarga;
    public TMP_Text textoPorcentaje;

    [Header("Configuración")]
    public float velocidadFade = 2f;

    [Tooltip("Tiempo mínimo en segundos que la pantalla negra se quedará a la vista")]
    public float tiempoMinimoDeCarga = 2.5f; 

    public void IniciarCarga(string nombreEscena)
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        StartCoroutine(RutinaCargaYFade(nombreEscena));
    }

    private IEnumerator RutinaCargaYFade(string nombreEscena)
    {
        grupoCanvas.blocksRaycasts = true;
        while (grupoCanvas.alpha < 1f)
        {
            grupoCanvas.alpha += Time.deltaTime * velocidadFade;
            yield return null;
        }


        AsyncOperation operacion = SceneManager.LoadSceneAsync(nombreEscena);
        operacion.allowSceneActivation = false;

        float tiempoInicio = Time.time;

        while (!operacion.isDone)
        {
            float progresoReal = Mathf.Clamp01(operacion.progress / 0.9f);
            float tiempoTranscurrido = Time.time - tiempoInicio;
            float progresoTiempo = Mathf.Clamp01(tiempoTranscurrido / tiempoMinimoDeCarga);

            float progresoVisual = Mathf.Min(progresoReal, progresoTiempo);

            if (barraDeCarga != null) barraDeCarga.value = progresoVisual;
            if (textoPorcentaje != null) textoPorcentaje.text = (progresoVisual * 100f).ToString("F0") + "%";

            if (operacion.progress >= 0.9f && tiempoTranscurrido >= tiempoMinimoDeCarga)
            {
                if (barraDeCarga != null) barraDeCarga.value = 1f;
                if (textoPorcentaje != null) textoPorcentaje.text = "100%";

                yield return new WaitForSeconds(0.5f);

                operacion.allowSceneActivation = true;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        while (grupoCanvas.alpha > 0f)
        {
            grupoCanvas.alpha -= Time.deltaTime * velocidadFade;
            yield return null;
        }
        Destroy(gameObject);
    }
}