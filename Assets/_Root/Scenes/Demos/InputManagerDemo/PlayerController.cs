using PROJECTNAME.Managers;
using UnityEngine;

namespace Demos
{
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings"), SerializeField,
	 Tooltip("Speed of the player movement")]
	private float _MoveSpeed = 5f;

	private void Update()
	{
		// Use MoveInput to handle movement directly from InputManager.
		Vector2 moveInput = InputManager.Instance.MoveInput;
		MoveCharacter(moveInput);
	}

	private void OnEnable()
	{
		// Subscribe to InputManager events for different actions.
		InputManager.Instance.OnMovePressed += HandleMovement;
		InputManager.Instance.OnJumpPressed += HandleJump;
		InputManager.Instance.OnAttackPressed += HandleAttack;
		InputManager.Instance.OnInteractionPressed += HandleInteraction;
	}

	private void OnDisable()
	{
		// Unsubscribe from events when the script is disabled.
		if (!InputManager.Instance) return;
		InputManager.Instance.OnMovePressed -= HandleMovement;
		InputManager.Instance.OnJumpPressed -= HandleJump;
		InputManager.Instance.OnAttackPressed -= HandleAttack;
		InputManager.Instance.OnInteractionPressed -= HandleInteraction;
	}

	// Optional method to adjust speed dynamically.
	public void SetSpeed(float newSpeed)
	{
		_MoveSpeed = newSpeed;
	}

	// Event handler for attack input.
	private void HandleAttack()
	{
		Debug.Log("Attack Pressed!");
	}

	// Event handler for interaction input.
	private void HandleInteraction()
	{
		Debug.Log("Interaction Pressed!");
	}

	// Event handler for jump input.
	private void HandleJump()
	{
		Debug.Log("Jump Pressed!");
	}

	// Event handler for movement start.
	private void HandleMovement()
	{
		Debug.Log("Movement Input Detected!");
	}

	// Method to move the character based on input.
	private void MoveCharacter(Vector2 move)
	{
		// Moving the player in the XZ plane.
		Vector3 movement = new Vector3(move.x, 0, move.y) *
		                   (_MoveSpeed * Time.deltaTime);
		transform.Translate(movement);
	}
}
}