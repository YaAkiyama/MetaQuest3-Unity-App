using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;
using System.Collections.Generic;

namespace MQ3VRApp.UI
{
    /// <summary>
    /// VR空間でのメディア選択パネル
    /// 3D空間に配置されるWorld Space Canvas
    /// </summary>
    public class VRMediaPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform contentArea;
        [SerializeField] private GridLayoutGroup gridLayout;
        
        [Header("Button Prefab")]
        [SerializeField] private GameObject mediaButtonPrefab;
        [SerializeField] private int buttonsPerRow = 3;
        [SerializeField] private int maxRows = 3;
        [SerializeField] private Vector2 buttonSize = new Vector2(200, 200);
        [SerializeField] private Vector2 spacing = new Vector2(20, 20);
        
        [Header("Panel Animation")]
        [SerializeField] private bool animateOnShow = true;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Title and Description")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private string panelTitle = "メディアライブラリ";
        [SerializeField] private string panelDescription = "360度コンテンツを選択してください";
        
        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private float backgroundPadding = 50f;
        
        private List<UIButtonInteractable> mediaButtons = new List<UIButtonInteractable>();
        private bool isVisible = false;
        private Vector3 originalScale;
        
        public System.Action<int> OnMediaSelected;
        
        protected virtual void Awake()
        {
            InitializePanel();
            SetupCanvas();
            CreateTitle();
            CreateMediaButtons();
            
            originalScale = transform.localScale;
            if (!isVisible)
            {
                gameObject.SetActive(false);
            }
        }
        
        private void InitializePanel()
        {
            // Canvasがない場合は作成
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("VRMediaPanel_Canvas");
                canvasGO.transform.SetParent(transform);
                canvasGO.transform.localPosition = Vector3.zero;
                canvasGO.transform.localRotation = Quaternion.identity;
                canvasGO.transform.localScale = Vector3.one;
                
                canvas = canvasGO.AddComponent<Canvas>();
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
        }
        
        private void SetupCanvas()
        {
            // World Space Canvasに設定
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // Canvas Scalerの設定
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
                scaler.physicalUnit = CanvasScaler.Unit.Millimeters;
                scaler.referencePixelsPerUnit = 100;
            }
            
            // 標準のGraphic Raycasterを使用（VR互換）
            GraphicRaycaster raycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // Canvas RectTransformの設定
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(800, 600);
            canvasRect.localScale = Vector3.one * 0.001f; // 1mm/unit
        }
        
        private void CreateTitle()
        {
            if (titleText == null && !string.IsNullOrEmpty(panelTitle))
            {
                GameObject titleGO = new GameObject("Title");
                titleGO.transform.SetParent(canvas.transform);
                
                titleText = titleGO.AddComponent<TextMeshProUGUI>();
                titleText.text = panelTitle;
                titleText.fontSize = 48;
                titleText.color = Color.white;
                titleText.alignment = TextAlignmentOptions.Center;
                
                RectTransform titleRect = titleText.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.85f);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.offsetMin = Vector2.zero;
                titleRect.offsetMax = Vector2.zero;
            }
            
            if (descriptionText == null && !string.IsNullOrEmpty(panelDescription))
            {
                GameObject descGO = new GameObject("Description");
                descGO.transform.SetParent(canvas.transform);
                
                descriptionText = descGO.AddComponent<TextMeshProUGUI>();
                descriptionText.text = panelDescription;
                descriptionText.fontSize = 24;
                descriptionText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                descriptionText.alignment = TextAlignmentOptions.Center;
                
                RectTransform descRect = descriptionText.GetComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0, 0.75f);
                descRect.anchorMax = new Vector2(1, 0.85f);
                descRect.offsetMin = Vector2.zero;
                descRect.offsetMax = Vector2.zero;
            }
        }
        
        private void CreateMediaButtons()
        {
            // Content Areaの作成
            if (contentArea == null)
            {
                GameObject contentGO = new GameObject("ContentArea");
                contentGO.transform.SetParent(canvas.transform);
                
                contentArea = contentGO.AddComponent<RectTransform>();
                contentArea.anchorMin = new Vector2(0.1f, 0.1f);
                contentArea.anchorMax = new Vector2(0.9f, 0.7f);
                contentArea.offsetMin = Vector2.zero;
                contentArea.offsetMax = Vector2.zero;
                
                gridLayout = contentGO.AddComponent<GridLayoutGroup>();
            }
            
            // Grid Layout Groupの設定
            if (gridLayout != null)
            {
                gridLayout.cellSize = buttonSize;
                gridLayout.spacing = spacing;
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = buttonsPerRow;
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }
            
            // サンプルボタンの作成
            CreateSampleButtons();
        }
        
        private void CreateSampleButtons()
        {
            string[] sampleMediaNames = {
                "サンプル画像1",
                "サンプル画像2", 
                "サンプル動画1",
                "お気に入り",
                "履歴",
                "設定"
            };
            
            for (int i = 0; i < sampleMediaNames.Length; i++)
            {
                CreateMediaButton(i, sampleMediaNames[i]);
            }
        }
        
        private void CreateMediaButton(int index, string title)
        {
            GameObject buttonGO;
            
            if (mediaButtonPrefab != null)
            {
                buttonGO = Instantiate(mediaButtonPrefab, contentArea);
            }
            else
            {
                // デフォルトボタンの作成
                buttonGO = CreateDefaultButton(title);
                buttonGO.transform.SetParent(contentArea);
            }
            
            // UIButtonInteractableコンポーネントの追加
            UIButtonInteractable buttonInteractable = buttonGO.GetComponent<UIButtonInteractable>();
            if (buttonInteractable == null)
            {
                buttonInteractable = buttonGO.AddComponent<UIButtonInteractable>();
            }
            
            // ボタンのイベント設定
            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                int buttonIndex = index; // クロージャー対策
                button.onClick.AddListener(() => OnButtonClicked(buttonIndex));
            }
            
            mediaButtons.Add(buttonInteractable);
        }
        
        private GameObject CreateDefaultButton(string title)
        {
            GameObject buttonGO = new GameObject($"MediaButton_{title}");
            
            // Image (Background)
            Image bgImage = buttonGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Button
            Button button = buttonGO.AddComponent<Button>();
            button.targetGraphic = bgImage;
            
            // RectTransform
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;
            
            // Text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            
            TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = title;
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = buttonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return buttonGO;
        }
        
        private void OnButtonClicked(int index)
        {
            UnityEngine.Debug.Log($"Media button {index} clicked");
            OnMediaSelected?.Invoke(index);
        }
        
        /// <summary>
        /// パネルの表示/非表示
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (isVisible == visible) return;
            
            isVisible = visible;
            
            if (visible)
            {
                gameObject.SetActive(true);
                if (animateOnShow)
                {
                    StartCoroutine(AnimateShow());
                }
            }
            else
            {
                if (animateOnShow)
                {
                    StartCoroutine(AnimateHide());
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
        
        private System.Collections.IEnumerator AnimateShow()
        {
            float elapsed = 0f;
            transform.localScale = Vector3.zero;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                float scaleValue = scaleCurve.Evaluate(progress);
                transform.localScale = originalScale * scaleValue;
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        private System.Collections.IEnumerator AnimateHide()
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                float scaleValue = scaleCurve.Evaluate(1f - progress);
                transform.localScale = startScale * scaleValue;
                yield return null;
            }
            
            gameObject.SetActive(false);
            transform.localScale = originalScale;
        }
        
        /// <summary>
        /// パネルのタイトルを設定
        /// </summary>
        public void SetTitle(string title)
        {
            panelTitle = title;
            if (titleText != null)
            {
                titleText.text = title;
            }
        }
        
        /// <summary>
        /// パネルの説明を設定
        /// </summary>
        public void SetDescription(string description)
        {
            panelDescription = description;
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
        }
        
        /// <summary>
        /// プレイヤーの方向を向くように回転
        /// </summary>
        public void LookAtPlayer()
        {
            Camera playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Vector3 direction = playerCamera.transform.position - transform.position;
                direction.y = 0; // Y軸は無視（水平方向のみ）
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        
        /// <summary>
        /// パネルを特定の距離と角度でプレイヤーの前に配置
        /// </summary>
        public void PositionInFrontOfPlayer(float distance = 2f, float angle = 0f, float height = 1.6f)
        {
            Camera playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Vector3 forward = playerCamera.transform.forward;
                forward.y = 0;
                forward.Normalize();
                
                // 角度調整
                if (angle != 0)
                {
                    forward = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                }
                
                Vector3 targetPosition = playerCamera.transform.position + forward * distance;
                targetPosition.y = height;
                
                transform.position = targetPosition;
                LookAtPlayer();
            }
        }
    }
}