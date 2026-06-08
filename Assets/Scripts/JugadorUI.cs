using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SistemaSalud))]
public class JugadorUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    public Image barraDeVidaUI; // La roja (Vida real)

    [Tooltip("Duplica tu barra roja, píntala naranja/amarilla y ponla AQUÍ")]
    public Image barraVidaTemporalUI; // La naranja (Vida temporal)

    private SistemaSalud moduloSalud;

    private void Awake()
    {
        moduloSalud = GetComponent<SistemaSalud>();

        moduloSalud.OnVidaCambiada += ActualizarBarraReal;
        moduloSalud.OnVidaTemporalCambiada += ActualizarBarraTemporal;
    }

    private void Start()
    {
        ActualizarBarraReal(1f);
        ActualizarBarraTemporal(0f);
    }

    private void OnDestroy()
    {
        if (moduloSalud != null)
        {
            moduloSalud.OnVidaCambiada -= ActualizarBarraReal;
            moduloSalud.OnVidaTemporalCambiada -= ActualizarBarraTemporal;
        }
    }

    private void ActualizarBarraReal(float porcentaje)
    {
        if (barraDeVidaUI != null) barraDeVidaUI.fillAmount = porcentaje;
        RefrescarFondoTemporal();
    }

    private void ActualizarBarraTemporal(float porcentajeTemp)
    {
        RefrescarFondoTemporal();
    }

    // La barra temporal se pone DETRÁS de la roja, así que sumamos ambos porcentajes.
    // Ejemplo: 20% roja + 30% temporal = La barra temporal se llena al 50%.
    private void RefrescarFondoTemporal()
    {
        if (barraVidaTemporalUI != null && moduloSalud != null)
        {
            float total = (moduloSalud.vidaActual + moduloSalud.vidaTemporal) / moduloSalud.vidaMaxima;
            barraVidaTemporalUI.fillAmount = total;
        }
    }
}