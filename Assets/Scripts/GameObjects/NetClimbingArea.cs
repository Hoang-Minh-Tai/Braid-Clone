using UnityEngine;

public class NetClimbingArea : MonoBehaviour
{
    private ClimbingNet net;

    private void Awake()
    {
        net = GetComponentInParent<ClimbingNet>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            net.inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            net.inRange = false;
        }
    }
}
