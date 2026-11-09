using UnityEngine;

public class GoalDoor : MonoBehaviour
{
    // The player who is supposed to reach this door to win
    public PlayerController.PlayerID targetWinner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            if (player.playerType == targetWinner)
            {
                GameManager.Instance.DeclareWinner(targetWinner);
            }
        }
    }
}