using UnityEngine;

namespace UnityMediaRecorder.Utils {
  public static class UnityUtils {
    public static T GetOrAddComponent<T>(this MonoBehaviour mb) where T : Component {
      T component = mb.GetComponent<T>();
      if (component == null) {
        component = mb.gameObject.AddComponent<T>();
      }
      return component;
    }
  }
}
