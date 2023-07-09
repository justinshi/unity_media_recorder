using System;
using System.Threading.Tasks;

namespace UnityMediaRecorder.Utils {
  public static class AsyncUtils {
    public static Task WaitUntil(Func<bool> predicate) {
      return Task.Run(async () => {
        while (!predicate()) {
          await Task.Delay(10);
        }
      });
    }

    public static async void HandleAsyncExceptions(this Task task) {
      try {
        await task;
      } catch {
        throw;
      }
    }
  }
}
