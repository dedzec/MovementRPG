using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {
  // REFERENCES
  private Rigidbody rb;

  // PHYSICS
  public float jump = 70.0f;
  public float gravity = 45.0f;
  public float speed = 2.0f;
  public float runSpeed = 5.0f;
  public float rotation = 100f;
  LayerMask mask = -1;
  private float xDeg = 0.0f;

  // NAVIGATION
  // public bool isAutoMove;
  public bool isGrounded;
  public bool isJumping;
  private string moveStatus = "idle";
  private Vector3 moveDirection = Vector3.zero;

  public float lookRadius = 1.5f;

  // [SerializeField] Transform groundCheck;
  private GameObject groundCheck;
  [SerializeField] LayerMask ground;

  void Awake() {
    // Create a child GameObject to identify the Ground
    // groundCheck = new GameObject("GroundCheck");
    // groundCheck.transform.parent = transform;
    // groundCheck.transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
    
    rb = GetComponent<Rigidbody>();
  }

  void Start() {
    Vector3 angles = transform.eulerAngles; 
    xDeg = angles.x;

    if (rb) {
      rb.freezeRotation = true;
      // rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
  }

  void Update() {
    // isGrounded = Physics.CheckSphere(groundCheck.transform.position, .1f, ground);
    isGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), .1f, ground);
    if (isGrounded) isJumping = false;
  }

  void LateUpdate() {
    if (isGrounded) {
      if (Input.GetKey(KeyCode.A)) xDeg -= +1 * rotation * Time.deltaTime;
      if (Input.GetKey(KeyCode.D)) xDeg -= -1 * rotation * Time.deltaTime;
      if (Input.GetKey(KeyCode.W)) {
        moveStatus = "front";
        moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));
      } else if (Input.GetKey(KeyCode.S)) {
        moveStatus = "back";
        moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));
      } else {
        moveStatus = "idle";
        moveDirection = new Vector3(0, 0, 0);
      }

      moveDirection *= moveStatus == "back" ? speed : runSpeed;

      // if (Input.GetKeyDown(KeyCode.Space)) {
      //   rb.AddForce(0, jump, 0, ForceMode.Impulse);
      //   isJumping = true;
      // }

      // if (Input.GetKeyDown(KeyCode.Space)) {
      //   rb.velocity = Vector3.up * runSpeed;
      //   isJumping = true;
      // }

      if (Input.GetKeyDown(KeyCode.Space)) {
        isJumping = true;
        rb.velocity = new Vector3(rb.velocity.x, runSpeed, rb.velocity.z);
      }
    }

    if (!isJumping) transform.rotation = Quaternion.Euler(0, xDeg, 0);
    transform.Translate(moveDirection * Time.deltaTime);
  }

  private void OnDrawGizmosSelected() {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, lookRadius);
  }
}
