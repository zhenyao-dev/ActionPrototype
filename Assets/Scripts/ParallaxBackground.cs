using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactor = 0.25f;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;

    private void Start()
    {
        startPosition = transform.position;

        if (cameraTransform != null)
        {
            cameraStartPosition =
                cameraTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            return;
        }

        Vector3 cameraDelta =
            cameraTransform.position -
            cameraStartPosition;

        transform.position =
            new Vector3(
                startPosition.x +
                cameraDelta.x * parallaxFactor,

                startPosition.y +
                cameraDelta.y * parallaxFactor,

                startPosition.z
            );
    }
}