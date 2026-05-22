using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(JugadorMovimiento), typeof(JugadorCombate), typeof(JugadorInput))]
[RequireComponent(typeof(SistemaSalud), typeof(JugadorUI))] // <-- ¡NUEVO! Ahora Unity pondrá la UI a la fuerza
public class JugadorController : MonoBehaviour
{
    [Header("Módulos (Músculos y Sentidos)")]
    private JugadorMovimiento moduloMovimiento;
    private JugadorCombate moduloCombate;
    private JugadorInput moduloInput;

    // AQUÍ ESTÁ LA VARIABLE QUE SOLUCIONA EL ERROR:
    public SistemaSalud moduloSalud;

    [Header("Estadísticas Base")]
    public float velocidadMovimiento = 5f; // Lo trajimos de la vieja Entidad

    [Header("¡OBLIGATORIO: Configuración de Cámara!")]
    public Camera camaraPrincipal;

    [Header("Linterna")]
    public GameObject objetoLinterna;

    private bool estaMuerto = false; // Variable local para bloquear el input

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
        moduloSalud = GetComponent<SistemaSalud>(); // Conectamos la salud

        if (camaraPrincipal == null) camaraPrincipal = Camera.main;

        // PATRÓN OBSERVER: El Cerebro se suscribe al evento de muerte
        if (moduloSalud != null)
        {
            moduloSalud.OnMuerte += ProcesarMuerte;
        }
    }

    private void OnDestroy()
    {
        // Nos desuscribimos para evitar errores de memoria si cambiamos de escena
        if (moduloSalud != null)
        {
            moduloSalud.OnMuerte -= ProcesarMuerte;
        }
    }

    public void Start() // Ya no es 'override', es un Start normal
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
    }

    // ==========================================
    // REACCIÓN A LA MUERTE (Llamada por Evento)
    // ==========================================
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