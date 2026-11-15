using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
public class EndManager : MonoBehaviour
{
    [Header("referenxe")]
    public PlatformGenerator generator;
    public Collider2D playerCollider;
    [Header("vittoria")]
    public string victory = "victory"; //nome della scena per futuro
    public GameObject panel;  

    public float heigh = 1f;
    private Bounds winZone;
    private bool ready;
    private bool win;

    IEnumerator Start()
    {
        if (generator == null || playerCollider == null)
        {
            Debug.Log("non collegati");
            yield break;
        }
        if(panel!=null)panel.SetActive(false);
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
        if (winZone.Intersects(playerCollider.bounds)) Win();
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

    public void mainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("nome");//mettere nome dellla scena iniziale
    }
}
