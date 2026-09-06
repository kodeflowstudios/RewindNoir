using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[Header("Speed")]
	[SerializeField] private float walkSpeed = 5f;
	[SerializeField] private float runSpeed = 8f;
	[SerializeField] private float crouchSpeed = 2f;

	[Header("Jumping&Gravity")]
	[SerializeField] private float jumpForce = 7f;
	[SerializeField] private float gravity = -12f;
	[SerializeField] private float initalFallVelocity = -2f;

	[Header("Crouching")]
	[SerializeField] private float standingHeight = 2f;
	[SerializeField] private float crouchingHeight = 1f;
	[SerializeField] private float crouchingTransitionSpeed = 10f;
	[SerializeField] private float cameraOffset = 0.4f;


	[Header("Refs")]
	[SerializeField] private Transform cameraTransform;
	[SerializeField] private CinemachineInputAxisController camController;
	[SerializeField] private InputActionReference moveAction;
	[SerializeField] private InputActionReference jumpAction;
	[SerializeField] private InputActionReference crouchAction;
	[SerializeField] private InputActionReference sprintAction;

	private CharacterController _characterController;
    private Vector2 _moveInput;
    private bool _canMove;
    private bool _isGrounded;
	private bool _isRunning;
	private bool _isCrouching;
	private float _verticalVelocity;
	private float _targetHeight;

    private void Awake()
	{
		EnableMoving();
		UpdateSensitivity();

		_characterController = GetComponent<CharacterController>();
		_targetHeight = standingHeight;

	}

	public void UpdateSensitivity()
	{
		float _sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1);

		foreach (var c in camController.Controllers)
		{
			if (c.Name == "Look X (Pan)")
			{
				c.Input.Gain = _sensitivity;

			}
			if (c.Name == "Look Y (Tilt)")
			{
				c.Input.Gain = -_sensitivity;
			}
		}
	}

	private void OnEnable()
	{
		moveAction.action.performed += StoreMovementInput;
		moveAction.action.canceled += StoreMovementInput;
		jumpAction.action.performed += Jump;
		sprintAction.action.performed += Sprint;
		sprintAction.action.canceled += Sprint;
		crouchAction.action.performed += Crouch;
	}

    private void OnDisable()
	{
		moveAction.action.performed -= StoreMovementInput;
		moveAction.action.canceled -= StoreMovementInput;
		jumpAction.action.performed -= Jump;
		sprintAction.action.performed -= Sprint;
		sprintAction.action.canceled -= Sprint;
		crouchAction.action.performed -= Crouch;
	}

	public void EnableMoving()
	{
		_canMove = true;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		camController.enabled = true;
	}

	public void DisableMoving()
	{
		_canMove = false;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		camController.enabled = false;
	}

    private void Update()
    {
		_isGrounded = _characterController.isGrounded;

		HandleGravity();
		if (!_canMove) return;
		HandleMovement();
		HandleCrouchTransition();
    }

    private void StoreMovementInput(InputAction.CallbackContext context)
    {
		_moveInput = context.ReadValue<Vector2>();
    }

    private void Crouch(InputAction.CallbackContext context)
    {
		if (!_canMove) return;
		if (_isCrouching)
		{
			if (!CanStandUp()) return;
			_targetHeight = standingHeight;
		}
		else _targetHeight = crouchingHeight;
		_isCrouching = !_isCrouching;
    }

    private bool CanStandUp()
    {
		return !Physics.CapsuleCast(
				transform.position + _characterController.center,
				transform.position + (Vector3.up * _characterController.height / 2),
				_characterController.radius,
				Vector3.up
		);
    }

    private void Sprint(InputAction.CallbackContext context)
    {
		_isRunning = context.performed;
    }

    private void Jump(InputAction.CallbackContext context)
    {
		if (_isGrounded)
		{
			_verticalVelocity = jumpForce;
		}
    }

    private void HandleGravity()
    {
		if (_isGrounded && _verticalVelocity < 0)
		{
			_verticalVelocity = initalFallVelocity;
		}

		_verticalVelocity += gravity * Time.deltaTime;
    }

	private void HandleMovement()
	{
		Vector3 move = cameraTransform.TransformDirection(new Vector3(_moveInput.x, 0, _moveInput.y)).normalized;
		float currentSpeed = _isCrouching ? crouchSpeed : _isRunning ? runSpeed : walkSpeed;
		Vector3 finalMove = move * currentSpeed; 
		finalMove.y = _verticalVelocity;

		var collisions = _characterController.Move(finalMove * Time.deltaTime);
		if ((collisions & CollisionFlags.Above) != 0)
	 	{
			_verticalVelocity = initalFallVelocity;
		}
	}

    private void HandleCrouchTransition()
    {
		float currentHeight = _characterController.height;
		if (Mathf.Abs(currentHeight - _targetHeight) < 0.01f)
		{
			_characterController.height = _targetHeight;
			return;
		}

		float newHeight = Mathf.Lerp(currentHeight, _targetHeight, crouchingTransitionSpeed * Time.deltaTime);
		_characterController.height = newHeight;
		_characterController.center = Vector3.up * (newHeight * 0.5f);

		Vector2 cameraTargetPosition = cameraTransform.localPosition;
		cameraTargetPosition.y = _targetHeight - cameraOffset;
		cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraTargetPosition, crouchingTransitionSpeed * Time.deltaTime);
    }
}
