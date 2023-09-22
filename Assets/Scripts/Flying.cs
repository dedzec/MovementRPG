using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flying : MonoBehaviour {
  // REFERENCES
  private Rigidbody rb;

  // PHYSICS
  public float flySpeed = 20f;
  public float rotation = 100f;
  private float xDeg = 0.0f;
  private float yDeg = 0.0f;

  public bool isFlying = true;
  // public bool IsFlying {
  //   get { return isFlying; }
  // }

  void Awake() {
    rb = GetComponent<Rigidbody>();
  }

  void Start() {
    DisabledCursor();
    rb.freezeRotation = true;
    rb.useGravity = false;
  }

  void Update() {
    if (Input.GetKey(KeyCode.F)) EnabledCursor();
    if (Input.GetKey(KeyCode.G)) DisabledCursor();

    xDeg += Input.GetAxis("Mouse X") * rotation * Time.deltaTime;
    yDeg -= Input.GetAxis("Mouse Y") * rotation * Time.deltaTime;

    transform.rotation = Quaternion.Euler(yDeg, xDeg, 0);
    transform.position += transform.forward * flySpeed * Time.deltaTime;

    ClampAngle(yDeg);
  }

  private void EnabledCursor() {
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
  }

  private void DisabledCursor() {
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
  }

  private void ClampAngle (float angle) {
    if (angle < -360) angle += 360;
    if (angle > 360) angle -= 360;

    yDeg = Mathf.Clamp(angle, -50, 50);
  }
}
