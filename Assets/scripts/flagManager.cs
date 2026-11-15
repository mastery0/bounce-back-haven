using UnityEngine;

public class flagManager : MonoBehaviour
{
    public EndManager endManager;

    private bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        triggered = true;
        endManager.Win();
    }
}
