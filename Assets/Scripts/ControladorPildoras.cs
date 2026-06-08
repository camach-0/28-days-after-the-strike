using UnityEngine;

public class ControladorPildoras : ControladorArma
{
    [Header("Efecto de las Píldoras")]
    public float cantidadSaludTemporal = 60f;

    private SistemaSalud saludJugador;
    private bool seEstaUsando = false;

    private void Start()
    {
        saludJugador = GetComponentInParent<SistemaSalud>();
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando || saludJugador == null) return;

        // Si ya tiene la vida al 100% (sumando real + temporal), no deja tomar pastillas
        if (saludJugador.vidaActual + saludJugador.vidaTemporal >= saludJugador.vidaMaxima)
        {
            Debug.Log("Vida al máximo, no necesitas píldoras.");
            return;
        }

        TomarPildoras();
    }

    private void TomarPildoras()
    {
        seEstaUsando = true;

        // ¡LA CLAVE ESTÁ AQUÍ! Añade vida que se irá decayendo
        saludJugador.AñadirVidaTemporal(cantidadSaludTemporal);
        Debug.Log("¡Píldoras tomadas! +60 HP Temporal.");

        InventarioJugador miInventario = GetComponentInParent<InventarioJugador>();
        if (miInventario != null)
        {
            for (int i = 0; i < miInventario.ranuras.Length; i++)
            {
                if (miInventario.ranuras[i] == this)
                {
                    miInventario.ranuras[i] = null;
                    break;
                }
            }

            if (miInventario.ranuras[0] != null) miInventario.CambiarSlot(0);
            else miInventario.CambiarSlot(1);
        }

        Destroy(gameObject);
    }

    public override void IntentarEmpujon(Vector2 direccion) { }
}