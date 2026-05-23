using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(JugadorMovimiento), typeof(JugadorCombate), typeof(JugadorInput))]
[RequireComponent(typeof(SistemaSalud), typeof(JugadorUI))]
[RequireComponent(typeof(InventarioJugador), typeof(InteraccionJugador))]
public class JugadorController : MonoBehaviour
{
    [Header("Módulos (Músculos y Sentidos)")]
    private JugadorMovimiento moduloMovimiento;
    private JugadorCombate moduloCombate;
    private JugadorInput moduloInput;
    private InventarioJugador moduloInventario;
    private InteraccionJugador moduloInteraccion;

    public SistemaSalud moduloSalud;

    [Header("Estadísticas Base")]
    public float velocidadMovimiento = 5f;

    [Header("¡OBLIGATORIO: Configuración de Cámara!")]
    public Camera camaraPrincipal;

    [Header("Linterna")]
    public GameObject objetoLinterna;

    private bool estaMuerto = false;

    public ControladorArma armaEquipada
    {
        get { return moduloCombate.armaEquipada; }
        set { moduloCombate.armaEquipada = value; }
    }

    private void Awake()
    {
        moduloMovimiento = GetComponent<JugadorMovimiento>();
        moduloCombate = GetComponent<JugadorCombate>();
        moduloInput = GetComponent<JugadorInput>();
        moduloSalud = GetComponent<SistemaSalud>();
        moduloInventario = GetComponent<InventarioJugador>();
        moduloInteraccion = GetComponent<InteraccionJugador>();

        if (camaraPrincipal == null) camaraPrincipal = Camera.main;

        if (moduloSalud != null) moduloSalud.OnMuerte += ProcesarMuerte;
    }

    private void OnDestroy()
    {
        if (moduloSalud != null) moduloSalud.OnMuerte -= ProcesarMuerte;
    }

    public void Start()
    {
        moduloInput.camaraPrincipal = this.camaraPrincipal;
        moduloInput.pivoteArma = moduloMovimiento.pivoteArma;
    }

    private void Update()
    {
        if (estaMuerto)
        {
            moduloMovimiento.Mover(Vector2.zero, 0f);
            moduloCombate.ProcesarInputDisparo(false, moduloInput.DireccionMirando);
            return;
        }

        moduloInput.ProcesarApuntadoRaton();

        // ====================================================================
        // --- ¡NUEVO! CÁLCULO DE PESO DEL ARMA EN LA VELOCIDAD ---
        // ====================================================================
        float velocidadFinal = velocidadMovimiento;
        if (armaEquipada != null) velocidadFinal *= armaEquipada.ModificadorVelocidad;

        moduloMovimiento.Mover(moduloInput.InputMovimiento, velocidadFinal);
        moduloMovimiento.Apuntar(moduloInput.DireccionMirando);

        moduloCombate.ProcesarInputDisparo(moduloInput.EstaDisparando, moduloInput.DireccionMirando);
        moduloCombate.ProcesarDisparoContinuo(moduloInput.DireccionMirando);

        if (moduloInput.IntentoRecargar)
        {
            moduloCombate.IntentarRecarga();
            moduloInput.IntentoRecargar = false;
        }

        if (moduloInput.IntentoLinterna)
        {
            if (objetoLinterna != null) objetoLinterna.SetActive(!objetoLinterna.activeSelf);
            moduloInput.IntentoLinterna = false;
        }

        // --- ¡NUEVO! DELEGACIÓN DEL CULATAZO ---
        if (moduloInput.IntentoEmpujar)
        {
            moduloCombate.ProcesarInputEmpujon(moduloInput.DireccionMirando);
            moduloInput.IntentoEmpujar = false;
        }

        if (moduloInput.IntentoCambioSlot != -1)
        {
            moduloInventario.CambiarSlot(moduloInput.IntentoCambioSlot);
            moduloInput.IntentoCambioSlot = -1;
        }

        if (moduloInput.IntentoScrollArma != 0)
        {
            moduloInventario.CiclarArma(moduloInput.IntentoScrollArma);
            moduloInput.IntentoScrollArma = 0;
        }

        if (moduloInput.IntentoCambioRapido)
        {
            moduloInventario.EjecutarCambioRapido();
            moduloInput.IntentoCambioRapido = false;
        }

        if (moduloInput.IntentoInteractuar)
        {
            if (moduloInteraccion != null) moduloInteraccion.IntentarRecoger();
            moduloInput.IntentoInteractuar = false;
        }
    }

    private void ProcesarMuerte()
    {
        estaMuerto = true;
        Debug.Log(gameObject.name + " ha muerto.");
        moduloMovimiento.Mover(Vector2.zero, 0f);
        moduloCombate.ProcesarInputDisparo(false, moduloInput.DireccionMirando);
        if (GameManager.Instancia != null) GameManager.Instancia.VerificarEstadoJugadores();
        Camera miCamara = GetComponentInChildren<Camera>();
        if (miCamara != null) miCamara.transform.SetParent(null);
        gameObject.SetActive(false);
    }
}