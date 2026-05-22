using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SistemaSalud))]
public class JugadorUI : MonoBehaviour
{
    [Header("Referencias de UI (Arrastra la imagen desde tu Canvas hijo)")]
    public Image barraDeVidaUI;

    private SistemaSalud moduloSalud;

    private void Awake()
    {
        moduloSalud = GetComponent<SistemaSalud>();

        // Nos suscribimos al evento: cuando la vida cambie, ejecutamos nuestra función
        moduloSalud.OnVidaCambiada += ActualizarBarraUI;
    }

    private void Start()
    {
        ActualizarBarraUI(1f); // Inicializamos la barra llena
    }

    private void OnDestroy()
    {
        if (moduloSalud != null) moduloSalud.OnVidaCambiada -= ActualizarBarraUI;
    }

    private void ActualizarBarraUI(float porcentaje)
    {
        if (barraDeVidaUI != null)
        {
            barraDeVidaUI.fillAmount = porcentaje;
        }
    }
}