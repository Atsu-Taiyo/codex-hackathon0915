using UnityEngine;

namespace Hackathon.Core
{
    /// <summary>Application entry point. Compose feature controllers here as they are added.</summary>
    public sealed class AppBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("[Hackathon] Application initialized.");
        }
    }
}
