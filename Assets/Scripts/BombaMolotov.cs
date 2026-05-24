using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BombaMolotov : MonoBehaviour
{
    [Header("Configuración")]
    public string etiquetaPool = "BombaMolotov";
    public string etiquetaFuego = "FuegoMolotov"; // Lo que aparecerá al chocar

    // OnCollisionEnter2D se activa en el instante que la botella choca con cualquier cosa (pared, zombi, suelo)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Estallar();
    }

    private void Estallar()
    {
        // 1. Instanciamos el charco de fuego exactamente donde chocó la botella
        PoolManager.Instancia.SolicitarObjeto(etiquetaFuego, transform.position, Quaternion.identity);

        // (Opcional) Aquí en el futuro puedes poner un sonido de cristal rompiéndose

        // 2. Desaparecemos la botella
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }
}