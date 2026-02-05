using UnityEngine;
using System.IO;
using Meta.XR.MRUtilityKit;

public class DataCollector : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer passthroughLayer;
    private StreamWriter dataWriter;
    private bool recording = false;
    private float recordingStartTime;

    void Start()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string path = Path.Combine(Application.persistentDataPath, $"tracking_data_{timestamp}.csv");
        
        dataWriter = new StreamWriter(path);
        dataWriter.WriteLine("frame,time,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z,rot_w,vel_x,vel_y,vel_z");
        
        Debug.Log($"Data will be saved to: {path}");
    }

    void Update()
    {
        // Press A to toggle recording
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            recording = !recording;
            if (recording) recordingStartTime = Time.time;
            Debug.Log($"Recording: {recording}");
        }

        if (recording)
        {
            LogFrame();
        }
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
        dataWriter?.Close();
    }
}