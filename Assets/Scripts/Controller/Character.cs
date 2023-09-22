using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MovementRPG.Controller
{
  [RequireComponent(typeof(Rigidbody))]
  public class Character : MonoBehaviour
  {
    // REFERENCES
    private Rigidbody rdb;

    // PHYSICS
    #region Phisics config

    public float jump = 7.0f;
    public float speed = 2.0f;
    public float runSpeed = 5.0f;
    public float flySpeed = 20f;
    public float rotation = 100f;
    private float xDeg = 0.0f;
    private float yDeg = 0.0f;

    #endregion

    // NAVIGATION
    #region Navigation config

    public bool isFlying;
    private bool isAutoMove;
    private bool isGrounded;
    private bool isJumping;
    private string moveStatus = "idle";
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 destination;
    public float maxGroundRaycastDistance = 100;
    public float minimumPathDistance = 1f;
    private GameObject groundCheck;
    [SerializeField] LayerMask GroundLayers = 6; // 6: Ground

    #endregion

    // INPUT FEEDBACK
    #region InputFeedback config

    public GameObject GroundPathPrefab;
    public float groundMarkerDuration = 2;
    public Vector3 markerPositionOffset = new Vector3(0, 0.1f, 0);
    public Vector3 markerSpawnPosition = new Vector3(0, 0, 0);

    // public float lookRadius = 1.5f;

    #endregion

    // CHECK DOUBLE PRESS
    #region CheckDoublePress config

    private float delayBetweenPresses = 0.30f;
    private bool pressedFirstTime = false;
    private float lastPressedTime;

    #endregion

    private void Awake() {
      rdb = GetComponent<Rigidbody>();
    }

    private void Start() {
      if (rdb) rdb.freezeRotation = true;
    }

    private void Update() {
      /*
       * Check Ground Path
       */
      if (!GroundPathPrefab) return;
      isGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), .1f, GroundLayers);

      /*
       * Check Player Input
       */
      if (!isFlying) {
        yDeg = 0.0f;
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
          isAutoMove = true;
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

      /*
       * Double Check Space button
       */
      if (Input.GetKeyDown(KeyCode.Space)) {
        if (pressedFirstTime) { // Já pressionamos o botão uma primeira vez, verificamos se a 2ª vez é rápida o suficiente para ser considerada um pressionamento duplo
          bool isDoublePress = Time.time - lastPressedTime <= delayBetweenPresses;
          if (isDoublePress) {
            if (!isFlying) {
              DisabledCursor();
              rdb.useGravity = false;
              isJumping = false;
              isFlying = true;
            }
            pressedFirstTime = false;
          }
        } else { // Ainda não pressionamos o botão pela primeira vez
          pressedFirstTime = true; // Dizemos que esta é a primeira vez
        }
        lastPressedTime = Time.time;
      }
    }

    private void LateUpdate() {
      /*
       * Movements
       */
      FlyMode();
      AutoMove();
      MoveWASD();
    }

    #region Fly

    void FlyMode() {
      if (!isFlying) return;

      xDeg += Input.GetAxis("Mouse X") * rotation * Time.deltaTime;
      yDeg -= Input.GetAxis("Mouse Y") * rotation * Time.deltaTime;

      transform.rotation = Quaternion.Euler(yDeg, xDeg, 0);
      transform.position += transform.forward * flySpeed * Time.deltaTime;
      yDeg = Util.ClampAngle(yDeg);
    }

    private void EnabledCursor() {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
    }

    private void DisabledCursor() {
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion

    #region MoveWASD

    private void MoveWASD() {
      if (isAutoMove || isFlying) return;

      if (isGrounded) {
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

        if (Input.GetKeyDown(KeyCode.Space)) {
          rdb.AddForce(0, jump, 0, ForceMode.Impulse);
          isJumping = true;
        }
      }

      transform.rotation = Quaternion.Euler(0, xDeg, 0);
      transform.Translate(moveDirection * Time.deltaTime);
    }

    #endregion

    #region AutoMoveToClick

    private void AutoMove() {
      if (!isAutoMove || IsPointerOverUIObject()) return;

      if (Input.GetKeyDown(KeyCode.Mouse0)) {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, maxGroundRaycastDistance, GroundLayers)) {
          isAutoMove = false;
          return;
        }

        destination = hit.point;
        if (Vector3.Distance(transform.position, destination) < minimumPathDistance) {
          isAutoMove = false;
          return;
        }

        SpawnGroundPathMarker(destination);
      }

    float distance = Vector3.Distance(transform.position, destination);

      if (distance > 1.3f) {
        Vector3 markerSpawnTarget = transform.InverseTransformPoint(markerSpawnPosition);

        float angle = Mathf.Atan2(markerSpawnTarget.x, markerSpawnTarget.z) * Mathf.Rad2Deg;
        // Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, angle, 0) * runRotation * Time.deltaTime);
        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, angle, 0));

        transform.rotation = transform.rotation *  deltaRotation;
        xDeg = transform.eulerAngles.y;
      }

      if (distance > 1.1f) transform.position = transform.position + transform.forward * runSpeed * Time.deltaTime;
      else isAutoMove = false;
    }

    private void SpawnGroundPathMarker(Vector3 point) {
      GameObject prefab = GroundPathPrefab;
      GameObject marker = Instantiate(
        prefab,
        new Vector3(point.x + markerPositionOffset.x, point.y + markerPositionOffset.y, point.z + markerPositionOffset.z),
        prefab.transform.rotation
      );
      markerSpawnPosition = marker.transform.position;
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

    private void OnCollisionEnter(Collision collision) {
      if (collision.gameObject.layer != 6) isAutoMove = false;
      if (collision.gameObject.layer == 6) {
        EnabledCursor();
        rdb.useGravity = true;
        isFlying = false;
        isJumping = false;
      }
    }

    #endregion

    // private void OnDrawGizmosSelected() {
    //   Gizmos.color = Color.red;
    //   Gizmos.DrawWireSphere(transform.position, lookRadius);
    // }

  }
}

