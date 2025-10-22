using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform target;
	public float fallDistance = 12f;         // насколько ниже камеры игрок должен упасть, чтобы умереть       
	private bool isDead = false;

	public PauseMenuUI pauseMenu;

	void LateUpdate()
	{

		if (target == null || isDead) return;

		if (target.position.y > transform.position.y)
		{
			Vector3 newPos = new Vector3(transform.position.x, target.position.y, transform.position.z);
			transform.position = newPos;
		}

		if (target.position.y < transform.position.y - fallDistance)
		{
			OnPlayerDeath();
		}
	}
	
	 void OnPlayerDeath()
    {
        isDead = true;
		Time.timeScale = 0f;

        // Отключаем управление игрока
        Player player = target.GetComponent<Player>();
        if (player != null)
            player.enabled = false;

        // Останавливаем движение
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
		if (rb != null)
			rb.linearVelocity = Vector2.zero;
			
		if (pauseMenu != null)
                pauseMenu.ShowDeathMenu();
    }

}
