using UnityEngine;
using UnityEngine.InputSystem;

public class JugadorInput : MonoBehaviour
{
    [HideInInspector] public Camera camaraPrincipal;
    [HideInInspector] public Transform pivoteArma;

    public Vector2 InputMovimiento { get; private set; }
    public Vector2 DireccionMirando { get; private set; } = Vector2.right;
    public bool EstaDisparando { get; private set; }

    public bool IntentoRecargar { get; set; }
    public bool IntentoLinterna { get; set; }
    public bool IntentoInteractuar { get; set; }
    public bool IntentoEmpujar { get; set; } // <-- ¡NUEVO! Clic Derecho / Melee

    public int IntentoCambioSlot { get; set; } = -1;
    public int IntentoScrollArma { get; set; } = 0;
    public bool IntentoCambioRapido { get; set; }

    private bool usandoRaton = true;
    private Vector2 posicionRatonPantalla;

    public void OnMover(InputValue valor)
    {
        Vector2 inputBruto = valor.Get<Vector2>();
        InputMovimiento = (inputBruto.magnitude > 0.15f) ? inputBruto : Vector2.zero;
    }

    public void OnApuntar(InputValue valor)
    {
        Vector2 inputApunte = valor.Get<Vector2>();

        if (inputApunte.sqrMagnitude > 2f)
        {
            usandoRaton = true;
            posicionRatonPantalla = inputApunte;
        }
        else if (inputApunte.sqrMagnitude > 0.01f)
        {
            usandoRaton = false;
            DireccionMirando = inputApunte.normalized;
        }
    }

    public void OnDisparar(InputValue valor) { EstaDisparando = valor.isPressed; }
    public void OnRecargar(InputValue valor) { if (valor.isPressed) IntentoRecargar = true; }
    public void OnLinterna(InputValue valor) { if (valor.isPressed) IntentoLinterna = true; }
    public void OnInteractuar(InputValue valor) { if (valor.isPressed) IntentoInteractuar = true; }

    // --- NUEVO INPUT --- Asegúrate de mapear "Empujar" en tu InputActions
    public void OnEmpujar(InputValue valor) { if (valor.isPressed) IntentoEmpujar = true; }

    public void OnArmaPrincipal(InputValue valor) { if (valor.isPressed) IntentoCambioSlot = 0; }
    public void OnArmaSecundaria(InputValue valor) { if (valor.isPressed) IntentoCambioSlot = 1; }
    public void OnArojadizos(InputValue valor) { if (valor.isPressed) IntentoCambioSlot = 2; }
    public void OnBotiquin(InputValue valor) { if (valor.isPressed) IntentoCambioSlot = 3; }
    public void OnPildoras(InputValue valor) { if (valor.isPressed) IntentoCambioSlot = 4; }

    public void OnRuedaRaton(InputValue valor)
    {
        float scroll = valor.Get<float>();
        if (scroll > 0) IntentoScrollArma = -1;
        else if (scroll < 0) IntentoScrollArma = 1;
    }

    public void OnCambioRapido(InputValue valor) { if (valor.isPressed) IntentoCambioRapido = true; }

    public void ProcesarApuntadoRaton()
    {
        if (usandoRaton && camaraPrincipal != null && pivoteArma != null)
        {
            float distanciaZ = Mathf.Abs(camaraPrincipal.transform.position.z - transform.position.z);
            Vector3 screenPoint = new Vector3(posicionRatonPantalla.x, posicionRatonPantalla.y, distanciaZ);
            Vector3 mouseWorldPosition = camaraPrincipal.ScreenToWorldPoint(screenPoint);

            Vector2 direccionHaciaRaton = new Vector2(
                mouseWorldPosition.x - pivoteArma.position.x,
                mouseWorldPosition.y - pivoteArma.position.y
            );

            if (direccionHaciaRaton.sqrMagnitude > 0.01f)
            {
                DireccionMirando = direccionHaciaRaton.normalized;
            }
        }
    }
}