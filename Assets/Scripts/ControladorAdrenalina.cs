using UnityEngine;

public class ControladorAdrenalina : ControladorArma
{
    [Header("Efecto de Adrenalina")]
    public float multiplicadorVelocidad = 1.5f;
    public float tiempoEfecto = 10f;
    public float saludTemporal = 25f;

    private SistemaSalud saludJugador;

    private void Start()
    {
        saludJugador = GetComponentInParent<SistemaSalud>();
    }

    // CLIC IZQUIERDO: Inyectarse
    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        JugadorMovimiento movimiento = GetComponentInParent<JugadorMovimiento>();
        if (movimiento != null)
        {
            movimiento.InyectarAdrenalina(multiplicadorVelocidad, tiempoEfecto);
        }

        if (saludJugador != null)
        {
            saludJugador.AñadirVidaTemporal(saludTemporal);
            Debug.Log("<color=cyan>¡Adrenalina inyectada!</color>");
        }

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

            Debug.Log("<color=green>Adrenalina entregada al aliado.</color>");
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