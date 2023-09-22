using UnityEngine;

namespace MovementRPG.Controller
{
  public static class Util
  {
    public static float ClampAngle(float angle)
    {
      if (angle < -360) angle += 360;
      if (angle > 360) angle -= 360;
      return  Mathf.Clamp(angle, -50, 50);
    }

    public static void DumpToConsole(object obj)
    {
      Debug.Log(JsonUtility.ToJson(obj, true));
    }
  }
}
