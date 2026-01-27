using UnityEngine;

public class Player_AnimationTrigger : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    public void OpenDoorFinish()
    {
        player.OpenCurrentDoor();
    }
}
