using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace MQ3VRApp
{
    /// <summary>
    /// VRアプリケーション全体を管理するマネージャークラス
    /// </summary>
    public class VRManager : MonoBehaviour
    {
        public static VRManager Instance { get; private set; }

        [Header("VR Settings")]
        [SerializeField] private bool enableHandTracking = true;
        [SerializeField] private bool enableControllers = true;
        [SerializeField] private float playerHeight = 1.8f;

        [Header("XR References")]
        [SerializeField] private GameObject xrOrigin;
        [SerializeField] private Camera vrCamera;
        
        [Header("Hand Tracking")]
        [SerializeField] private GameObject leftHandModel;
        [SerializeField] private GameObject rightHandModel;
        
        [Header("Controllers")]
        [SerializeField] private GameObject leftController;
        [SerializeField] private GameObject rightController;

        private List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
        
        private void Awake()
        {
            // シングルトンパターンの実装
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeVR();
            SetupXROrigin();
            ConfigureInputMode();
        }

        private void InitializeVR()
        {
            UnityEngine.Debug.Log("Initializing VR System...");
            
            // XRデバイスの初期化
            SubsystemManager.GetInstances(inputSubsystems);
            
            foreach (var subsystem in inputSubsystems)
            {
                subsystem.Start();
                UnityEngine.Debug.Log($"Started XR Input Subsystem: {subsystem.GetType().Name}");
            }

            // VRカメラの設定
            if (vrCamera == null && xrOrigin != null)
            {
                vrCamera = xrOrigin.GetComponentInChildren<Camera>();
            }
            
            if (vrCamera != null)
            {
                vrCamera.nearClipPlane = 0.01f;
                vrCamera.farClipPlane = 1000f;
            }
        }

        private void SetupXROrigin()
        {
            if (xrOrigin == null)
            {
                // XR Originが指定されていない場合、シーンから検索
                xrOrigin = GameObject.Find("XR Origin");
                
                if (xrOrigin == null)
                {
                    UnityEngine.Debug.LogWarning("XR Origin not found. Creating new XR Origin...");
                    CreateXROrigin();
                }
            }
            
            // プレイヤーの高さを設定
            if (xrOrigin != null)
            {
                xrOrigin.transform.position = new Vector3(0, 0, 0);
            }
        }

        private void CreateXROrigin()
        {
            // XR Originを動的に作成
            xrOrigin = new GameObject("XR Origin");
            xrOrigin.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            
            // カメラオフセットの作成
            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOrigin.transform);
            cameraOffset.transform.localPosition = new Vector3(0, playerHeight, 0);
            
            // VRカメラの作成
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.transform.SetParent(cameraOffset.transform);
            cameraGO.transform.localPosition = Vector3.zero;
            
            vrCamera = cameraGO.AddComponent<Camera>();
            vrCamera.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();
            
            // TrackedPoseDriverの追加（カメラトラッキング用）
            var poseDriver = cameraGO.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            poseDriver.positionAction = new UnityEngine.InputSystem.InputAction(binding: "<XRHMD>/centerEyePosition");
            poseDriver.rotationAction = new UnityEngine.InputSystem.InputAction(binding: "<XRHMD>/centerEyeRotation");
        }

        private void ConfigureInputMode()
        {
            // ハンドトラッキングとコントローラーの切り替え設定
            if (enableHandTracking && leftHandModel != null && rightHandModel != null)
            {
                leftHandModel.SetActive(true);
                rightHandModel.SetActive(true);
                UnityEngine.Debug.Log("Hand tracking enabled");
            }
            
            if (enableControllers && leftController != null && rightController != null)
            {
                leftController.SetActive(!enableHandTracking);
                rightController.SetActive(!enableHandTracking);
                UnityEngine.Debug.Log($"Controllers {(enableHandTracking ? "disabled" : "enabled")}");
            }
        }

        public void SwitchToHandTracking()
        {
            enableHandTracking = true;
            enableControllers = false;
            ConfigureInputMode();
        }

        public void SwitchToControllers()
        {
            enableHandTracking = false;
            enableControllers = true;
            ConfigureInputMode();
        }

        public void RecenterView()
        {
            // ビューの再センタリング
            UnityEngine.XR.InputTracking.Recenter();
            UnityEngine.Debug.Log("View recentered");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                // アプリケーションが再開された時の処理
                RecenterView();
            }
        }

        private void OnDestroy()
        {
            // XRサブシステムのクリーンアップ
            foreach (var subsystem in inputSubsystems)
            {
                if (subsystem.running)
                {
                    subsystem.Stop();
                }
            }
        }
    }
}
