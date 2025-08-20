using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace MQ3VRApp.UI
{
    /// <summary>
    /// VR用レーザーポインターコントローラー
    /// XR Ray Interactorと連携してレーザーの表示を制御
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor))]
    [RequireComponent(typeof(LineRenderer))]
    public class LaserPointerController : MonoBehaviour
    {
        [Header("Laser Settings")]
        [SerializeField] private float laserMaxLength = 10f;
        [SerializeField] private float laserWidth = 0.01f;
        [SerializeField] private Color laserColor = new Color(0.2f, 0.5f, 1f, 0.8f);
        [SerializeField] private Color hoverColor = new Color(0.2f, 1f, 0.5f, 0.8f);
        
        [Header("Hit Indicator")]
        [SerializeField] private GameObject hitDotPrefab;
        [SerializeField] private float dotSize = 0.05f;
        [SerializeField] private Color dotColor = Color.white;
        
        [Header("Controller Input")]
        [SerializeField] private InputActionReference activateAction;
        [SerializeField] private InputActionReference selectAction;
        
        [Header("Effects")]
        [SerializeField] private bool showOnlyWhenPointing = true;
        [SerializeField] private AnimationCurve laserWidthCurve = AnimationCurve.Linear(0, 1, 1, 0.5f);
        [SerializeField] private bool vibrateOnHover = true;
        [SerializeField] private float hapticIntensity = 0.1f;
        
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
        private LineRenderer lineRenderer;
        private GameObject hitDot;
        private MeshRenderer hitDotRenderer;
        
        private bool isHovering;
        private bool isSelecting;
        private float currentLaserLength;
        
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor xrController;
        
        private void Awake()
        {
            InitializeComponents();
            CreateHitIndicator();
        }
        
        private void InitializeComponents()
        {
            // XR Ray Interactorの取得
            rayInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
            if (rayInteractor == null)
            {
                rayInteractor = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
            }
            
            // Line Rendererの設定
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            ConfigureLineRenderer();
            
            // XR Controllerの取得
            xrController = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>();
        }
        
        private void ConfigureLineRenderer()
        {
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth * 0.5f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = laserColor;
            lineRenderer.endColor = laserColor;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;
        }
        
        private void CreateHitIndicator()
        {
            if (hitDotPrefab != null)
            {
                hitDot = Instantiate(hitDotPrefab);
            }
            else
            {
                // デフォルトのドットを作成
                hitDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hitDot.transform.localScale = Vector3.one * dotSize;
                
                // コライダーを無効化
                Collider col = hitDot.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
            
            hitDot.name = "LaserHitDot";
            hitDotRenderer = hitDot.GetComponent<MeshRenderer>();
            
            if (hitDotRenderer != null)
            {
                Material dotMat = new Material(Shader.Find("Unlit/Color"));
                dotMat.color = dotColor;
                hitDotRenderer.material = dotMat;
            }
            
            hitDot.SetActive(false);
        }
        
        private void OnEnable()
        {
            if (rayInteractor != null)
            {
                rayInteractor.hoverEntered.AddListener(OnHoverEntered);
                rayInteractor.hoverExited.AddListener(OnHoverExited);
                rayInteractor.selectEntered.AddListener(OnSelectEntered);
                rayInteractor.selectExited.AddListener(OnSelectExited);
            }
        }
        
        private void OnDisable()
        {
            if (rayInteractor != null)
            {
                rayInteractor.hoverEntered.RemoveListener(OnHoverEntered);
                rayInteractor.hoverExited.RemoveListener(OnHoverExited);
                rayInteractor.selectEntered.RemoveListener(OnSelectEntered);
                rayInteractor.selectExited.RemoveListener(OnSelectExited);
            }
            
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
            
            if (hitDot != null)
            {
                hitDot.SetActive(false);
            }
        }
        
        private void Update()
        {
            UpdateLaser();
            UpdateInput();
        }
        
        private void UpdateLaser()
        {
            // レイキャストの結果を取得
            bool hasHit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            
            // レーザーの表示制御
            bool shouldShowLaser = !showOnlyWhenPointing || 
                                  (activateAction != null && activateAction.action.ReadValue<float>() > 0.1f) ||
                                  isHovering || isSelecting;
            
            if (shouldShowLaser)
            {
                lineRenderer.enabled = true;
                
                // レーザーの始点と終点を設定
                Vector3 startPoint = transform.position;
                Vector3 endPoint;
                
                if (hasHit)
                {
                    endPoint = hit.point;
                    currentLaserLength = hit.distance;
                    
                    // ヒットドットの表示
                    if (hitDot != null)
                    {
                        hitDot.SetActive(true);
                        hitDot.transform.position = hit.point + hit.normal * 0.01f;
                        hitDot.transform.rotation = Quaternion.LookRotation(-hit.normal);
                    }
                }
                else
                {
                    endPoint = startPoint + transform.forward * laserMaxLength;
                    currentLaserLength = laserMaxLength;
                    
                    if (hitDot != null)
                    {
                        hitDot.SetActive(false);
                    }
                }
                
                // LineRendererの更新
                lineRenderer.SetPosition(0, startPoint);
                lineRenderer.SetPosition(1, endPoint);
                
                // レーザーの幅を距離に応じて調整
                float normalizedDistance = currentLaserLength / laserMaxLength;
                float widthMultiplier = laserWidthCurve.Evaluate(normalizedDistance);
                lineRenderer.startWidth = laserWidth * widthMultiplier;
                lineRenderer.endWidth = laserWidth * widthMultiplier * 0.5f;
                
                // ホバー時の色変更
                Color currentColor = isHovering ? hoverColor : laserColor;
                lineRenderer.startColor = currentColor;
                lineRenderer.endColor = currentColor;
            }
            else
            {
                lineRenderer.enabled = false;
                if (hitDot != null)
                {
                    hitDot.SetActive(false);
                }
            }
        }
        
        private void UpdateInput()
        {
            // 選択状態の更新
            if (selectAction != null)
            {
                float selectValue = selectAction.action.ReadValue<float>();
                bool wasSelecting = isSelecting;
                isSelecting = selectValue > 0.5f;
                
                // 選択開始時に振動フィードバック
                if (isSelecting && !wasSelecting && xrController != null)
                {
                    xrController.SendHapticImpulse(hapticIntensity * 2f, 0.1f);
                }
            }
        }
        
        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            isHovering = true;
            
            // ホバー時の振動フィードバック
            if (vibrateOnHover && xrController != null)
            {
                xrController.SendHapticImpulse(hapticIntensity, 0.1f);
            }
        }
        
        private void OnHoverExited(HoverExitEventArgs args)
        {
            isHovering = false;
        }
        
        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            isSelecting = true;
        }
        
        private void OnSelectExited(SelectExitEventArgs args)
        {
            isSelecting = false;
        }
        
        /// <summary>
        /// レーザーの有効/無効を切り替え
        /// </summary>
        public void SetLaserEnabled(bool enabled)
        {
            this.enabled = enabled;
            if (!enabled)
            {
                lineRenderer.enabled = false;
                if (hitDot != null)
                {
                    hitDot.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// レーザーの色を設定
        /// </summary>
        public void SetLaserColor(Color color)
        {
            laserColor = color;
            if (lineRenderer != null && !isHovering)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }
        
        /// <summary>
        /// レーザーの最大長を設定
        /// </summary>
        public void SetLaserMaxLength(float length)
        {
            laserMaxLength = Mathf.Max(0.1f, length);
        }
    }
}