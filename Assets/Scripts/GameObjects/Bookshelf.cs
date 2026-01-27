using UnityEngine;
using UnityEngine.Events;

public class Bookshelf : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bookRender;
    [SerializeField] private float interactionDistance = 2.0f; // Distance to check for player proximity
    private RewindableAudioPlayer audioPlayer;
    private Transform player;

    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;

    public UnityEvent onBookOpen;
    public UnityEvent onBookClose;

    private bool isOpen = false;

    void Awake()
    {
        audioPlayer = GetComponent<RewindableAudioPlayer>();
    }
    private void Start()
    {
        player = Player.Instance.transform;
    }

    private void FixedUpdate()
    {
        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance && !isOpen)
        {
            OpenBookshelf();
        }
        else if (distance > interactionDistance && isOpen)
        {
            CloseBookshelf();
        }
    }

    private void OpenBookshelf()
    {
        isOpen = true;
        bookRender.sprite = openSprite; // Change to open sprite
        onBookOpen?.Invoke();
        audioPlayer.Play("book_open"); // Play opening sound
    }

    private void CloseBookshelf()
    {
        isOpen = false;
        bookRender.sprite = closedSprite; // Change to closed sprite
        onBookClose?.Invoke();
        audioPlayer.Play("book_close"); // Play closing sound
    }
}
