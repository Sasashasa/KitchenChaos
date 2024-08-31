using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private const float FOOTSTEP_TIMER_MAX = .1f;
    
    private Player _player;
    private float _footstepTimer;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        _footstepTimer -= Time.deltaTime;

        if (_footstepTimer < 0f)
        {
            _footstepTimer = FOOTSTEP_TIMER_MAX;

            if (_player.IsWalking())
            {
                float volume = 1f;
                SoundManager.Instance.PlayFootstepsSound(_player.transform.position, volume);
            }
        }
    }
}