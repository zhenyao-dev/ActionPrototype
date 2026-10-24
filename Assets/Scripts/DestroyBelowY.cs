using UnityEngine;

public class DestroyBelowY : MonoBehaviour
{
    [SerializeField] private float destroyY = -10f;

    private void Update()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}