using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
public class EndManager : MonoBehaviour
{
    [Header("referenxe")]
    public PlatformGenerator generator;
    public Collider2D playerCollider;
    [Header("vittoria")]
    public string victory = "victory"; //nome della scena per futuro
    public GameObject panel;

    [Header("game over")]
    public GameObject panelOver;
    public float minY;
    public bool relativeRespown = false;
    public bool intantRespown = false;

    private Rigidbody2D playerRb;
    private Vector3 spawnPosition;
    public float heigh = 1f;
    private Bounds winZone;
    private bool ready;
    private bool win;
    private bool ended = false;

    IEnumerator Start()
    {
        if (generator == null || playerCollider == null)
        {
            Debug.Log("non collegati");
            yield break;
        }
        if(panel!=null)panel.SetActive(false);
        if (panelOver != panel) panel.SetActive(false);
        yield return null;
        Bounds last = generator.lastBounds;
        winZone = new Bounds(
            new Vector3(last.center.x, last.max.y + heigh * 0.5f, 0f),
            new Vector3(last.size.x, heigh, 10f)
            );
        ready = true;
    }

    void Update()
    {
        if (!ready || win) return;
        if (winZone.Intersects(playerCollider.bounds))
        {
            Win();
            return;
        }
        if(playerCollider.transform.position.y<minY)
        {
            if (intantRespown) respown();
            else gameOver();
        }

    }

    private void Win()
    {
        win = true;
        if (panel != null)
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }

    }

    private void gameOver()
    {
        ended = true;
        if (panelOver != null)
        {
            panelOver.SetActive(true);
            Time.timeScale = 0f;
        }
        else respown();
    }

    private void respown()
    {
        Time.timeScale = 1f;
        ended = false;
        if (panelOver != null) panelOver.SetActive(false);
        if(playerRb!=null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.position = spawnPosition;
        }
        playerCollider.transform.position = spawnPosition;
        Physics2D.SyncTransforms();
    }

    public void restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void mainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("nome");//mettere nome dellla scena iniziale
    }
}
