using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementRPG.Controller
{
  public class PlayerCamera : MonoBehaviour
  {
    public Transform target;
    public PlayerController _player;
    public float targetHeight = 1.2f;
    public float distance = 12.0f;
    public float maxDistance = 20;
    public float minDistance = 5.6f;
    public float rotationSpeed = 200.0f;
    public float zoomRate = 40;
    public float zoomDampening = 5.0f;
    LayerMask collisionLayers = -1;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    public float desiredDistance;
    private float correctedDistance;
    private bool rotateBehind;

    public bool isFixedCamera = true;

    private void Awake() {
      // if (!target) target = GameObject.Find("Player").transform;
      _player = FindObjectOfType<PlayerController>();
      if (!target) target = _player.transform;
    }

    private void Start() {
      xDeg = transform.eulerAngles.x;
      yDeg = transform.eulerAngles.y;
      currentDistance = distance; 
      desiredDistance = distance; 
      correctedDistance = distance;
      
      if (_player == null) _player = GetComponent<PlayerController>();

      if (GetComponent<Rigidbody>()) GetComponent<Rigidbody>().freezeRotation = true;
    }

    private void Update() {
      if (!target) return;

      if (_player.isFlying) {
        xDeg = target.transform.eulerAngles.y;
        yDeg = target.transform.eulerAngles.x;
      } else {
        CameraLogic();
        rotationCamera();
        yDeg = Util.ClampAngle(yDeg);
      }
    }

    private void LateUpdate() {
      Quaternion rotation = Quaternion.Euler(yDeg, xDeg, 0);

      desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance); 
      desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance); 
      correctedDistance = desiredDistance;

      Vector3 vTargetOffset = new Vector3 (0, -targetHeight, 0);
      Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + vTargetOffset);

      RaycastHit collisionHit; 
      Vector3 trueTargetPosition = new Vector3 (target.position.x, target.position.y + targetHeight, target.position.z); 

      bool isCorrected = false; 
      if (Physics.Linecast(trueTargetPosition, position,out collisionHit, collisionLayers)) {
        correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - 0.1f;
        isCorrected = true;
      }

      if (!isCorrected || correctedDistance > currentDistance) {
        currentDistance = Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening);
      } else {
        currentDistance = correctedDistance;
      }

      currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance); 

      position = target.position - (rotation * Vector3.forward * currentDistance + vTargetOffset);

      transform.rotation = rotation;
      transform.position = position;
    }

    private void CameraLogic() {
      if (Input.GetKey(KeyCode.UpArrow)) yDeg += +1 * rotationSpeed * 0.02f * 0.5f;
      if (Input.GetKey(KeyCode.DownArrow)) yDeg += -1 * rotationSpeed * 0.02f * 0.5f;
      
      if (!isFixedCamera) return;
      if (
        (Input.GetKey(KeyCode.W)
        || Input.GetKey(KeyCode.S)
        || Input.GetKey(KeyCode.A)
        || Input.GetKey(KeyCode.D))
        && !Input.GetMouseButton(1)
      ) {
        xDeg = target.transform.eulerAngles.y;
      } else {
        if (Input.GetMouseButton(1)) {
          xDeg += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
          yDeg -= Input.GetAxis("Mouse Y") * rotationSpeed * 0.02f;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) xDeg -= -1 * rotationSpeed * 0.02f * 0.5f;
        if (Input.GetKey(KeyCode.RightArrow)) xDeg -= +1 * rotationSpeed * 0.02f * 0.5f;
      }
    }

    private void rotationCamera() {
      if (isFixedCamera) return;
      if (Input.GetMouseButton(1)) {
        xDeg += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
        yDeg -= Input.GetAxis("Mouse Y") * rotationSpeed * 0.02f;
      }
      if (Input.GetKey(KeyCode.LeftArrow)) xDeg -= -1 * rotationSpeed * 0.02f * 0.5f;
      if (Input.GetKey(KeyCode.RightArrow)) xDeg -= +1 * rotationSpeed * 0.02f * 0.5f;
    }
  }
}