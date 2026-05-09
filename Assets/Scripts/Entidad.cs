using UnityEngine;
using UnityEngine.UI; // OBLIGATORIO AÑADIR ESTO PARA USAR UI

public class Entidad : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float velocidadMovimiento = 5f; // ¡AQUÍ ESTÁ LA VARIABLE QUE SE NOS BORRÓ!
    public bool estaMuerto = false;

    [Header("Interfaz Visual")]
    public Image barraDeVidaUI;

    public virtual void Start()
    {
        vidaActual = vidaMaxima;
        ActualizarBarraDeVida();
    }

    public virtual void RecibirDano(float cantidadDano)
    {
        if (estaMuerto) return;

        vidaActual -= cantidadDano;

        ActualizarBarraDeVida();

        StartCoroutine(EfectoDano());

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

    private System.Collections.IEnumerator EfectoDano()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color colorOriginal = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = colorOriginal;
        }
    }

    public virtual void Morir()
    {
        estaMuerto = true;
        Debug.Log(gameObject.name + " ha muerto.");
        Destroy(gameObject);
    }
}