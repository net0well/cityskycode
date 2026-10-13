using UnityEngine;

namespace CitySky.Demo
{
    /// <summary>Gira o objeto para manter a cena-backdrop em movimento (ver <see cref="DemoSceneDriver"/>).</summary>
    public sealed class DemoSpinner : MonoBehaviour
    {
        public Vector3 DegreesPerSecond = new Vector3(0f, 45f, 0f);

        private void Update() => transform.Rotate(DegreesPerSecond * Time.deltaTime, Space.Self);
    }
}
