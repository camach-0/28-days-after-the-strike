using UnityEngine;

public class VariacionZombi : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;

    [Header("Los 4 Cerebros (1 Maestro + 3 Overrides)")]
    public RuntimeAnimatorController[] variacionesAnimacion;

    private void OnEnable()
    {
        // Al aparecer en la escena, elige un "cerebro" al azar
        if (animator != null && variacionesAnimacion.Length > 0)
        {
            int indiceAleatorio = Random.Range(0, variacionesAnimacion.Length);
            animator.runtimeAnimatorController = variacionesAnimacion[indiceAleatorio];
        }
    }
}