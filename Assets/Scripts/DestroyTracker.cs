using UnityEngine;

public class DestroyTracker : MonoBehaviour
{
    [HideInInspector] public WaveManager manager;
    
    private void OnDestroy()
    {
        if (manager != null)
            manager.NotifyDestroyed(gameObject);
    }
    
}