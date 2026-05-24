using System.Collections;
using UnityEngine;

public class EfectoQuemadura : MonoBehaviour
{
    private float danoPorSegundo;
    private float duracion;
    private bool esPermanente;
    private IReceptorDano receptor;

    public void IniciarQuemadura(float dano, float tiempo, bool permanente)
    {
        danoPorSegundo = dano;
        duracion = tiempo;
        esPermanente = permanente;
        receptor = GetComponent<IReceptorDano>();

        if (receptor != null) StartCoroutine(RutinaLlamas());
        else Destroy(this);
    }

    private IEnumerator RutinaLlamas()
    {
        float tiempoQuemando = 0f;
        while (esPermanente || tiempoQuemando < duracion)
        {
            receptor.RecibirDano(danoPorSegundo, Vector2.zero, 0f);
            yield return new WaitForSeconds(1f);
            if (!esPermanente) tiempoQuemando += 1f;
        }
        Destroy(this);
    }

    private void OnDisable()
    {
        Destroy(this);
    }
}