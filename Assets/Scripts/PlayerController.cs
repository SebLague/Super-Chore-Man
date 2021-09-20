using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

	public const KeyCode DashKey = KeyCode.None;
	public const KeyCode ShootKey = KeyCode.Space;

	public Cloth cape;
	public float capeForce = 10;
	public float capeVerticalForce = 2;
	public float moveSpeed = 3;
	public float smoothMoveTime = 0.1f;
	public float turnSpeed = 2;
	public float collisionForce = 5;
	public float dashDuration;
	public float dashSpeed;
	public float dashCooldown;
	public GameObject capeCollidersHolder;
	public ScreenCol colEffect;

	Vector3 mousePointOnFloor;
	Camera cam;
	Vector3 velocity;
	Vector3 velocitySmoothV;
	public Vector3 faceDir { get; private set; }
	public Rigidbody rb { get; private set; }
	Vector3 externalForce;
	float dashTimeRemaining;
	Vector3 dashDir;

	int livesRemaining = 3;


	void Start()
	{
		faceDir = transform.forward;
		rb = GetComponent<Rigidbody>();
		cam = Camera.main;
		dashTimeRemaining = -dashCooldown;
		colEffect.red = 0;
		colEffect.greyScale = 0;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody != null)
		{
			if (!collision.gameObject.CompareTag("IgnoreForce"))
			{
				collision.rigidbody.AddForce((transform.forward + Vector3.up * 1.25f) * collisionForce, ForceMode.VelocityChange);
			}
		}
	}


	void Update()
	{
		if (livesRemaining <= 0)
		{
			colEffect.greyScale = Mathf.Clamp01(colEffect.greyScale + Time.deltaTime);
			return;
		}
		// Keyboard movement
		Vector2 moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
		Vector3 targetVelocity = new Vector3(moveDir.x, 0, moveDir.y) * moveSpeed;

		if (targetVelocity != Vector3.zero)
		{
			faceDir = velocity.normalized;
		}

		velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref velocitySmoothV, smoothMoveTime);

		if (Input.GetKeyDown(DashKey) && dashTimeRemaining <= -dashCooldown)
		{
			dashTimeRemaining = dashDuration;
		}

		if (dashTimeRemaining > 0)
		{
			velocity = faceDir * dashSpeed;
		}

		dashTimeRemaining -= Time.deltaTime;

		cape.externalAcceleration = -transform.forward * capeForce + Vector3.up * capeVerticalForce;

		colEffect.red = Mathf.Clamp01(colEffect.red - Time.deltaTime * 2);
	}

	void FixedUpdate()
	{
		if (livesRemaining <= 0)
		{
			return;
		}

		Vector3 newVelocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
		if (Input.GetKey(ShootKey))
		{
			newVelocity.x = 0;
			newVelocity.z = 0;
		}

		rb.velocity = newVelocity + externalForce;
		externalForce -= Time.fixedDeltaTime * externalForce * 5;

		float angle = Mathf.Atan2(faceDir.x, faceDir.z);
		Quaternion targRot = Quaternion.Euler(Vector3.up * angle * Mathf.Rad2Deg);
		rb.MoveRotation(Quaternion.Slerp(rb.rotation, targRot, Time.fixedDeltaTime * turnSpeed));

	}

	public void GhostHit(Vector3 f)
	{
		livesRemaining--;
		if (livesRemaining <= 0)
		{
			rb.constraints = RigidbodyConstraints.None;
			rb.AddTorque(Random.insideUnitSphere * 5, ForceMode.VelocityChange);
			cape.randomAcceleration = Vector3.zero;
			capeCollidersHolder.SetActive(false);
			rb.AddForce(f, ForceMode.VelocityChange);
		}
		colEffect.red = 0.5f;
		externalForce += f;
	}

	void OnDestroy()
	{
		colEffect.red = 0;
		colEffect.greyScale = 0;
	}
}
