using UnityEngine;
using TMPro;

namespace MQ3VRApp.Debug
{
    public class VRTrackingDebug : MonoBehaviour
    {
        [Header("Debug Display")]
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private bool showDebugInfo = true;
        
        private Camera vrCamera;
        private Transform xrOrigin;
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;
        
        private void Start()
        {
            vrCamera = Camera.main;
            if (vrCamera == null)
            {
                vrCamera = FindObjectOfType<Camera>();
            }
            
            // XR Originを探す
            GameObject xrOriginGO = GameObject.Find("XR Origin");
            if (xrOriginGO != null)
            {
                xrOrigin = xrOriginGO.transform;
            }
            
            if (vrCamera != null)
            {
                lastCameraPosition = vrCamera.transform.position;
                lastCameraRotation = vrCamera.transform.rotation;
            }
            
            // デバッグテキストを作成
            if (debugText == null && showDebugInfo)
            {
                CreateDebugText();
            }
        }
        
        private void CreateDebugText()
        {
            GameObject textGO = new GameObject("VR Debug Text");
            textGO.transform.SetParent(transform);
            
            debugText = textGO.AddComponent<TextMeshProUGUI>();
            debugText.text = "VR Tracking Debug";
            debugText.fontSize = 18;
            debugText.color = Color.green;
            debugText.alignment = TextAlignmentOptions.TopLeft;
            
            RectTransform textRect = debugText.GetComponent<RectTransform>();
            textRect.localPosition = new Vector3(-1f, 0.5f, 0);
            textRect.sizeDelta = new Vector2(400, 200);
        }
        
        private void Update()
        {
            if (!showDebugInfo || debugText == null || vrCamera == null)
                return;
            
            // 現在の位置と回転
            Vector3 currentPos = vrCamera.transform.position;
            Quaternion currentRot = vrCamera.transform.rotation;
            
            // 変化量を計算
            Vector3 positionDelta = currentPos - lastCameraPosition;
            float rotationDelta = Quaternion.Angle(currentRot, lastCameraRotation);
            
            // VR Origin の情報
            string xrOriginInfo = xrOrigin != null ? 
                $"XR Origin: {xrOrigin.position:F2}" : 
                "XR Origin: Not Found";
            
            // デバッグ情報を表示
            debugText.text = $@"VR Tracking Debug
Camera Pos: {currentPos:F2}
Camera Rot: {currentRot.eulerAngles:F1}
{xrOriginInfo}
Pos Delta: {positionDelta:F3}
Rot Delta: {rotationDelta:F1}°
Head Tracking: {(positionDelta.magnitude > 0.001f || rotationDelta > 0.1f ? "WORKING" : "STATIC")}
FPS: {1f / Time.unscaledDeltaTime:F0}";
            
            // 前フレームの値を保存
            lastCameraPosition = currentPos;
            lastCameraRotation = currentRot;
        }
    }
}