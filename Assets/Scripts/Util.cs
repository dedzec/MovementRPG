using UnityEngine;

namespace DEDZEC {
  public static class Util {
    public static Vector3 SetX(this Vector3 vec, float x) {
      return new Vector3(x, vec.y, vec.z);
    }

    public static Vector3 SetY(this Vector3 vec, float y) {
      return new Vector3(vec.x, y, vec.z);
    }

    public static Vector3 SetZ(this Vector3 vec, float z) {
      return new Vector3(vec.x, vec.y, z);
    }

    public static Vector3 Multiply(this Vector3 vec, float x, float y, float z) {
      return new Vector3(vec.x * x, vec.y * y, vec.z * z);
    }

    public static Vector3 Multiply(this Vector3 vec, Vector3 other) {
      return Multiply(vec, other.x, other.y, other.z);
    }

    public static float ClampAngle(float angle) {
      if (angle < -360) angle += 360;
      if (angle > 360) angle -= 360;
      return  Mathf.Clamp(angle, -50, 50);
    }

    public static void DumpToConsole(object obj) {
      Debug.Log(JsonUtility.ToJson(obj, true));
    }
  }
}
