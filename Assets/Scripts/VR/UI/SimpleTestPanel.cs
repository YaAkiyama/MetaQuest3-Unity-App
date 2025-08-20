using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MQ3VRApp.UI
{
    /// <summary>
    /// 簡単なテスト用VRパネル
    /// </summary>
    public class SimpleTestPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private float distance = 2f;
        [SerializeField] private float width = 2f;
        [SerializeField] private float height = 1.5f;
        
        private Canvas canvas;
        private GameObject panelBackground;
        
        private void Start()
        {
            CreateTestPanel();
            PositionPanel();
        }
        
        private void CreateTestPanel()
        {
            // Canvas作成
            GameObject canvasGO = new GameObject("Test Panel Canvas");
            canvasGO.transform.SetParent(transform);
            
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1;
            
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Canvas RectTransform設定
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(width * 100, height * 100);
            canvasRect.localScale = Vector3.one * 0.01f;
            
            // 背景パネル作成
            panelBackground = new GameObject("Background");
            panelBackground.transform.SetParent(canvasGO.transform);
            
            Image bgImage = panelBackground.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            RectTransform bgRect = panelBackground.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // テストテキスト作成
            GameObject textGO = new GameObject("Test Text");
            textGO.transform.SetParent(panelBackground.transform);
            
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "VR Test Panel\\nPanel is Working!\\nLook around with your head\\nControllers should be visible";
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 20);
            textRect.offsetMax = new Vector2(-20, -20);
            
            UnityEngine.Debug.Log("SimpleTestPanel created successfully");
        }
        
        private void PositionPanel()
        {
            // プレイヤーの前方に配置
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
            
            if (mainCamera != null)
            {
                Vector3 cameraPos = mainCamera.transform.position;
                Vector3 cameraForward = mainCamera.transform.forward;
                
                transform.position = cameraPos + cameraForward * distance;
                transform.LookAt(cameraPos);
                transform.Rotate(0, 180, 0); // 反転して正面を向くように
            }
            else
            {
                // フォールバック位置
                transform.position = new Vector3(0, 1.6f, 2f);
                transform.rotation = Quaternion.identity;
            }
            
            UnityEngine.Debug.Log($"Panel positioned at: {transform.position}");
        }
        
        private void Update()
        {
            // デバッグ情報をコンソールに出力（最初の数フレームのみ）
            if (Time.time < 5f && Time.frameCount % 120 == 0)
            {
                UnityEngine.Debug.Log($"SimpleTestPanel Update - Position: {transform.position}, Active: {gameObject.activeInHierarchy}");
                
                if (canvas != null)
                {
                    UnityEngine.Debug.Log($"Canvas - RenderMode: {canvas.renderMode}, Enabled: {canvas.enabled}");
                }
            }
        }
    }
}