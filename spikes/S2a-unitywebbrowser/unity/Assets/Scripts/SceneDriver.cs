using UnityEngine;

// Cena 3D colorida e sempre em movimento para o spike S2a: garante que o swapchain faz Present
// a cada frame (é isso que estressa a composição da janela-filha do editor por cima dele) e que o
// jogo continua renderizando mesmo sem foco (runInBackground), que é a condição real do produto.
public sealed class SceneDriver : MonoBehaviour
{
    private Transform _camera;
    private float _angle;

    private void Awake()
    {
        Application.runInBackground = true;   // continua renderizando quando o editor tem o foco
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        _camera = Camera.main != null ? Camera.main.transform : null;
    }

    private void Update()
    {
        if (_camera == null)
        {
            return;
        }

        // Órbita lenta ao redor da origem: movimento contínuo = Present contínuo.
        _angle += Time.deltaTime * 12f;
        float rad = _angle * Mathf.Deg2Rad;
        _camera.position = new Vector3(Mathf.Sin(rad) * 14f, 8f, Mathf.Cos(rad) * 14f);
        _camera.LookAt(Vector3.zero);
    }
}
