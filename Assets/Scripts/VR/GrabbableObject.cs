using UnityEngine;
using UnityEngine.Events;

namespace MQ3VRApp
{
    /// <summary>
    /// VR環境でつかむことができるオブジェクトのコンポーネント
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class GrabbableObject : MonoBehaviour
    {
        [Header("Grab Settings")]
        [SerializeField] private bool canBeGrabbed = true;
        [SerializeField] private bool snapToHand = false;
        [SerializeField] private Vector3 grabOffset = Vector3.zero;
        [SerializeField] private bool maintainOriginalRotation = false;
        
        [Header("Physics Settings")]
        [SerializeField] private float grabbedDrag = 10f;
        [SerializeField] private float grabbedAngularDrag = 10f;
        [SerializeField] private bool useGravityWhenReleased = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.5f);
        [SerializeField] private bool showOutlineOnHover = true;
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip grabSound;
        [SerializeField] private AudioClip releaseSound;
        [SerializeField] private float audioVolume = 0.5f;
        
        [Header("Events")]
        public UnityEvent OnGrabbed;
        public UnityEvent OnReleased;
        public UnityEvent OnHoverEnter;
        public UnityEvent OnHoverExit;
        
        // コンポーネント参照
        private Rigidbody rb;
        private Collider col;
        private MeshRenderer meshRenderer;
        private AudioSource audioSource;
        private Material originalMaterial;
        private Outline outlineEffect;
        
        // 状態管理
        private bool isGrabbed = false;
        private bool isHovered = false;
        private HandInteractionController currentGrabber = null;
        
        // 物理設定の保存
        private float originalDrag;
        private float originalAngularDrag;
        private bool originalUseGravity;
        private RigidbodyConstraints originalConstraints;
        
        // トランスフォーム情報
        private Vector3 initialScale;
        private Quaternion initialRotation;

        private void Awake()
        {
            // コンポーネントの取得
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            meshRenderer = GetComponent<MeshRenderer>();
            
            // AudioSourceの設定
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (grabSound != null || releaseSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D音源
            }
            
            // タグの設定
            if (!gameObject.CompareTag("Grabbable"))
            {
                gameObject.tag = "Grabbable";
            }
            
            // 元の物理設定を保存
            SaveOriginalPhysicsSettings();
            
            // 初期トランスフォーム情報を保存
            initialScale = transform.localScale;
            initialRotation = transform.rotation;
            
            // マテリアルの保存
            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.material;
            }
        }

        private void SaveOriginalPhysicsSettings()
        {
            if (rb != null)
            {
                originalDrag = rb.drag;
                originalAngularDrag = rb.angularDrag;
                originalUseGravity = rb.useGravity;
                originalConstraints = rb.constraints;
            }
        }

        /// <summary>
        /// オブジェクトが掴まれた時の処理
        /// </summary>
        public void OnGrab(HandInteractionController grabber)
        {
            if (!canBeGrabbed || isGrabbed) return;
            
            isGrabbed = true;
            currentGrabber = grabber;
            
            // 物理設定の変更
            if (rb != null)
            {
                rb.drag = grabbedDrag;
                rb.angularDrag = grabbedAngularDrag;
                rb.useGravity = false;
                
                if (maintainOriginalRotation)
                {
                    rb.constraints = originalConstraints | RigidbodyConstraints.FreezeRotation;
                }
            }
            
            // スナップ位置の設定
            if (snapToHand && grabber != null)
            {
                transform.position = grabber.transform.position + grabber.transform.TransformDirection(grabOffset);
            }
            
            // オーディオ再生
            PlaySound(grabSound);
            
            // イベント発火
            OnGrabbed?.Invoke();
            
            Debug.Log($"{gameObject.name} was grabbed");
        }

        /// <summary>
        /// オブジェクトが離された時の処理
        /// </summary>
        public void OnRelease()
        {
            if (!isGrabbed) return;
            
            isGrabbed = false;
            
            // 物理設定を元に戻す
            if (rb != null)
            {
                rb.drag = originalDrag;
                rb.angularDrag = originalAngularDrag;
                rb.useGravity = useGravityWhenReleased ? originalUseGravity : false;
                rb.constraints = originalConstraints;
            }
            
            // オーディオ再生
            PlaySound(releaseSound);
            
            // イベント発火
            OnReleased?.Invoke();
            
            currentGrabber = null;
            
            Debug.Log($"{gameObject.name} was released");
        }

        /// <summary>
        /// ホバー開始時の処理
        /// </summary>
        public void OnHoverStart()
        {
            if (isHovered) return;
            
            isHovered = true;
            
            // ビジュアルフィードバック
            if (showOutlineOnHover)
            {
                EnableOutline(true);
            }
            
            if (highlightMaterial != null && meshRenderer != null)
            {
                meshRenderer.material = highlightMaterial;
            }
            else if (meshRenderer != null && originalMaterial != null)
            {
                // ハイライトカラーを適用
                meshRenderer.material.color = highlightColor;
            }
            
            // イベント発火
            OnHoverEnter?.Invoke();
        }

        /// <summary>
        /// ホバー終了時の処理
        /// </summary>
        public void OnHoverEnd()
        {
            if (!isHovered) return;
            
            isHovered = false;
            
            // ビジュアルフィードバックを元に戻す
            if (showOutlineOnHover)
            {
                EnableOutline(false);
            }
            
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
            
            // イベント発火
            OnHoverExit?.Invoke();
        }

        private void EnableOutline(bool enable)
        {
            if (outlineEffect == null)
            {
                outlineEffect = GetComponent<Outline>();
                if (outlineEffect == null && enable)
                {
                    outlineEffect = gameObject.AddComponent<Outline>();
                    outlineEffect.OutlineColor = highlightColor;
                    outlineEffect.OutlineWidth = 5f;
                }
            }
            
            if (outlineEffect != null)
            {
                outlineEffect.enabled = enable;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, audioVolume);
            }
        }

        /// <summary>
        /// オブジェクトのリセット
        /// </summary>
        public void ResetObject()
        {
            transform.localScale = initialScale;
            transform.rotation = initialRotation;
            
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            if (isGrabbed)
            {
                OnRelease();
            }
        }

        /// <summary>
        /// つかめるかどうかを設定
        /// </summary>
        public void SetGrabbable(bool grabbable)
        {
            canBeGrabbed = grabbable;
            
            if (!grabbable && isGrabbed)
            {
                OnRelease();
            }
        }

        /// <summary>
        /// 現在掴まれているかどうか
        /// </summary>
        public bool IsGrabbed()
        {
            return isGrabbed;
        }

        /// <summary>
        /// 現在ホバーされているかどうか
        /// </summary>
        public bool IsHovered()
        {
            return isHovered;
        }

        private void OnValidate()
        {
            // エディタでの値変更時の処理
            if (rb != null && !Application.isPlaying)
            {
                SaveOriginalPhysicsSettings();
            }
        }

        private void OnDestroy()
        {
            // クリーンアップ
            if (isGrabbed)
            {
                OnRelease();
            }
        }
    }

    /// <summary>
    /// アウトライン効果の簡易実装（実際にはQuickOutlineなどのアセットを使用推奨）
    /// </summary>
    public class Outline : MonoBehaviour
    {
        public Color OutlineColor { get; set; } = Color.yellow;
        public float OutlineWidth { get; set; } = 5f;
        
        // 実際の実装では、シェーダーを使用してアウトライン効果を実現
        // ここでは簡易的なプレースホルダー
    }
}
