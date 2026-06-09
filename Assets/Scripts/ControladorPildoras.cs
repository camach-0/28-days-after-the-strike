using UnityEngine;

public class ControladorPildoras : ControladorArma
{
    [Header("Efecto de las Píldoras")]
    public float cantidadSaludTemporal = 60f;

    [Header("Sistema de Botín (Drop)")]
    public string etiquetaSuelo = "Pickup_Pildoras";

    private SistemaSalud saludJugador;

    private void Start()
    {
        saludJugador = GetComponentInParent<SistemaSalud>();
    }

    // CLIC IZQUIERDO: Tomarse las pastillas
    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (saludJugador == null || (saludJugador.vidaActual + saludJugador.vidaTemporal) >= saludJugador.vidaMaxima) return;

        saludJugador.AñadirVidaTemporal(cantidadSaludTemporal);
        Debug.Log("<color=cyan>¡Píldoras tomadas! +60 HP Temporal.</color>");

        ConsumirYDestruir();
    }

    // CLIC DERECHO: Pasar a un compañero
    public override void IntentarEmpujon(Vector2 direccion)
    {
        if (saludJugador == null) return;

        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var col in cercanos)
        {
            if (col.gameObject != saludJugador.gameObject)
            {
                SistemaSalud s = col.GetComponent<SistemaSalud>();
                if (s != null && s.esSuperviviente && !s.estaMuertoDefinitivo)
                {
                    PasarItem(s);
                    break;
                }
            }
        }
    }

    private void PasarItem(SistemaSalud aliado)
    {
        InventarioJugador invAliado = aliado.GetComponent<InventarioJugador>();
        InventarioJugador miInv = GetComponentInParent<InventarioJugador>();

        if (invAliado == null || miInv == null) return;

        // --- CORRECCIÓN: Buscamos desde la última ranura hacia la primera ---
        int slotVacio = -1;
        for (int i = invAliado.ranuras.Length - 1; i >= 0; i--)
        {
            if (invAliado.ranuras[i] == null)
            {
                slotVacio = i;
                break;
            }
        }

        if (slotVacio != -1)
        {
            for (int i = 0; i < miInv.ranuras.Length; i++)
            {
                if (miInv.ranuras[i] == this) miInv.ranuras[i] = null;
            }

            JugadorMovimiento movAliado = aliado.GetComponent<JugadorMovimiento>();
            transform.SetParent(movAliado != null ? movAliado.pivoteArma : aliado.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            invAliado.ranuras[slotVacio] = this;
            this.saludJugador = aliado;
            gameObject.SetActive(false);

            if (miInv.ranuras[0] != null) miInv.CambiarSlot(0);
            else miInv.CambiarSlot(1);

            Debug.Log("<color=green>Píldoras entregadas al aliado.</color>");
        }
    }

    private void ConsumirYDestruir()
    {
        InventarioJugador miInv = GetComponentInParent<InventarioJugador>();
        if (miInv != null)
        {
            for (int i = 0; i < miInv.ranuras.Length; i++)
            {
                if (miInv.ranuras[i] == this) { miInv.ranuras[i] = null; break; }
            }
            if (miInv.ranuras[0] != null) miInv.CambiarSlot(0);
            else miInv.CambiarSlot(1);
        }
        Destroy(gameObject);
    }
}