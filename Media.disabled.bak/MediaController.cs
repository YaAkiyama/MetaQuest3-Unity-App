using UnityEngine;
using System.Collections.Generic;
using MQ3VRApp.UI;

namespace MQ3VRApp.Media
{
    /// <summary>
    /// メディア再生制御クラス
    /// PanoramaManagerとUIシステムを連携させる中間コントローラー
    /// </summary>
    public class MediaController : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private PanoramaManager panoramaManager;
        [SerializeField] private MediaLibrary mediaLibrary;
        
        [Header("UI References")]
        [SerializeField] private VRMediaPanel mediaSelectionPanel;
        [SerializeField] private VRMediaPanel playbackControlPanel;
        
        [Header("Playback Settings")]
        [SerializeField] private bool autoPlayNext = false;
        [SerializeField] private float autoPlayDelay = 3f;
        [SerializeField] private bool shuffleMode = false;
        [SerializeField] private bool repeatMode = false;
        
        private List<PanoramaManager.MediaContent> currentPlaylist;
        private int currentMediaIndex = 0;
        private PanoramaManager.MediaContent currentMedia;
        private bool isInitialized = false;
        
        public delegate void MediaControllerEvent(PanoramaManager.MediaContent content);
        public event MediaControllerEvent OnMediaChanged;
        public event MediaControllerEvent OnPlaybackStarted;
        public event MediaControllerEvent OnPlaybackStopped;
        
        private void Awake()
        {
            InitializeController();
        }
        
        private void Start()
        {
            SetupUI();
            LoadInitialPlaylist();
        }
        
        private void InitializeController()
        {
            // PanoramaManagerの取得または作成
            if (panoramaManager == null)
            {
                panoramaManager = FindObjectOfType<PanoramaManager>();
                if (panoramaManager == null)
                {
                    GameObject panoramaObject = new GameObject("PanoramaManager");
                    panoramaManager = panoramaObject.AddComponent<PanoramaManager>();
                }
            }
            
            // MediaLibraryの取得
            if (mediaLibrary == null)
            {
                mediaLibrary = MediaLibrary.Instance;
            }
            
            isInitialized = true;
            Debug.Log("MediaController initialized successfully");
        }
        
        private void SetupUI()
        {
            // メディア選択パネルの設定
            if (mediaSelectionPanel != null)
            {
                SetupMediaSelectionPanel();
            }
            
            // 再生制御パネルの設定
            if (playbackControlPanel != null)
            {
                SetupPlaybackControlPanel();
            }
        }
        
        private void SetupMediaSelectionPanel()
        {
            // メディアライブラリからコンテンツを取得してボタンを生成
            if (mediaLibrary != null)
            {
                List<PanoramaManager.MediaContent> allContent = mediaLibrary.GetAllContent();
                
                foreach (var content in allContent)
                {
                    // メディア選択ボタンを作成
                    CreateMediaButton(content);
                }
            }
        }
        
        private void SetupPlaybackControlPanel()
        {
            // 再生制御ボタンの設定
            CreatePlaybackControls();
        }
        
        private void CreateMediaButton(PanoramaManager.MediaContent content)
        {
            if (mediaSelectionPanel == null) return;
            
            // ボタンの作成とイベント設定
            string buttonText = $"{content.name}\n({content.type})";
            
            // 実際のボタン作成処理はVRMediaPanelに委譲
            // この例では概念的な実装を示す
            Debug.Log($"Creating media button for: {content.name}");
        }
        
        private void CreatePlaybackControls()
        {
            if (playbackControlPanel == null) return;
            
            // 再生制御ボタンの作成
            Debug.Log("Creating playback control buttons");
        }
        
        private void LoadInitialPlaylist()
        {
            if (mediaLibrary == null) return;
            
            currentPlaylist = mediaLibrary.GetAllContent();
            
            if (currentPlaylist.Count > 0)
            {
                // デフォルトメディアまたは最初のメディアを読み込み
                PanoramaManager.MediaContent defaultMedia = mediaLibrary.GetDefaultContent();
                if (defaultMedia != null)
                {
                    LoadMedia(defaultMedia);
                }
                else
                {
                    LoadMedia(currentPlaylist[0]);
                }
            }
        }
        
        /// <summary>
        /// メディアを読み込んで再生
        /// </summary>
        public void LoadMedia(PanoramaManager.MediaContent content)
        {
            if (!isInitialized || content == null) return;
            
            currentMedia = content;
            currentMediaIndex = currentPlaylist.FindIndex(m => m.name == content.name);
            
            // PanoramaManagerにメディア読み込みを指示
            panoramaManager.LoadMedia(content);
            
            // イベント発火
            OnMediaChanged?.Invoke(content);
            OnPlaybackStarted?.Invoke(content);
            
            Debug.Log($"Loading media: {content.name}");
        }
        
        /// <summary>
        /// メディアを名前で読み込み
        /// </summary>
        public void LoadMediaByName(string mediaName)
        {
            if (mediaLibrary == null) return;
            
            PanoramaManager.MediaContent content = mediaLibrary.FindContentByName(mediaName);
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
        /// 次のメディアに移動
        /// </summary>
        public void PlayNext()
        {
            if (currentPlaylist == null || currentPlaylist.Count == 0) return;
            
            int nextIndex;
            
            if (shuffleMode)
            {
                nextIndex = Random.Range(0, currentPlaylist.Count);
            }
            else
            {
                nextIndex = (currentMediaIndex + 1) % currentPlaylist.Count;
                
                // リピートモードが無効で最後のメディアの場合
                if (!repeatMode && currentMediaIndex == currentPlaylist.Count - 1)
                {
                    StopPlayback();
                    return;
                }
            }
            
            LoadMedia(currentPlaylist[nextIndex]);
        }
        
        /// <summary>
        /// 前のメディアに移動
        /// </summary>
        public void PlayPrevious()
        {
            if (currentPlaylist == null || currentPlaylist.Count == 0) return;
            
            int previousIndex;
            
            if (shuffleMode)
            {
                previousIndex = Random.Range(0, currentPlaylist.Count);
            }
            else
            {
                previousIndex = (currentMediaIndex - 1 + currentPlaylist.Count) % currentPlaylist.Count;
            }
            
            LoadMedia(currentPlaylist[previousIndex]);
        }
        
        /// <summary>
        /// 再生の一時停止/再開
        /// </summary>
        public void TogglePlayback()
        {
            if (panoramaManager == null) return;
            
            panoramaManager.ToggleVideoPlayback();
        }
        
        /// <summary>
        /// 再生停止
        /// </summary>
        public void StopPlayback()
        {
            OnPlaybackStopped?.Invoke(currentMedia);
            Debug.Log("Playback stopped");
        }
        
        /// <summary>
        /// シャッフルモードの切り替え
        /// </summary>
        public void ToggleShuffle()
        {
            shuffleMode = !shuffleMode;
            Debug.Log($"Shuffle mode: {shuffleMode}");
        }
        
        /// <summary>
        /// リピートモードの切り替え
        /// </summary>
        public void ToggleRepeat()
        {
            repeatMode = !repeatMode;
            Debug.Log($"Repeat mode: {repeatMode}");
        }
        
        /// <summary>
        /// 自動再生の切り替え
        /// </summary>
        public void ToggleAutoPlay()
        {
            autoPlayNext = !autoPlayNext;
            Debug.Log($"Auto play: {autoPlayNext}");
        }
        
        /// <summary>
        /// Skyboxの自動回転を切り替え
        /// </summary>
        public void ToggleAutoRotation()
        {
            if (panoramaManager != null)
            {
                // PanoramaManagerの自動回転状態を取得して切り替え
                // 現在の状態を取得する方法がないため、常に切り替える
                panoramaManager.SetAutoRotation(!panoramaManager.GetCurrentMediaType().Equals(PanoramaManager.MediaType.Video));
            }
        }
        
        /// <summary>
        /// 特定のコレクションからプレイリストを作成
        /// </summary>
        public void CreatePlaylistFromCollection(string collectionName)
        {
            if (mediaLibrary == null) return;
            
            currentPlaylist = mediaLibrary.GetContentByCollection(collectionName);
            currentMediaIndex = 0;
            
            if (currentPlaylist.Count > 0)
            {
                LoadMedia(currentPlaylist[0]);
            }
            
            Debug.Log($"Created playlist from collection: {collectionName} ({currentPlaylist.Count} items)");
        }
        
        /// <summary>
        /// 特定のメディアタイプでフィルタリング
        /// </summary>
        public void FilterByMediaType(PanoramaManager.MediaType mediaType)
        {
            if (mediaLibrary == null) return;
            
            currentPlaylist = mediaLibrary.GetContentByType(mediaType);
            currentMediaIndex = 0;
            
            if (currentPlaylist.Count > 0)
            {
                LoadMedia(currentPlaylist[0]);
            }
            
            Debug.Log($"Filtered playlist by type: {mediaType} ({currentPlaylist.Count} items)");
        }
        
        /// <summary>
        /// ランダムメディアを再生
        /// </summary>
        public void PlayRandomMedia()
        {
            if (mediaLibrary == null) return;
            
            PanoramaManager.MediaContent randomContent = mediaLibrary.GetRandomContent();
            if (randomContent != null)
            {
                LoadMedia(randomContent);
            }
        }
        
        /// <summary>
        /// 現在のメディア情報を取得
        /// </summary>
        public PanoramaManager.MediaContent GetCurrentMedia()
        {
            return currentMedia;
        }
        
        /// <summary>
        /// 現在のプレイリスト情報を取得
        /// </summary>
        public List<PanoramaManager.MediaContent> GetCurrentPlaylist()
        {
            return new List<PanoramaManager.MediaContent>(currentPlaylist ?? new List<PanoramaManager.MediaContent>());
        }
        
        /// <summary>
        /// プレイリストの統計情報を取得
        /// </summary>
        public PlaylistInfo GetPlaylistInfo()
        {
            return new PlaylistInfo
            {
                totalItems = currentPlaylist?.Count ?? 0,
                currentIndex = currentMediaIndex,
                shuffleEnabled = shuffleMode,
                repeatEnabled = repeatMode,
                autoPlayEnabled = autoPlayNext
            };
        }
        
        [System.Serializable]
        public class PlaylistInfo
        {
            public int totalItems;
            public int currentIndex;
            public bool shuffleEnabled;
            public bool repeatEnabled;
            public bool autoPlayEnabled;
        }
    }
}