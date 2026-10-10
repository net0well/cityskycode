using UnityEngine;

// Gira o objeto para manter a cena em movimento (ver SceneDriver).
public sealed class Spinner : MonoBehaviour
{
    public Vector3 DegreesPerSecond = new Vector3(0f, 45f, 0f);

    private void Update() => transform.Rotate(DegreesPerSecond * Time.deltaTime, Space.Self);
}
