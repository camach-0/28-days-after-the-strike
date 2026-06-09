
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

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

    [Header("Sistema de Muerte")]
    [Tooltip("El prefab del cuerpo sin vida que quedará en el suelo")]
    public GameObject prefabCadaver; // ¡NUEVO!

    [Header("Linterna")]
    public GameObject objetoLinterna;

    private bool estaMuerto = false;

    // Variables para recuperar la cámara tras resucitar
    private Transform padreOriginalCamara;
    private Vector3 posicionOriginalCamara;

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

        // Guardamos dónde estaba la cámara para devolvérsela si revive
        Camera miCamara = GetComponentInChildren<Camera>();
        if (miCamara != null)
        {
            padreOriginalCamara = miCamara.transform.parent;
            posicionOriginalCamara = miCamara.transform.localPosition;
        }

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

        // --- CREACIÓN DEL CADÁVER ---
        if (prefabCadaver != null)
        {
            GameObject miCadaver = Instantiate(prefabCadaver, transform.position, transform.rotation);
            Cadaver scriptCadaver = miCadaver.GetComponent<Cadaver>();
            if (scriptCadaver != null) scriptCadaver.ConfigurarCadaver(this);
        }

        if (GameManager.Instancia != null) GameManager.Instancia.VerificarEstadoJugadores();

        Camera miCamara = GetComponentInChildren<Camera>();
        if (miCamara != null) miCamara.transform.SetParent(null);

        // --- ¡NUEVO! LIMPIAR INVENTARIO Y DAR PISTOLA ANTES DE MORIR ---
        if (moduloInventario != null) moduloInventario.ResetearInventarioPorMuerte();

        gameObject.SetActive(false);
    }

    public void RevivirDesdeCadaver(Vector3 posicionResurreccion)
    {
        transform.position = posicionResurreccion;
        gameObject.SetActive(true);
        estaMuerto = false;

        if (camaraPrincipal != null && padreOriginalCamara != null)
        {
            camaraPrincipal.transform.SetParent(padreOriginalCamara);
            camaraPrincipal.transform.localPosition = posicionOriginalCamara;
        }

        if (moduloSalud != null) moduloSalud.Revivir();

        Debug.Log($"<color=green>{gameObject.name} ha sido revivido por el Desfibrilador.</color>");
    }
    public void ConfigurarRol(bool esHumano)
    {
        // 1. Buscamos todos los componentes implicados
        PlayerInput pInput = GetComponent<PlayerInput>();
        JugadorInput jInput = GetComponent<JugadorInput>();
        JugadorUI jUI = GetComponent<JugadorUI>();
        InteraccionRescate iRescate = GetComponent<InteraccionRescate>();

        NavMeshAgent agente = GetComponent<NavMeshAgent>();
        AliadoBotController botController = GetComponent<AliadoBotController>();

        if (esHumano)
        {
            // ENCENDEMOS cerebro humano
            if (pInput != null) pInput.enabled = true;
            if (jInput != null) jInput.enabled = true;
            if (jUI != null) jUI.enabled = true;
            if (iRescate != null) iRescate.enabled = true;

            // APAGAMOS cerebro bot
            if (botController != null) botController.enabled = false;
            if (agente != null) agente.enabled = false;

            Debug.Log($"<color=cyan>{gameObject.name} configurado como HUMANO.</color>");
        }
        else
        {
            // APAGAMOS cerebro humano
            if (pInput != null) pInput.enabled = false;
            if (jInput != null) jInput.enabled = false;
            if (jUI != null) jUI.enabled = false;
            if (iRescate != null) iRescate.enabled = false;

            // ENCENDEMOS cerebro bot
            if (agente != null) agente.enabled = true;
            if (botController != null) botController.enabled = true;

            Debug.Log($"<color=yellow>{gameObject.name} configurado como BOT.</color>");
        }
    }
}