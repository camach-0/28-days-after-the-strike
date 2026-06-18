using UnityEngine;

public class TriggerAscensor : MonoBehaviour
{
    private bool yaSeActivo = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
      
        if (yaSeActivo) return;

   
        if (collision.CompareTag("Player"))
        {
            yaSeActivo = true;
            GestorVictoria.Instancia.IniciarSecuenciaVictoria();
        }
    }
}