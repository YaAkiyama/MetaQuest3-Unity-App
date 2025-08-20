using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace MQ3VRApp.Player
{
    /// <summary>
    /// VRプレイヤーコントローラー（修正版）
    /// 固定位置での360度ビューアー用（移動無効、回転のみ可能）
    /// </summary>
    public class VRPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private bool allowMovement = false;
        [SerializeField] private bool allowRotation = true;
        [SerializeField] private bool allowTeleportation = false;
        
        [Header("Player References")]
        [SerializeField] private Transform xrOrigin;
        [SerializeField] private Transform cameraOffset;
        [SerializeField] private Camera vrCamera;
        
        [Header("Initial Position")]
        [SerializeField] private Vector3 fixedPosition = Vector3.zero;
        [SerializeField] private float playerHeight = 1.6f;
        
        // プライベート変数（XROriginを使用しない簡略化版）
        private Transform xrOriginTransform;
        private ContinuousMoveProvider moveProvider;
        private SnapTurnProvider snapTurnProvider;
        private Vector3 initialPosition;
        
        private void Awake()
        {
            InitializeComponents();
            SetupPlayer();
        }
        
        private void InitializeComponents()
        {
            // XR Origin Transformの設定（XROriginコンポーネントは使用しない）
            xrOriginTransform = xrOrigin != null ? xrOrigin : transform;
            
            // Camera Offsetの設定
            if (cameraOffset == null)
            {
                Transform offset = transform.Find("Camera Offset");
                if (offset != null)
                {
                    cameraOffset = offset;
                }
            }
            
            // VRカメラの取得
            if (vrCamera == null)
            {
                vrCamera = GetComponentInChildren<Camera>();
            }
            
            // 移動プロバイダーの取得
            moveProvider = GetComponent<ContinuousMoveProvider>();
            snapTurnProvider = GetComponent<SnapTurnProvider>();
        }
        
        private void SetupPlayer()
        {
            // 初期位置の設定（VRトラッキングを妨げないよう設定のみ保存）
            initialPosition = fixedPosition;
            initialPosition.y = playerHeight;
            // transform.position = initialPosition; // VRヘッドトラッキング有効化のため無効化
            
            // 移動の無効化
            DisableMovement();
            
            // 回転設定
            if (!allowRotation && snapTurnProvider != null)
            {
                snapTurnProvider.enabled = false;
            }
        }
        
        private void DisableMovement()
        {
            // 連続移動の無効化
            if (moveProvider != null)
            {
                moveProvider.enabled = false;
            }
            
            // Rigidbodyがある場合は固定
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezePosition;
            }
        }
        
        private void LateUpdate()
        {
            // VRヘッドトラッキングを妨げないよう、XR Originの位置固定は行わない
            // VRヘッドセットによる位置・回転の変更を許可
        }
        
        /// <summary>
        /// プレイヤーをリセット位置に戻す
        /// </summary>
        public void ResetPlayerPosition()
        {
            transform.position = initialPosition;
            // カメラの回転はVRヘッドセットに任せるため、リセットしない
        }
        
        /// <summary>
        /// 位置固定の有効/無効を切り替え
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            allowMovement = enabled;
            
            if (moveProvider != null)
            {
                moveProvider.enabled = enabled;
            }
        }
        
        /// <summary>
        /// VRカメラの取得
        /// </summary>
        public Camera GetVRCamera()
        {
            return vrCamera;
        }
        
        /// <summary>
        /// プレイヤーの高さ調整
        /// </summary>
        public void SetPlayerHeight(float height)
        {
            playerHeight = height;
            initialPosition.y = height;
            ResetPlayerPosition();
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 pos = Application.isPlaying ? initialPosition : fixedPosition;
            pos.y = Application.isPlaying ? pos.y : playerHeight;
            Gizmos.DrawWireSphere(pos, 0.3f);
            Gizmos.DrawLine(pos, pos + Vector3.up * 0.5f);
        }
    }
}