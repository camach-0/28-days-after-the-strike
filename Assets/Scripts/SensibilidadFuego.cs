using UnityEngine;

public class SensibilidadFuego : MonoBehaviour
{
    [Tooltip("¿Cuánto daño por segundo recibe este zombi al quemarse?")]
    public float danoPorSegundo = 5f;

    [Tooltip("¿El fuego se queda para siempre hasta que muera?")]
    public bool esPermanente = true;
}