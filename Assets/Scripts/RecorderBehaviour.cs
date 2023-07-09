using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityMediaRecorder.Utils;

namespace UnityMediaRecorder {
  public enum RecorderState {
    INVALID = 0,
    IDLE,
    INITIALIZING,
    RECORDING,
    CLEANING_UP
  }

  public class RecorderBehaviour : MonoBehaviour {
    private class VideoData {
      public byte[] pixels;
      public int width;
      public int height;

      public readonly long timestamp;

      public VideoData(long timestamp) {
        this.timestamp = timestamp;
      }
    }

    private class AudioData {
      public readonly float[] samples;
      public readonly int channels;
      public readonly int sampleRate;

      public AudioData(float[] samples, int channels, int sampleRate) {
        this.samples = samples;
        this.channels = channels;
        this.sampleRate = sampleRate;
      }
    }

    // Unity references
    private Camera cam_;
    private AudioListener mainAudioListener_;
    private AudioListener recAudioListener_;

    // for video encoding
    private ConcurrentQueue<VideoData> vQueue_;
    private Task vTask_;

    // for audio encoding
    private int sampleRate_;
    private ConcurrentQueue<AudioData> aQueue_;
    private Task aTask_;

    private Recording rec_;
    private RecorderState state_;
    private readonly object lock_ = new object(); // lock used to access state

    public RecorderState State {
      get {
        lock (lock_) {
          return state_;
        }
      }
    }

    private void Awake() {
      cam_ = this.GetOrAddComponent<Camera>();
      mainAudioListener_ = FindObjectOfType<AudioListener>();
      recAudioListener_ = this.GetOrAddComponent<AudioListener>();
      if (mainAudioListener_ == null) {
        mainAudioListener_ = recAudioListener_;
      }
      if (mainAudioListener_ != recAudioListener_) {
        recAudioListener_.enabled = false;
      }
      state_ = RecorderState.IDLE;
    }

    // runs on the audio thread
    private void OnAudioFilterRead(float[] samples, int channels) {
      lock (lock_) {
        if (state_ == RecorderState.RECORDING && rec_.IsAudioEnabled) {
          float[] samplesCopy = samples.ToArray();
          aQueue_.Enqueue(new AudioData(samplesCopy, channels, sampleRate_));
          if (aTask_.IsCompleted) {
            aTask_ = UpdateAudio();
          }
        }
      }
    }

    private void OnPostRender() {
      lock (lock_) {
        if (state_ == RecorderState.RECORDING && rec_.IsVideoEnabled && rec_.CanEncodeVideoFrame(out long timestamp)) {
          VideoData data = new VideoData(timestamp);
          vQueue_.Enqueue(data);
          AsyncGPUReadback.Request(
            cam_.activeTexture,
            0,
            TextureFormat.RGBA32,
            req => {
              if (req.hasError) {
                throw new UnityException("async GPU readback failed");
              }

              data.pixels = req.GetData<byte>().ToArray();
              data.width = req.width;
              data.height = req.height;
            });
          if (vTask_.IsCompleted) {
            vTask_ = UpdateVideo();
          }
        }
      }
    }

    private void OnDestroy() {
      StopRecording().HandleAsyncExceptions();
    }

    public async Task<bool> StartRecording(RecordingOptions options) {
      lock (lock_) {
        if (state_ != RecorderState.IDLE) {
          return false;
        }

        state_ = RecorderState.INITIALIZING;
      }

      // no longer need lock in this method at this point
      // all other critical sections will fail with state_ == RecorderState.INITIALIZING

      if (options.vParams != null) {
        vQueue_ = new ConcurrentQueue<VideoData>();
        vTask_ = Task.CompletedTask;
      }

      if (options.aParams != null) {
        ToggleAudioListeners();
        sampleRate_ = AudioSettings.outputSampleRate;
        aQueue_ = new ConcurrentQueue<AudioData>();
        aTask_ = Task.CompletedTask;
      }

      await Task.Run(() => {
        rec_ = new Recording(options);
      });
      state_ = RecorderState.RECORDING;
      return true;
    }

    public async Task<bool> StopRecording() {
      lock (lock_) {
        if (state_ != RecorderState.RECORDING) {
          return false;
        }

        state_ = RecorderState.CLEANING_UP;
      }

      // no longer need lock in this method at this point
      // all other critical sections will fail with state_ == RecorderState.CLEANING_UP

      if (rec_.IsVideoEnabled) {
        await vTask_; // ensure UpdateVideo tasks are completed
        await UpdateVideo(); // clear out anything in the queue that might still be there
        vQueue_ = null;
        vTask_ = null;
      }

      if (rec_.IsAudioEnabled) {
        await aTask_; // ensure UpdateAudio tasks are completed
        await UpdateAudio(); // clear out anything in the queue that we need to catch the audio stream up to the video stream
        ToggleAudioListeners();
        sampleRate_ = 0;
        aQueue_ = null;
        aTask_ = null;
      }

      await Task.Run(() => {
        rec_.CleanUp();
      });
      rec_ = null;
      state_ = RecorderState.IDLE;
      return true;
    }

    private Task UpdateVideo() {
      return Task.Run(async () => {
        while (vQueue_.TryDequeue(out VideoData data)) {
          await AsyncUtils.WaitUntil(() => data.pixels != null);
          rec_.EncodeVideoFrame(data.pixels, data.width, data.height, data.timestamp);
        }
      });
    }

    private Task UpdateAudio() {
      return Task.Run(() => {
        while (rec_.CanEncodeAudioFrame() && aQueue_.TryDequeue(out AudioData data)) {
          rec_.EncodeAudioFrame(data.samples, data.channels, data.sampleRate);
        }
      });
    }

    private void ToggleAudioListeners() {
      if (mainAudioListener_ == null || recAudioListener_ == null || mainAudioListener_ == recAudioListener_) {
        return;
      }

      mainAudioListener_.enabled = !mainAudioListener_.enabled;
      recAudioListener_.enabled = !recAudioListener_.enabled;
    }
  }
}
