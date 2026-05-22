using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(JugadorMovimiento), typeof(JugadorCombate), typeof(JugadorInput))]
[RequireComponent(typeof(SistemaSalud), typeof(JugadorUI))]
[RequireComponent(typeof(InventarioJugador))] // <-- ¡NUEVO! Forzamos a que el personaje tenga inventario
public class JugadorController : MonoBehaviour
{
    [Header("Módulos (Músculos y Sentidos)")]
    private JugadorMovimiento moduloMovimiento;
    private JugadorCombate moduloCombate;
    private JugadorInput moduloInput;
    private InventarioJugador moduloInventario; // <-- ¡NUEVO! Conexión con el inventario

    // AQUÍ ESTÁ LA VARIABLE QUE SOLUCIONA EL ERROR:
    public SistemaSalud moduloSalud;

    [Header("Estadísticas Base")]
    public float velocidadMovimiento = 5f;

    [Header("¡OBLIGATORIO: Configuración de Cámara!")]
    public Camera camaraPrincipal;

    [Header("Linterna")]
    public GameObject objetoLinterna;

    private bool estaMuerto = false;

    // El puente del Inventario
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
        moduloInventario = GetComponent<InventarioJugador>(); // <-- ¡NUEVO! Enlazamos el inventario

        if (camaraPrincipal == null) camaraPrincipal = Camera.main;

        if (moduloSalud != null)
        {
            moduloSalud.OnMuerte += ProcesarMuerte;
        }
    }

    private void OnDestroy()
    {
        if (moduloSalud != null)
        {
            moduloSalud.OnMuerte -= ProcesarMuerte;
        }
    }

    public void Start()
    {
        moduloInput.camaraPrincipal = this.camaraPrincipal;
        moduloInput.pivoteArma = moduloMovimiento.pivoteArma;
    }

    // ==========================================
    // EL CEREBRO EN ACCIÓN (Delegación pura)
    // ==========================================
    private void Update()
    {
        if (estaMuerto)
        {
            moduloMovimiento.Mover(Vector2.zero, 0f);
            moduloCombate.ProcesarInputDisparo(false, moduloInput.DireccionMirando);
            return;
        }

        moduloInput.ProcesarApuntadoRaton();

        moduloMovimiento.Mover(moduloInput.InputMovimiento, velocidadMovimiento);
        moduloMovimiento.Apuntar(moduloInput.DireccionMirando);

        moduloCombate.ProcesarInputDisparo(moduloInput.EstaDisparando, moduloInput.DireccionMirando);
        moduloCombate.ProcesarDisparoContinuo(moduloInput.DireccionMirando);

        // --- ACCIONES DE UN SOLO TOQUE (Delegación) ---
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

        // ====================================================================
        // --- ¡NUEVO! GESTIÓN CENTRALIZADA DEL INVENTARIO (Fase 2) ---
        // ====================================================================

        // 1. Cambio de ranura directo (Teclas 1 a 5)
        if (moduloInput.IntentoCambioSlot != -1)
        {
            moduloInventario.CambiarSlot(moduloInput.IntentoCambioSlot);
            moduloInput.IntentoCambioSlot = -1; // Consumimos la orden
        }

        // 2. Cambio por rueda de ratón (Scroll)
        if (moduloInput.IntentoScrollArma != 0)
        {
            moduloInventario.CiclarArma(moduloInput.IntentoScrollArma);
            moduloInput.IntentoScrollArma = 0; // Consumimos la orden
        }

        // 3. Botón de cambio rápido (Mando / Triángulo)
        if (moduloInput.IntentoCambioRapido)
        {
            moduloInventario.EjecutarCambioRapido();
            moduloInput.IntentoCambioRapido = false; // Consumimos la orden
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