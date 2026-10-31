using UnityEngine;

public class FallDeath : MonoBehaviour
{
    [SerializeField] private float deathY = -10f;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (transform.position.y < deathY)
        {
            if (playerHealth != null)
            {
                playerHealth.Kill();
            }
        }
    }
}