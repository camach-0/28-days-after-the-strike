using UnityEngine;

public interface IReceptorDano
{
    void RecibirDano(float cantidad, Vector2 direccion, float empuje, int idAtacante = -1);

}