using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DEDZEC.Controller {
  [RequireComponent(typeof(Rigidbody))]
  public class Move : MonoBehaviour {
    // REFERENCES
    private Rigidbody rb;

    // PHYSICS
    public float jump = 7.0f;
    public float speed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotation = 100f;
    private float xDeg = 0.0f;

    // NAVIGATION
    public bool isFlying;
    public bool isAutoMove;
    public bool isGrounded;
    public bool isJumping;
    private string moveStatus = "idle";
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 destination;
    public float maxGroundRaycastDistance = 100;
    public float minimumPathDistance = 1f;
    private GameObject groundCheck;
    [SerializeField] LayerMask ground = 6; // 6: Ground

    // INPUT FEEDBACK
    public GameObject GroundPathPrefab;
    public float groundMarkerDuration = 2;
    public Vector3 markerPositionOffset = new Vector3(0, 0.1f, 0);
    public Vector3 markerSpawnPosition = new Vector3(0, 0, 0);

    public float lookRadius = 1.5f;

    // CHECK DOUBLE PRESS
    private float delayBetweenPresses = 0.25f;
    private bool pressedFirstTime = false;
    private float lastPressedTime;

    void Awake() {
      rb = GetComponent<Rigidbody>();
    }

    void Start() {
      if (rb) rb.freezeRotation = true;
    }

    void Update() {
      if (!GroundPathPrefab) return;
      isGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), .1f, ground);

      if (!isFlying) {
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

      if (Input.GetKeyDown(KeyCode.Space)) {
        if (pressedFirstTime) { // Já pressionamos o botão uma primeira vez, verificamos se a 2ª vez é rápida o suficiente para ser considerada um pressionamento duplo
          bool isDoublePress = Time.time - lastPressedTime <= delayBetweenPresses;
          if (isDoublePress) {
            Debug.Log("check for ammo");
            rb.useGravity = false;
            pressedFirstTime = false;
            isJumping = false;
            isFlying = true;
          }
        } else { // Ainda não pressionamos o botão pela primeira vez
          pressedFirstTime = true; // Dizemos que esta é a primeira vez
        }
        lastPressedTime = Time.time;
      } 
 
      // Estamos esperando um segundo pressionamento de tecla, mas atingimos o atraso, não podemos mais considerá-lo um pressionamento duplo
      if (pressedFirstTime && Time.time - lastPressedTime > delayBetweenPresses) {
        // Observe que, verificando primeiro a primeira vez pressionada na condição acima, fazemos o programa pular a próxima parte da condição se ela não for verdadeira,
        // Assim, evitamos a "computação pesada" (a subtração e a comparação) na maioria das vezes.
        // Também estamos nos certificando de que pressionamos a tecla uma primeira vez antes de fazer o cálculo, o que evita fazer o cálculo enquanto lastPressedTime ainda não foi inicializado
        Debug.Log("reload");
        pressedFirstTime = false;
      }
    }

    void LateUpdate() {
      FlyMode();
      AutoMove();
      MoveWASD();
    }

    private void FlyMode() {
      if (!isFlying) return;

      transform.rotation = Quaternion.Euler(0, -Input.GetAxis("Mouse X") * rotation * 0.2f, 0);

      if (Input.GetKey(KeyCode.LeftShift)) transform.position += transform.forward * runSpeed * Time.deltaTime * 10.0f; 
      else transform.position += transform.forward * speed * Time.deltaTime * 10.0f;
    }

    private void MoveWASD() {
      if (isAutoMove) return;

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
          rb.AddForce(0, jump, 0, ForceMode.Impulse);
          isJumping = true;
        }
      }

      transform.rotation = Quaternion.Euler(0, xDeg, 0);
      transform.Translate(moveDirection * Time.deltaTime);
    }

    private void AutoMove() {
      if (!isAutoMove || IsPointerOverUIObject()) return;

      if (Input.GetKeyDown(KeyCode.Mouse0)) {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, maxGroundRaycastDistance, ground) ) {
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
        rb.useGravity = true;
        isFlying = false;
        isJumping = false;
      }
    }

    private void OnDrawGizmosSelected() {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
  }
}
