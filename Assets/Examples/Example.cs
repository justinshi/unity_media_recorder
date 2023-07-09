using System;
using System.IO;
using UnityEngine;
using UnityMediaRecorder.Utils;

namespace UnityMediaRecorder.Examples {
  public class Example : MonoBehaviour {
    private RecorderBehaviour recorder_;

    private void Awake() {
      recorder_ = this.GetOrAddComponent<RecorderBehaviour>();
    }

    // this example will move and tilt back and forth along the X axis
    // press space bar to start/stop an audio + video recording
    // the recording will be saved as an mp4 file to the My Videos folder
    // file will have video start Unixtime in seconds as name
    private void Update() {
      if (Input.GetKeyDown(KeyCode.Space)) {
        if (recorder_.State == RecorderState.IDLE) {
          VideoParams vParams = new VideoParams(30, 4000000, 512, 512, "vflip");
          AudioParams aParams = new AudioParams(AudioSettings.outputSampleRate, 320000, string.Empty, AudioSettings.speakerMode);
          string filename = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
          RecordingOptions recOpts = new RecordingOptions(filename, vParams, aParams);
          recorder_.StartRecording(recOpts).HandleAsyncExceptions();
        } else if (recorder_.State == RecorderState.RECORDING) {
          recorder_.StopRecording().HandleAsyncExceptions();
        }
      }

      float sine = Mathf.Sin(0.5f * Mathf.PI * Time.time);
      transform.position = new Vector3(5f * sine, 0f, 0f);
      transform.rotation = Quaternion.Euler(20f * sine, 0f, 0f);
    }
  }
}
