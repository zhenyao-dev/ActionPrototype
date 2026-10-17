using System.Collections;
using UnityEngine;

public class HitEffectController : MonoBehaviour
{
    [SerializeField] private float effectDuration = 0.2f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Coroutine hideCoroutine;

    public float EffectDuration => effectDuration;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void Play()
    {
        if (animator == null || spriteRenderer == null)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        spriteRenderer.enabled = true;
        animator.SetTrigger("Play");

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(effectDuration);

        spriteRenderer.enabled = false;
        hideCoroutine = null;
    }
}