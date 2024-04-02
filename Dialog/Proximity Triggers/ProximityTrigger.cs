using UnityEngine;

public class ProximityTrigger : MonoBehaviour
{
    public GameObject _icon;
    protected PlayerPlatformerController player_movement;

    protected virtual void Start()
    {
        player_movement = GameObject.Find(PlayerPlatformerController.player_collider).GetComponent<PlayerPlatformerController>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals(PlayerPlatformerController.player_collider))
        {
            Enter();
        }
    }

    protected virtual void Update()
    {
        if (_icon.activeSelf && player_movement.IsGrounded && !MenuState.On)
        {
            if (Command.Interact())
            {
                _icon.SetActive(false);
                SoundEffectSystem.Play(SFX._interact);
                TriggerEvent();
            }
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals(PlayerPlatformerController.player_collider))
        {
            Exit();
        }
    }

    protected virtual void TriggerEvent() { }

    protected virtual void Enter()
    {
        _icon.SetActive(true);
    }

    protected virtual void Exit()
    {
        _icon.SetActive(false);
    }
}
