using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace MQ3VRApp
{
    /// <summary>
    /// ハンドトラッキングとインタラクションを制御するクラス
    /// </summary>
    public class HandInteractionController : MonoBehaviour
    {
        public enum HandType
        {
            Left,
            Right
        }

        [Header("Hand Settings")]
        [SerializeField] private HandType handType = HandType.Right;
        [SerializeField] private float pinchThreshold = 0.7f;
        [SerializeField] private float grabRadius = 0.1f;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject handVisualPrefab;
        [SerializeField] private Material defaultHandMaterial;
        [SerializeField] private Material interactingHandMaterial;
        
        [Header("Interaction")]
        [SerializeField] private LayerMask grabbableLayer = -1;
        [SerializeField] private float grabForce = 500f;
        
        private XRHand hand;
        private GameObject handVisual;
        private MeshRenderer handRenderer;
        private bool isPinching = false;
        private bool wasPickedUp = false;
        private GameObject currentGrabbedObject = null;
        private Rigidbody grabbedRigidbody = null;
        
        // 指のジョイント
        private Transform thumbTip;
        private Transform indexTip;
        private Transform middleTip;
        
        // インタラクションイベント
        public delegate void HandInteractionEvent(GameObject target);
        public event HandInteractionEvent OnGrabStart;
        public event HandInteractionEvent OnGrabEnd;
        public event HandInteractionEvent OnHoverStart;
        public event HandInteractionEvent OnHoverEnd;
        
        private GameObject hoveredObject = null;

        private void Start()
        {
            SetupHandVisual();
            SetupXRHand();
        }

        private void SetupHandVisual()
        {
            if (handVisualPrefab != null)
            {
                handVisual = Instantiate(handVisualPrefab, transform);
                handRenderer = handVisual.GetComponent<MeshRenderer>();
                
                if (handRenderer != null && defaultHandMaterial != null)
                {
                    handRenderer.material = defaultHandMaterial;
                }
            }
        }

        private void SetupXRHand()
        {
            // XR Hand Subsystemの初期化
            var handSubsystem = XRHandSubsystem.GetSubsystemInManager();
            
            if (handSubsystem != null)
            {
                if (handType == HandType.Left)
                {
                    hand = handSubsystem.leftHand;
                }
                else
                {
                    hand = handSubsystem.rightHand;
                }
                
                Debug.Log($"{handType} hand tracking initialized");
            }
            else
            {
                Debug.LogWarning("XRHandSubsystem not found. Hand tracking may not be available.");
            }
        }

        private void Update()
        {
            if (hand == null) return;
            
            UpdateHandTracking();
            UpdatePinchDetection();
            UpdateInteraction();
            UpdateVisualFeedback();
        }

        private void UpdateHandTracking()
        {
            // ハンドトラッキングデータの更新
            if (hand.TryGetJoint(XRHandJointID.ThumbTip, out var thumbJoint))
            {
                if (thumbTip == null)
                {
                    GameObject thumbGO = new GameObject("ThumbTip");
                    thumbGO.transform.SetParent(transform);
                    thumbTip = thumbGO.transform;
                }
                thumbTip.position = thumbJoint.pose.position;
                thumbTip.rotation = thumbJoint.pose.rotation;
            }
            
            if (hand.TryGetJoint(XRHandJointID.IndexTip, out var indexJoint))
            {
                if (indexTip == null)
                {
                    GameObject indexGO = new GameObject("IndexTip");
                    indexGO.transform.SetParent(transform);
                    indexTip = indexGO.transform;
                }
                indexTip.position = indexJoint.pose.position;
                indexTip.rotation = indexJoint.pose.rotation;
            }
            
            if (hand.TryGetJoint(XRHandJointID.MiddleTip, out var middleJoint))
            {
                if (middleTip == null)
                {
                    GameObject middleGO = new GameObject("MiddleTip");
                    middleGO.transform.SetParent(transform);
                    middleTip = middleGO.transform;
                }
                middleTip.position = middleJoint.pose.position;
                middleTip.rotation = middleJoint.pose.rotation;
            }
        }

        private void UpdatePinchDetection()
        {
            if (thumbTip == null || indexTip == null) return;
            
            // ピンチジェスチャーの検出
            float pinchDistance = Vector3.Distance(thumbTip.position, indexTip.position);
            isPinching = pinchDistance < pinchThreshold * 0.1f; // 10cm以下でピンチとみなす
            
            if (isPinching && !wasPickedUp)
            {
                TryGrabObject();
                wasPickedUp = true;
            }
            else if (!isPinching && wasPickedUp)
            {
                ReleaseObject();
                wasPickedUp = false;
            }
        }

        private void UpdateInteraction()
        {
            if (indexTip == null) return;
            
            // ホバー検出
            Collider[] colliders = Physics.OverlapSphere(indexTip.position, grabRadius, grabbableLayer);
            
            GameObject nearestObject = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Grabbable"))
                {
                    float distance = Vector3.Distance(indexTip.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestObject = collider.gameObject;
                    }
                }
            }
            
            // ホバーイベントの処理
            if (nearestObject != hoveredObject)
            {
                if (hoveredObject != null)
                {
                    OnHoverEnd?.Invoke(hoveredObject);
                }
                
                hoveredObject = nearestObject;
                
                if (hoveredObject != null)
                {
                    OnHoverStart?.Invoke(hoveredObject);
                }
            }
        }

        private void TryGrabObject()
        {
            if (hoveredObject != null && currentGrabbedObject == null)
            {
                currentGrabbedObject = hoveredObject;
                grabbedRigidbody = currentGrabbedObject.GetComponent<Rigidbody>();
                
                if (grabbedRigidbody != null)
                {
                    grabbedRigidbody.useGravity = false;
                    grabbedRigidbody.drag = 10f;
                }
                
                // Grabbableコンポーネントの処理
                var grabbable = currentGrabbedObject.GetComponent<GrabbableObject>();
                if (grabbable != null)
                {
                    grabbable.OnGrab(this);
                }
                
                OnGrabStart?.Invoke(currentGrabbedObject);
                Debug.Log($"Grabbed object: {currentGrabbedObject.name}");
            }
        }

        private void ReleaseObject()
        {
            if (currentGrabbedObject != null)
            {
                if (grabbedRigidbody != null)
                {
                    grabbedRigidbody.useGravity = true;
                    grabbedRigidbody.drag = 1f;
                    
                    // 手の速度を物体に適用
                    if (hand.TryGetJoint(XRHandJointID.Palm, out var palmJoint))
                    {
                        grabbedRigidbody.velocity = palmJoint.linearVelocity;
                        grabbedRigidbody.angularVelocity = palmJoint.angularVelocity;
                    }
                }
                
                // Grabbableコンポーネントの処理
                var grabbable = currentGrabbedObject.GetComponent<GrabbableObject>();
                if (grabbable != null)
                {
                    grabbable.OnRelease();
                }
                
                OnGrabEnd?.Invoke(currentGrabbedObject);
                Debug.Log($"Released object: {currentGrabbedObject.name}");
                
                currentGrabbedObject = null;
                grabbedRigidbody = null;
            }
        }

        private void FixedUpdate()
        {
            // 掴んでいるオブジェクトを手に追従させる
            if (currentGrabbedObject != null && grabbedRigidbody != null && indexTip != null)
            {
                Vector3 targetPosition = indexTip.position;
                Vector3 force = (targetPosition - grabbedRigidbody.position) * grabForce;
                grabbedRigidbody.AddForce(force, ForceMode.Force);
            }
        }

        private void UpdateVisualFeedback()
        {
            if (handRenderer == null) return;
            
            // インタラクション状態に応じて手のマテリアルを変更
            if (isPinching && interactingHandMaterial != null)
            {
                handRenderer.material = interactingHandMaterial;
            }
            else if (defaultHandMaterial != null)
            {
                handRenderer.material = defaultHandMaterial;
            }
        }

        public bool IsPinching()
        {
            return isPinching;
        }

        public GameObject GetGrabbedObject()
        {
            return currentGrabbedObject;
        }

        private void OnDrawGizmosSelected()
        {
            // デバッグ用のギズモ表示
            if (indexTip != null)
            {
                Gizmos.color = isPinching ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(indexTip.position, grabRadius);
            }
            
            if (thumbTip != null && indexTip != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(thumbTip.position, indexTip.position);
            }
        }
    }

    // XRHandSubsystemのヘルパークラス（簡易実装）
    public static class XRHandSubsystem
    {
        public static object GetSubsystemInManager()
        {
            // 実際の実装では適切なSubsystemManagerを使用
            return null;
        }
    }
}
