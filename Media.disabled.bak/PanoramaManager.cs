using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

namespace MQ3VRApp.Media
{
    /// <summary>
    /// 360度パノラマメディア管理クラス
    /// Skyboxマテリアルの切り替えによる360度表示を制御
    /// </summary>
    public class PanoramaManager : MonoBehaviour
    {
        [Header("Skybox Settings")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Shader panoramicShader;
        [SerializeField] private string texturePropertyName = "_MainTex";
        
        [Header("Display Settings")]
        [SerializeField] private bool autoRotate = false;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        
        [Header("Video Settings")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RenderTexture videoRenderTexture;
        [SerializeField] private int videoTextureWidth = 2048;
        [SerializeField] private int videoTextureHeight = 1024;
        
        [Header("Fade Settings")]
        [SerializeField] private bool useFadeTransition = true;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private Material currentSkyboxMaterial;
        private Texture2D currentTexture;
        private VideoClip currentVideo;
        private bool isVideoMode = false;
        private float skyboxRotation = 0f;
        
        public enum MediaType
        {
            Image,
            Video
        }
        
        [System.Serializable]
        public class MediaContent
        {
            public string name;
            public MediaType type;
            public Texture2D image;
            public VideoClip video;
            public string description;
            public bool isDefault;
        }
        
        [Header("Media Library")]
        [SerializeField] private List<MediaContent> mediaLibrary = new List<MediaContent>();
        
        private void Awake()
        {
            InitializePanoramaSystem();
        }
        
        private void Start()
        {
            LoadDefaultMedia();
        }
        
        private void Update()
        {
            if (autoRotate)
            {
                UpdateSkyboxRotation();
            }
        }
        
        private void InitializePanoramaSystem()
        {
            // デフォルトのPanoramicシェーダーを設定
            if (panoramicShader == null)
            {
                panoramicShader = Shader.Find("Skybox/Panoramic");
            }
            
            // Skyboxマテリアルの作成
            if (skyboxMaterial == null)
            {
                skyboxMaterial = new Material(panoramicShader);
                skyboxMaterial.name = "VR Panorama Material";
            }
            
            // VideoPlayerの初期化
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
            
            SetupVideoPlayer();
            
            Debug.Log("PanoramaManager initialized successfully");
        }
        
        private void SetupVideoPlayer()
        {
            if (videoPlayer == null) return;
            
            // RenderTextureの作成
            if (videoRenderTexture == null)
            {
                videoRenderTexture = new RenderTexture(videoTextureWidth, videoTextureHeight, 0);
                videoRenderTexture.name = "VideoRenderTexture";
            }
            
            // VideoPlayerの設定
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            
            // AudioSourceの追加
            if (GetComponent<AudioSource>() == null)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                videoPlayer.SetTargetAudioSource(0, audioSource);
                audioSource.spatialBlend = 0f; // 2D Audio for 360 content
            }
        }
        
        private void LoadDefaultMedia()
        {
            // デフォルトメディアの読み込み
            foreach (var media in mediaLibrary)
            {
                if (media.isDefault)
                {
                    LoadMedia(media);
                    break;
                }
            }
        }
        
        /// <summary>
        /// メディアコンテンツを読み込み、360度表示
        /// </summary>
        public void LoadMedia(MediaContent content)
        {
            if (content == null) return;
            
            StartCoroutine(LoadMediaCoroutine(content));
        }
        
        private IEnumerator LoadMediaCoroutine(MediaContent content)
        {
            // フェードアウト
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeOut());
            }
            
            // メディアタイプに応じた処理
            switch (content.type)
            {
                case MediaType.Image:
                    LoadImageContent(content);
                    break;
                case MediaType.Video:
                    LoadVideoContent(content);
                    break;
            }
            
            // フェードイン
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeIn());
            }
            
            Debug.Log($"Loaded media: {content.name}");
        }
        
        private void LoadImageContent(MediaContent content)
        {
            if (content.image == null) return;
            
            isVideoMode = false;
            
            // VideoPlayerを停止
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            
            // Skyboxマテリアルに画像を設定
            currentTexture = content.image;
            skyboxMaterial.SetTexture(texturePropertyName, currentTexture);
            
            // マッピングを球面（Spherical）に設定
            skyboxMaterial.SetFloat("_Mapping", 1);
            skyboxMaterial.SetFloat("_ImageType", 0);
            
            // SkyboxをRenderSettingsに適用
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
        
        private void LoadVideoContent(MediaContent content)
        {
            if (content.video == null) return;
            
            isVideoMode = true;
            currentVideo = content.video;
            
            // VideoPlayerに動画を設定
            videoPlayer.clip = currentVideo;
            
            // Skyboxマテリアルに動画テクスチャを設定
            skyboxMaterial.SetTexture(texturePropertyName, videoRenderTexture);
            skyboxMaterial.SetFloat("_Mapping", 1);
            skyboxMaterial.SetFloat("_ImageType", 0);
            
            // SkyboxをRenderSettingsに適用
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            
            // 動画再生開始
            videoPlayer.Play();
        }
        
        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            Color startColor = skyboxMaterial.GetColor("_Tint");
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeOutDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                
                Color fadeColor = Color.Lerp(startColor, Color.black, curveValue);
                skyboxMaterial.SetColor("_Tint", fadeColor);
                
                yield return null;
            }
        }
        
        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            Color targetColor = Color.white;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeInDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                
                Color fadeColor = Color.Lerp(Color.black, targetColor, curveValue);
                skyboxMaterial.SetColor("_Tint", fadeColor);
                
                yield return null;
            }
        }
        
        private void UpdateSkyboxRotation()
        {
            skyboxRotation += rotationSpeed * Time.deltaTime;
            skyboxRotation = skyboxRotation % 360f;
            
            skyboxMaterial.SetFloat("_Rotation", skyboxRotation);
        }
        
        /// <summary>
        /// 特定のメディアを名前で読み込み
        /// </summary>
        public void LoadMediaByName(string mediaName)
        {
            MediaContent content = mediaLibrary.Find(m => m.name == mediaName);
            if (content != null)
            {
                LoadMedia(content);
            }
            else
            {
                Debug.LogWarning($"Media not found: {mediaName}");
            }
        }
        
        /// <summary>
        /// 動画の再生/一時停止を切り替え
        /// </summary>
        public void ToggleVideoPlayback()
        {
            if (!isVideoMode || videoPlayer == null) return;
            
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Play();
            }
        }
        
        /// <summary>
        /// 自動回転の有効/無効を切り替え
        /// </summary>
        public void SetAutoRotation(bool enabled)
        {
            autoRotate = enabled;
        }
        
        /// <summary>
        /// Skyboxの回転角度を設定
        /// </summary>
        public void SetSkyboxRotation(float rotation)
        {
            skyboxRotation = rotation % 360f;
            skyboxMaterial.SetFloat("_Rotation", skyboxRotation);
        }
        
        /// <summary>
        /// 新しいメディアをライブラリに追加
        /// </summary>
        public void AddMediaToLibrary(string name, MediaType type, Texture2D image = null, VideoClip video = null, string description = "")
        {
            MediaContent newContent = new MediaContent
            {
                name = name,
                type = type,
                image = image,
                video = video,
                description = description,
                isDefault = false
            };
            
            mediaLibrary.Add(newContent);
        }
        
        /// <summary>
        /// メディアライブラリの取得
        /// </summary>
        public List<MediaContent> GetMediaLibrary()
        {
            return new List<MediaContent>(mediaLibrary);
        }
        
        /// <summary>
        /// 現在のメディアタイプを取得
        /// </summary>
        public MediaType GetCurrentMediaType()
        {
            return isVideoMode ? MediaType.Video : MediaType.Image;
        }
        
        /// <summary>
        /// 動画の再生状態を取得
        /// </summary>
        public bool IsVideoPlaying()
        {
            return isVideoMode && videoPlayer != null && videoPlayer.isPlaying;
        }
        
        private void OnDestroy()
        {
            // リソースのクリーンアップ
            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
            }
        }
    }
}