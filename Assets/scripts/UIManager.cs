using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [Header("Altimeter")]
    public TMP_Text altimeterText;
    public Image altimeterFillImage;
    public int altitude = 0;    
    public float altitudeFloat = 0f;
    public float objectiveAltitude;
    public GameObject player;

    void Start()
    {
        objectiveAltitude = PlatformGenerator.Instance.lastPlatformY;
        Debug.Log("Objective Altitude: " + objectiveAltitude);
    }
    void Update()
    {
        altimeterText.text = "Altitude: " + Mathf.RoundToInt(player.transform.position.y).ToString() + "m";
        altitude = Mathf.RoundToInt(player.transform.position.y);
        altitudeFloat = altitude/(float)objectiveAltitude;
        altimeterFillImage.fillAmount = altitudeFloat;
    }
}
