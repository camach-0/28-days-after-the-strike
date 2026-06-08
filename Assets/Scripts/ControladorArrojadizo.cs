using UnityEngine;

// Heredamos de ControladorArma para que el inventario de Jorge lo reconozca
public class ControladorArrojadizo : ControladorArma
{
    [Header("Configuración del Sistema")]
    // 1. ¡OBLIGATORIO! La etiqueta para que el nuevo sistema lo reconozca
    //public override string EtiquetaPoolSuelo => "BombaCasera";

    [Header("Configuración de Lanzamiento")]
    [Tooltip("La etiqueta del Pool de la bomba que saldrá volando")]
    public string etiquetaBombaVuelo = "BombaCasera";
    public float fuerzaLanzamiento = 15f;

    private bool seEstaUsando = false;

    // El JugadorCombate de Jorge llamará a esta función al hacer clic
    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando) return;
        LanzarBomba();
    }

    // 2. ¡OBLIGATORIO! El método de empujón aunque las bombas no se usen para empujar
    public override void IntentarEmpujon(Vector2 direccion)
    {
        // Se deja vacío porque una bomba en la mano no empuja zombis, 
        // pero el script base exige que esta función exista.
    }

    private void LanzarBomba()
    {
        seEstaUsando = true;

        // 1. Pedimos la bomba de verdad (la que vuela) al PoolManager
        GameObject bombaFisica = PoolManager.Instancia.SolicitarObjeto(etiquetaBombaVuelo, transform.position, Quaternion.identity);

        if (bombaFisica != null)
        {
            // 2. Calculamos la dirección (derecha del arma) y la empujamos
            Vector2 direccion = transform.right;
            Rigidbody2D rbBomba = bombaFisica.GetComponent<Rigidbody2D>();

            if (rbBomba != null)
            {
                rbBomba.linearVelocity = Vector2.zero;
                rbBomba.AddForce(direccion * fuerzaLanzamiento, ForceMode2D.Impulse);
            }
        }

        // 3. Comunicación con el inventario (sacar la bomba de la mano)
        InventarioJugador miInventario = GetComponentInParent<InventarioJugador>();
        if (miInventario != null)
        {
            // Buscamos esta bomba en los slots y la vaciamos
            for (int i = 0; i < miInventario.ranuras.Length; i++)
            {
                if (miInventario.ranuras[i] == this)
                {
                    miInventario.ranuras[i] = null;
                    break;
                }
            }

            // Automáticamente devolvemos el arma al jugador (Slot 1 o Slot 2)
            if (miInventario.ranuras[0] != null) miInventario.CambiarSlot(0);
            else miInventario.CambiarSlot(1);
        }

        // 4. Destruimos este objeto "de utilería" de las manos
        Destroy(gameObject);
    }
}