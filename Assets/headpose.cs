using UnityEngine;
using System.IO;
using Meta.XR.MRUtilityKit;

public class DataCollector : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer passthroughLayer;
    private StreamWriter dataWriter;
    private bool recording = false;
    private float recordingStartTime;
    private int sessionCount = 0;
    private string currentFilePath;

    void Update()
    {
        // Press A to toggle recording
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (recording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        if (recording)
        {
            LogFrame();
        }
    }

    void StartRecording()
    {
        recording = true;
        recordingStartTime = Time.time;
        sessionCount++;
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        currentFilePath = Path.Combine(Application.persistentDataPath, $"tracking_data_session{sessionCount}_{timestamp}.csv");
        
        dataWriter = new StreamWriter(currentFilePath);
        dataWriter.WriteLine("frame,time,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z,rot_w,vel_x,vel_y,vel_z");
        
        Debug.Log($"🔴 Recording STARTED - Session {sessionCount}");
        Debug.Log($"Saving to: {currentFilePath}");
        
        // Haptic feedback - short vibration
        OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.RTouch);
        Invoke("StopHaptics", 0.2f);
    }

    void StopRecording()
    {
        recording = false;
        
        if (dataWriter != null)
        {
            dataWriter.Flush();
            dataWriter.Close();
            dataWriter = null;
        }
        
        float duration = Time.time - recordingStartTime;
        int frameCount = Time.frameCount;
        
        Debug.Log($"⏹️ Recording STOPPED - Session {sessionCount}");
        Debug.Log($"Duration: {duration:F2}s");
        Debug.Log($"File saved: {currentFilePath}");
        
        // Haptic feedback - double pulse to indicate stop
        OVRInput.SetControllerVibration(0.3f, 0.3f, OVRInput.Controller.RTouch);
        Invoke("StopHaptics", 0.1f);
        Invoke("StartSecondPulse", 0.2f);
    }

    void StartSecondPulse()
    {
        OVRInput.SetControllerVibration(0.3f, 0.3f, OVRInput.Controller.RTouch);
        Invoke("StopHaptics", 0.1f);
    }

    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    void LogFrame()
    {
        Transform centerEye = OVRManager.instance.GetComponent<OVRCameraRig>().centerEyeAnchor;
        
        Vector3 pos = centerEye.position;
        Quaternion rot = centerEye.rotation;
        Vector3 velocity = OVRManager.instance.headPoseRelativeOffsetTranslation / Time.deltaTime;
        
        float time = Time.time - recordingStartTime;
        int frame = Time.frameCount;
        
        dataWriter.WriteLine($"{frame},{time:F6},{pos.x:F6},{pos.y:F6},{pos.z:F6},{rot.x:F6},{rot.y:F6},{rot.z:F6},{rot.w:F6},{velocity.x:F6},{velocity.y:F6},{velocity.z:F6}");
    }

    void OnApplicationQuit()
    {
        if (recording)
        {
            StopRecording();
        }
    }

    void OnDestroy()
    {
        dataWriter?.Close();
    }
}
