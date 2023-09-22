using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour {
  // float delayBetweenPresses = 0.25f;
  // bool pressedFirstTime = false;
  // float lastPressedTime;
  public float forwardSpeed = 25f;
  public float strafeSpeed = 7.5f;
  public float houverSpeed = 5f;
  private float activeforwardSpeed;
  private float activeStrafeSpeed;
  private float activeHouverSpeed;
  public float forwardAcceleration = 2.5f;
  public float strafeAcceleration = 2f;
  public float houverAcceleration = 2f;

  public float lookRateSpeed = 90f;
  private Vector2 lookInput, screenCenter, mouseDistance;

  private float rollInput;
  public float rollSpeed = 90f;
  public float rollAcceleration = 3.5f;

  void Start() {
    screenCenter.x = Screen.width * .5f;
    screenCenter.y = Screen.height * .5f;

    // Cursor.lokState = CursorLockMode.confined;
  }

  void Update() {
    // if (Input.GetKeyDown(KeyCode.Space)) {
    //   if (pressedFirstTime) { // we've already pressed the button a first time, we check if the 2nd time is fast enough to be considered a double-press
    //     bool isDoublePress = Time.time - lastPressedTime <= delayBetweenPresses;

    //     if (isDoublePress) {
    //       Debug.Log("check for ammo");
    //       pressedFirstTime = false;
    //       isFly = !isFly;
    //     }
    //   } else { // we've not already pressed the button a first time
    //     pressedFirstTime = true; // we tell this is the first time
    //   }

    //   lastPressedTime = Time.time;
    // }

    // if (pressedFirstTime && Time.time - lastPressedTime > delayBetweenPresses) { // we're waiting for a 2nd key press but we've reached the delay, we can't consider it a double press anymore
    //   // note that by checking first for pressedFirstTime in the condition above, we make the program skip the next part of the condition if it's not true,
    //   // thus we're avoiding the "heavy computation" (the substraction and comparison) most of the time.
    //   // we're also making sure we've pressed the key a first time before doing the computation, which avoids doing the computation while lastPressedTime is still uninitialized
    //   Debug.Log("reload");
    //   pressedFirstTime = false;
    // }

    lookInput.x = Input.mousePosition.x;
    lookInput.y = Input.mousePosition.y;

    mouseDistance.x = (lookInput.x - screenCenter.x) / screenCenter.x;
    mouseDistance.y = (lookInput.y - screenCenter.y) / screenCenter.y;

    mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

    // rollInput = Mathf.Lerp(rollInput, Input.GetKey(KeyCode.Z), rollAcceleration * Time.deltaTime);
    
    // transform.Rotate(-mouseDistance.y * lookRateSpeed * Time.deltaTime, mouseDistance.x * lookRateSpeed * Time.deltaTime, 0f, Space.Self);
    transform.Rotate(0, Input.GetAxis("Horizontal"), 0);

    activeforwardSpeed = Mathf.Lerp(activeforwardSpeed, Input.GetAxis("Vertical") * forwardSpeed, forwardAcceleration * Time.deltaTime);
    activeStrafeSpeed = Mathf.Lerp(activeStrafeSpeed, Input.GetAxis("Horizontal") * strafeSpeed, strafeAcceleration * Time.deltaTime);
    activeHouverSpeed = Mathf.Lerp(activeHouverSpeed, Input.GetAxis("Vertical") * houverSpeed, houverAcceleration * Time.deltaTime);

    transform.position += transform.forward * activeforwardSpeed * Time.deltaTime;
    transform.position += (transform.right * activeStrafeSpeed * Time.deltaTime) + (transform.up * activeHouverSpeed * Time.deltaTime);
  }

}
