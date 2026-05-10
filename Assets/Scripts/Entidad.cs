using System.Collections; 
using UnityEngine;
using UnityEngine.UI;

public class Entidad : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float velocidadMovimiento = 5f;
    public bool estaMuerto = false;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
    private Coroutine rutinaParpadeo;

    [Header("Interfaz Visual")]
    public Image barraDeVidaUI;

    public virtual void Start()
    {
        vidaActual = vidaMaxima;
        ActualizarBarraDeVida();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            colorOriginal = spriteRenderer.color;
        }
    }

    public virtual void RecibirDano(float cantidadDano)
    {
        if (estaMuerto) return;

        vidaActual -= cantidadDano;
        ActualizarBarraDeVida();

        // 2. Activamos el parpadeo seguro (borramos la línea duplicada que tenías antes)
        if (spriteRenderer != null)
        {
            // Si ya estaba parpadeando, DETENEMOS el cronómetro viejo
            if (rutinaParpadeo != null)
            {
                StopCoroutine(rutinaParpadeo);
            }
            // Iniciamos uno nuevo y lo guardamos en nuestra variable
            rutinaParpadeo = StartCoroutine(EfectoDano());
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void ActualizarBarraDeVida()
    {
        if (barraDeVidaUI != null)
        {
            barraDeVidaUI.fillAmount = vidaActual / vidaMaxima;
        }
    }

    private IEnumerator EfectoDano()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = colorOriginal;
    }

    public virtual void Morir()
    {
        estaMuerto = true;

        // ¡ESTE ES EL CULPABLE!
        if (gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
        Debug.Log(gameObject.name + " ha muerto.");
    }
}