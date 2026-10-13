using UnityEngine;

namespace CitySky.Demo
{
    /// <summary>
    /// Backdrop PROVISÓRIO para o marco F1-112 (editor embutido no jogo): orbita a câmera devagar para
    /// a cena apresentar frames continuamente — é o que estressa a composição do painel-filho por cima
    /// do swapchain — e mantém o jogo renderizando mesmo sem foco (condição real do produto). NÃO é a
    /// renderização final da cidade (ECS/overlays + URP vêm depois); é só um cenário vivo para enxergar
    /// o painel encaixado. Sem lógica de domínio.
    /// </summary>
    public sealed class DemoSceneDriver : MonoBehaviour
    {
        private Transform _camera;
        private float _angle;

        private void Awake()
        {
            UnityEngine.Application.runInBackground = true; // renderiza mesmo quando o editor tem o foco
            UnityEngine.Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            _camera = Camera.main != null ? Camera.main.transform : null;
        }

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            _angle += Time.deltaTime * 12f;
            float rad = _angle * Mathf.Deg2Rad;
            _camera.position = new Vector3(Mathf.Sin(rad) * 16f, 9f, Mathf.Cos(rad) * 16f);
            _camera.LookAt(Vector3.zero);
        }
    }
}
