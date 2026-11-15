using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 10f);
    public float time = 0.15f;
    public bool asseX = true;
    public bool asseY = true;
    public bool canJump = false;
    private Vector3 velocity = Vector3.zero;
    private float high;

    void Start()
    {
        if (target == null)
        {
            Debug.Log("porco  in start");
            enabled = false;
            return;
        }
        high = transform.position.y;
    }
    void LateUpdate()
    {
        Vector3 des = target.position+offset;
        if (!asseX) des.x = transform.position.x;
        if (!asseY) des.y = transform.position.y;
        if(canJump)
        {
            des.y=Mathf.Max(des.y,high);
            high = des.y;
        }
        transform.position = Vector3.SmoothDamp(transform.position, des, ref velocity,time);

    }
}
