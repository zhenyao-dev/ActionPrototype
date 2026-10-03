using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private float restartDelay = 1f;

    private void Start()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied += HandleGameOver;
        }
    }

    private void HandleGameOver()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(
            restartDelay
        );

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= HandleGameOver;
        }
    }
}