using UnityEngine;
using UnityEngine.UI;

namespace MQ3VRApp.UI
{
    /// <summary>
    /// 参考画像を基にした3パネル構成のメディアビューアー
    /// 左：チャンネル、中央：コンテンツ、右：お気に入り/履歴
    /// </summary>
    public class MediaViewerPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject leftPanel;
        [SerializeField] private GameObject centerPanel; 
        [SerializeField] private GameObject rightPanel;
        
        [Header("Left Panel - Channels")]
        [SerializeField] private Transform channelsContainer;
        [SerializeField] private GameObject channelButtonPrefab;
        
        [Header("Center Panel - Content")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject contentItemPrefab;
        [SerializeField] private Text contentTitleText;
        
        [Header("Right Panel - Tools")]
        [SerializeField] private Transform favoritesContainer;
        [SerializeField] private Transform historyContainer;
        [SerializeField] private Text statusText;
        
        [Header("Style Settings")]
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color accentColor = new Color(1f, 0.6f, 0.2f, 1f); // オレンジ
        [SerializeField] private Color selectedColor = new Color(1f, 0.6f, 0.2f, 0.3f);
        
        // チャンネルデータ
        private string[] channelNames = {
            "All Files",
            "Local Files", 
            "Network",
            "AirScreen",
            "YouTube",
            "Hidden Files"
        };
        
        private string[] channelIcons = {
            "📁", "💻", "🌐", "📺", "▶️", "🔒"
        };
        
        private int selectedChannelIndex = 0;
        
        private void Start()
        {
            CreatePanelStructure();
            PopulateChannels();
            PopulateContent();
            UpdateRightPanel();
        }
        
        private void CreatePanelStructure()
        {
            // メインキャンバスの設定
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // Canvas Scalerの設定
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }
            
            // Graphic Raycasterの設定
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // RectTransformの設定
            RectTransform canvasRect = GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1200, 700);
            canvasRect.localScale = Vector3.one * 0.001f;
            
            CreateLeftPanel();
            CreateCenterPanel();
            CreateRightPanel();
        }
        
        private void CreateLeftPanel()
        {
            if (leftPanel == null)
            {
                leftPanel = new GameObject("LeftPanel");
                leftPanel.transform.SetParent(transform);
                
                Image bgImage = leftPanel.AddComponent<Image>();
                bgImage.color = backgroundColor;
                
                RectTransform rect = leftPanel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0.25f, 1);
                rect.offsetMin = new Vector2(10, 10);
                rect.offsetMax = new Vector2(-5, -10);
                
                // タイトル
                GameObject title = new GameObject("Title");
                title.transform.SetParent(leftPanel.transform);
                Text titleText = title.AddComponent<Text>();
                titleText.text = "CHANNELS";
                titleText.fontSize = 24;
                titleText.color = textColor;
                titleText.alignment = TextAnchor.MiddleCenter;
                
                RectTransform titleRect = title.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.9f);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.offsetMin = Vector2.zero;
                titleRect.offsetMax = Vector2.zero;
                
                // チャンネルコンテナ
                GameObject container = new GameObject("ChannelsContainer");
                container.transform.SetParent(leftPanel.transform);
                channelsContainer = container.transform;
                
                RectTransform containerRect = container.GetComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0, 0);
                containerRect.anchorMax = new Vector2(1, 0.9f);
                containerRect.offsetMin = new Vector2(5, 5);
                containerRect.offsetMax = new Vector2(-5, -5);
                
                VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 5;
                layout.childControlHeight = false;
                layout.childControlWidth = true;
                layout.childForceExpandWidth = true;
            }
        }
        
        private void CreateCenterPanel()
        {
            if (centerPanel == null)
            {
                centerPanel = new GameObject("CenterPanel");
                centerPanel.transform.SetParent(transform);
                
                Image bgImage = centerPanel.AddComponent<Image>();
                bgImage.color = backgroundColor;
                
                RectTransform rect = centerPanel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.25f, 0);
                rect.anchorMax = new Vector2(0.75f, 1);
                rect.offsetMin = new Vector2(5, 10);
                rect.offsetMax = new Vector2(-5, -10);
                
                // タイトル
                GameObject title = new GameObject("Title");
                title.transform.SetParent(centerPanel.transform);
                contentTitleText = title.AddComponent<Text>();
                contentTitleText.text = "Gallery";
                contentTitleText.fontSize = 24;
                contentTitleText.color = textColor;
                contentTitleText.alignment = TextAnchor.MiddleCenter;
                
                RectTransform titleRect = title.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.9f);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.offsetMin = Vector2.zero;
                titleRect.offsetMax = Vector2.zero;
                
                // コンテンツコンテナ
                GameObject container = new GameObject("ContentContainer");
                container.transform.SetParent(centerPanel.transform);
                contentContainer = container.transform;
                
                RectTransform containerRect = container.GetComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0, 0);
                containerRect.anchorMax = new Vector2(1, 0.9f);
                containerRect.offsetMin = new Vector2(10, 10);
                containerRect.offsetMax = new Vector2(-10, -10);
                
                GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(100, 80);
                grid.spacing = new Vector2(10, 10);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 4;
                grid.childAlignment = TextAnchor.UpperCenter;
            }
        }
        
        private void CreateRightPanel()
        {
            if (rightPanel == null)
            {
                rightPanel = new GameObject("RightPanel");
                rightPanel.transform.SetParent(transform);
                
                Image bgImage = rightPanel.AddComponent<Image>();
                bgImage.color = backgroundColor;
                
                RectTransform rect = rightPanel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.75f, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = new Vector2(5, 10);
                rect.offsetMax = new Vector2(-10, -10);
                
                // 上部：FAVORITES
                GameObject favTitle = new GameObject("FavoritesTitle");
                favTitle.transform.SetParent(rightPanel.transform);
                Text favTitleText = favTitle.AddComponent<Text>();
                favTitleText.text = "FAVORITES";
                favTitleText.fontSize = 20;
                favTitleText.color = textColor;
                favTitleText.alignment = TextAnchor.MiddleCenter;
                
                RectTransform favTitleRect = favTitle.GetComponent<RectTransform>();
                favTitleRect.anchorMin = new Vector2(0, 0.8f);
                favTitleRect.anchorMax = new Vector2(1, 0.9f);
                favTitleRect.offsetMin = Vector2.zero;
                favTitleRect.offsetMax = Vector2.zero;
                
                // 下部：HISTORY  
                GameObject histTitle = new GameObject("HistoryTitle");
                histTitle.transform.SetParent(rightPanel.transform);
                Text histTitleText = histTitle.AddComponent<Text>();
                histTitleText.text = "HISTORY";
                histTitleText.fontSize = 20;
                histTitleText.color = textColor;
                histTitleText.alignment = TextAnchor.MiddleCenter;
                
                RectTransform histTitleRect = histTitle.GetComponent<RectTransform>();
                histTitleRect.anchorMin = new Vector2(0, 0.4f);
                histTitleRect.anchorMax = new Vector2(1, 0.5f);
                histTitleRect.offsetMin = Vector2.zero;
                histTitleRect.offsetMax = Vector2.zero;
                
                // ステータステキスト
                GameObject status = new GameObject("Status");
                status.transform.SetParent(rightPanel.transform);
                statusText = status.AddComponent<Text>();
                statusText.text = "NO HISTORY";
                statusText.fontSize = 16;
                statusText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                statusText.alignment = TextAnchor.MiddleCenter;
                
                RectTransform statusRect = status.GetComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0, 0.2f);
                statusRect.anchorMax = new Vector2(1, 0.4f);
                statusRect.offsetMin = Vector2.zero;
                statusRect.offsetMax = Vector2.zero;
            }
        }
        
        private void PopulateChannels()
        {
            for (int i = 0; i < channelNames.Length; i++)
            {
                GameObject channelButton = CreateChannelButton(i, channelIcons[i], channelNames[i]);
                channelButton.transform.SetParent(channelsContainer);
            }
        }
        
        private GameObject CreateChannelButton(int index, string icon, string text)
        {
            GameObject buttonObj = new GameObject($"Channel_{text}");
            
            Image bgImage = buttonObj.AddComponent<Image>();
            bgImage.color = index == selectedChannelIndex ? selectedColor : Color.clear;
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.onClick.AddListener(() => OnChannelSelected(index));
            
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);
            
            // テキスト
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = $"{icon} {text}";
            textComponent.fontSize = 18;
            textComponent.color = index == selectedChannelIndex ? accentColor : textColor;
            textComponent.alignment = TextAnchor.MiddleLeft;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            return buttonObj;
        }
        
        private void PopulateContent()
        {
            // サンプルコンテンツアイテム
            string[] sampleFiles = {
                "Sample_001", "Sample_002", "Sample_003", "Sample_004",
                "Sample_005", "Sample_006", "Sample_007", "Sample_008"
            };
            
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (string fileName in sampleFiles)
            {
                GameObject contentItem = CreateContentItem(fileName);
                contentItem.transform.SetParent(contentContainer);
            }
        }
        
        private GameObject CreateContentItem(string fileName)
        {
            GameObject itemObj = new GameObject($"Content_{fileName}");
            
            Image bgImage = itemObj.AddComponent<Image>();
            bgImage.color = accentColor;
            
            Button button = itemObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.onClick.AddListener(() => OnContentSelected(fileName));
            
            // ファイル名テキスト
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform);
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = fileName;
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.LowerCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);
            
            return itemObj;
        }
        
        private void OnChannelSelected(int index)
        {
            if (selectedChannelIndex != index)
            {
                selectedChannelIndex = index;
                UnityEngine.Debug.Log($"Channel selected: {channelNames[index]}");
                
                // チャンネル表示を更新
                for (int i = 0; i < channelsContainer.childCount; i++)
                {
                    Transform child = channelsContainer.GetChild(i);
                    Image bg = child.GetComponent<Image>();
                    Text text = child.GetComponentInChildren<Text>();
                    
                    if (i == index)
                    {
                        bg.color = selectedColor;
                        text.color = accentColor;
                    }
                    else
                    {
                        bg.color = Color.clear;
                        text.color = textColor;
                    }
                }
                
                // コンテンツタイトルを更新
                contentTitleText.text = channelNames[index];
                PopulateContent();
            }
        }
        
        private void OnContentSelected(string fileName)
        {
            UnityEngine.Debug.Log($"Content selected: {fileName}");
            // ここで360度メディアの表示処理を行う
        }
        
        private void UpdateRightPanel()
        {
            // 履歴やお気に入りの状態を更新
            statusText.text = "NO HISTORY";
        }
    }
}