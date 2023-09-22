using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DEDZEC.Controller {
  
  public class InputType {
    public bool Valid;
    public bool Held;
  }

  [RequireComponent(typeof(Rigidbody))]
  public class MoveToClick : MonoBehaviour {
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
    public bool isAutoMove;
    public bool isGrounded;
    public bool isJumping;
    private string moveStatus = "idle";
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 destination;
    public float maxGroundRaycastDistance = 100;
    public float minimumPathDistance = 0.5f;
    // [SerializeField] Transform groundCheck;
    private GameObject groundCheck;
    [SerializeField] LayerMask ground;

    public float holdMoveCd = 0.1f;
    public float nextHoldMove;

    public float lookRadius = 1.5f;

    // INPUT FEEDBACK
    public bool alwaysTriggerGroundPathFeedback;
    public GameObject validGroundPathPrefab;
    public GameObject rectifiedGroundPathPrefab;
    public float groundMarkerDuration = 2;
    public Vector3 markerPositionOffset = new Vector3(0, 0.1f, 0);

    void Awake() {
      // Create a child GameObject to identify the Ground
      groundCheck = new GameObject("GroundCheck");
      groundCheck.transform.parent = transform;
      groundCheck.transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
      
      rb = GetComponent<Rigidbody>();
    }

    void Start() {
      Vector3 angles = transform.eulerAngles; 
      xDeg = angles.x;

      if (rb) {
        // rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
      }
    }

    void Update() {
      // Quaternion rot = Quaternion.Euler(0, Input.mousePosition.x, 0);
      // transform.rotation = rot;

      if (Input.GetKeyDown(KeyCode.Mouse0)) {
        // isAutoMove = true;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, maxGroundRaycastDistance)) {
          var point = hit.point;
          transform.LookAt(new Vector3(point.x + markerPositionOffset.x, point.y + markerPositionOffset.y, point.z + markerPositionOffset.z));
          // transform.rotation = Quaternion.LookRotation(newDirection);
        }
 
        // Calcula o vetor entre a posição do mouse e o objeto
        // var vector = transform.position - mousePos;
 
        // And then the distance
        // var distance = vector.magnitude;

      }

      if (
        Input.GetKey(KeyCode.W)
        || Input.GetKey(KeyCode.S)
        || Input.GetKey(KeyCode.A)
        || Input.GetKey(KeyCode.D)
      ) {
        isAutoMove = false;

        if (Input.GetKey(KeyCode.A)) xDeg -= +1 * rotation * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) xDeg -= -1 * rotation * Time.deltaTime;
      }
    }

    void LateUpdate() {
      MoveWASD();
      // AutoMove();
      // SetNewDestination();
    }

    private void MoveWASD() {
      if (isAutoMove) return;
      Quaternion rotation = Quaternion.Euler(0, xDeg, 0);

      if (isGrounded) {
        isJumping = false;
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
          rb.velocity = new Vector3(rb.velocity.x, runSpeed, rb.velocity.z);
          isJumping = true;
        }
      }

      if (!isJumping) transform.rotation = rotation;
      transform.Translate(moveDirection * Time.deltaTime);

      isGrounded = Physics.CheckSphere(groundCheck.transform.position, .1f, ground);
    }

    private void AutoMove() {
      if (!isAutoMove || IsPointerOverUIObject()) return;

      InputType inputType = InputMove();
      if (!inputType.Valid) return;
      if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, maxGroundRaycastDistance)) return;
      bool validClick = true;
      if (IsPathTooClose(destination)) return;
      destination = hit.point;
      if (inputType.Held) return;
      SpawnGroundPathMarker(destination, validClick);
    }

    private InputType InputMove() {
      InputType inputType = new InputType();
      if (Input.GetKeyDown(KeyCode.Mouse0)) {
        inputType.Valid = true;
        isAutoMove = true;
        return inputType;
      }

      if (!(Time.time >= nextHoldMove)) {
        inputType.Valid = false;
        return inputType;
      }

      if (!Input.GetKey(KeyCode.Mouse0)) return inputType;
      nextHoldMove = Time.time + holdMoveCd;
      inputType.Valid = true;
      inputType.Held = true;
      return inputType;
    }

    private void SetNewDestination() {
      float distance = Vector3.Distance(transform.position, new Vector3(destination.x + markerPositionOffset.x, destination.y + markerPositionOffset.y, destination.z + markerPositionOffset.z));
      if (!isAutoMove || distance < 1f) return;

      float moveDistance = Mathf.Clamp(0.7f * Time.fixedDeltaTime, 0, distance);

      Vector3 move = (destination - transform.position).normalized * moveDistance;

      transform.Translate(move);
      // transform.position = Vector3.Lerp(transform.position, move, speed * Time.deltaTime);
      // transform.position = Vector3.MoveTowards(transform.position, move, Time.deltaTime * speed);
    }

    private bool IsPathTooClose(Vector3 point) {
      return Vector3.Distance(transform.position, point) < minimumPathDistance;
    }

    private void SpawnGroundPathMarker(Vector3 point, bool rectified) {
      GameObject prefab = rectified ? validGroundPathPrefab : rectifiedGroundPathPrefab;
      if (prefab == null) return;
      GameObject marker = Instantiate(prefab, new Vector3(point.x + markerPositionOffset.x, point.y + markerPositionOffset.y, point.z + markerPositionOffset.z), prefab.transform.rotation);
      Destroy(marker, groundMarkerDuration);
    }

    public bool IsPointerOverUIObject() {
      var eventDataCurrentPosition = new PointerEventData(EventSystem.current) {
        position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
      };
      var results = new List<RaycastResult>();
      if (EventSystem.current == null) return false;
      EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
      return results.Count > 0;
    }

    // private bool IsGrounded() {
    //   return Physics.CheckSphere(groundCheck.transform.position, .1f, ground);
    // }

    private void OnDrawGizmosSelected() {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
  }
}
