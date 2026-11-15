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
    public int objectiveAltitude = 100;
    public GameObject player;

    void Update()
    {
        altimeterText.text = "Altitude: " + Mathf.RoundToInt(player.transform.position.y).ToString() + "m";
        altitude = Mathf.RoundToInt(player.transform.position.y);
        altitudeFloat = altitude/(float)objectiveAltitude;
        altimeterFillImage.fillAmount = altitudeFloat;
    }
}
