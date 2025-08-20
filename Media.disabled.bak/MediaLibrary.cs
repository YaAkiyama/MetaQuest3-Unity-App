using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using System.IO;

namespace MQ3VRApp.Media
{
    /// <summary>
    /// メディアライブラリ管理クラス
    /// 360度画像・動画コンテンツの管理とロード機能を提供
    /// </summary>
    [CreateAssetMenu(fileName = "MediaLibrary", menuName = "MQ3VRApp/Media Library")]
    public class MediaLibrary : ScriptableObject
    {
        [Header("Media Collections")]
        [SerializeField] private List<MediaCollection> mediaCollections = new List<MediaCollection>();
        
        [Header("Default Content")]
        [SerializeField] private PanoramaManager.MediaContent defaultContent;
        
        [Header("Loading Settings")]
        [SerializeField] private bool autoLoadFromStreamingAssets = true;
        [SerializeField] private string streamingAssetsPath = "VRMedia";
        [SerializeField] private bool enableAsyncLoading = true;
        
        [System.Serializable]
        public class MediaCollection
        {
            public string collectionName;
            public string description;
            public List<PanoramaManager.MediaContent> contents = new List<PanoramaManager.MediaContent>();
            public bool isEnabled = true;
            public Sprite collectionThumbnail;
            
            [Header("Collection Settings")]
            public bool allowShuffle = true;
            public bool autoPlay = false;
            public float autoPlayInterval = 30f;
        }
        
        public delegate void MediaLibraryEvent(PanoramaManager.MediaContent content);
        public static event MediaLibraryEvent OnMediaLoaded;
        public static event MediaLibraryEvent OnMediaLoadFailed;
        
        private static MediaLibrary instance;
        public static MediaLibrary Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<MediaLibrary>("MediaLibrary");
                    if (instance == null)
                    {
                        Debug.LogWarning("MediaLibrary not found in Resources folder");
                    }
                }
                return instance;
            }
        }
        
        private void OnEnable()
        {
            if (autoLoadFromStreamingAssets)
            {
                LoadContentFromStreamingAssets();
            }
        }
        
        /// <summary>
        /// StreamingAssetsフォルダからメディアコンテンツを自動ロード
        /// </summary>
        private void LoadContentFromStreamingAssets()
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);
            
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"Streaming assets path not found: {fullPath}");
                return;
            }
            
            // 画像ファイルの読み込み
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tga" };
            foreach (string extension in imageExtensions)
            {
                string[] imageFiles = Directory.GetFiles(fullPath, "*" + extension);
                foreach (string imagePath in imageFiles)
                {
                    LoadImageFromFile(imagePath);
                }
            }
            
            // 動画ファイルの読み込み
            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".webm" };
            foreach (string extension in videoExtensions)
            {
                string[] videoFiles = Directory.GetFiles(fullPath, "*" + extension);
                foreach (string videoPath in videoFiles)
                {
                    LoadVideoFromFile(videoPath);
                }
            }
        }
        
        /// <summary>
        /// ファイルパスから画像を読み込み
        /// </summary>
        private void LoadImageFromFile(string filePath)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                
                if (texture.LoadImage(fileData))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    
                    PanoramaManager.MediaContent content = new PanoramaManager.MediaContent
                    {
                        name = fileName,
                        type = PanoramaManager.MediaType.Image,
                        image = texture,
                        description = $"Loaded from: {fileName}",
                        isDefault = false
                    };
                    
                    AddContentToCollection("Streaming Assets", content);
                    OnMediaLoaded?.Invoke(content);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load image from {filePath}: {e.Message}");
            }
        }
        
        /// <summary>
        /// ファイルパスから動画を読み込み
        /// </summary>
        private void LoadVideoFromFile(string filePath)
        {
            try
            {
                // VideoClipはランタイムでの動的ロードが制限されているため、
                // ファイルパスのみを保存し、実際の再生はVideoPlayer.urlを使用
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                PanoramaManager.MediaContent content = new PanoramaManager.MediaContent
                {
                    name = fileName,
                    type = PanoramaManager.MediaType.Video,
                    description = $"Video file: {fileName}",
                    isDefault = false
                };
                
                AddContentToCollection("Streaming Assets", content);
                OnMediaLoaded?.Invoke(content);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load video from {filePath}: {e.Message}");
            }
        }
        
        /// <summary>
        /// 指定したコレクションにコンテンツを追加
        /// </summary>
        public void AddContentToCollection(string collectionName, PanoramaManager.MediaContent content)
        {
            MediaCollection collection = GetOrCreateCollection(collectionName);
            collection.contents.Add(content);
        }
        
        /// <summary>
        /// コレクションを取得または新規作成
        /// </summary>
        private MediaCollection GetOrCreateCollection(string collectionName)
        {
            MediaCollection collection = mediaCollections.Find(c => c.collectionName == collectionName);
            
            if (collection == null)
            {
                collection = new MediaCollection
                {
                    collectionName = collectionName,
                    description = $"Auto-generated collection: {collectionName}",
                    isEnabled = true
                };
                mediaCollections.Add(collection);
            }
            
            return collection;
        }
        
        /// <summary>
        /// 全てのメディアコンテンツを取得
        /// </summary>
        public List<PanoramaManager.MediaContent> GetAllContent()
        {
            List<PanoramaManager.MediaContent> allContent = new List<PanoramaManager.MediaContent>();
            
            foreach (var collection in mediaCollections)
            {
                if (collection.isEnabled)
                {
                    allContent.AddRange(collection.contents);
                }
            }
            
            return allContent;
        }
        
        /// <summary>
        /// 指定したコレクションのコンテンツを取得
        /// </summary>
        public List<PanoramaManager.MediaContent> GetContentByCollection(string collectionName)
        {
            MediaCollection collection = mediaCollections.Find(c => c.collectionName == collectionName);
            return collection?.contents ?? new List<PanoramaManager.MediaContent>();
        }
        
        /// <summary>
        /// コンテンツを名前で検索
        /// </summary>
        public PanoramaManager.MediaContent FindContentByName(string contentName)
        {
            foreach (var collection in mediaCollections)
            {
                if (!collection.isEnabled) continue;
                
                PanoramaManager.MediaContent content = collection.contents.Find(c => c.name == contentName);
                if (content != null) return content;
            }
            
            return null;
        }
        
        /// <summary>
        /// タイプ別でコンテンツを取得
        /// </summary>
        public List<PanoramaManager.MediaContent> GetContentByType(PanoramaManager.MediaType mediaType)
        {
            List<PanoramaManager.MediaContent> filteredContent = new List<PanoramaManager.MediaContent>();
            
            foreach (var collection in mediaCollections)
            {
                if (!collection.isEnabled) continue;
                
                foreach (var content in collection.contents)
                {
                    if (content.type == mediaType)
                    {
                        filteredContent.Add(content);
                    }
                }
            }
            
            return filteredContent;
        }
        
        /// <summary>
        /// ランダムなコンテンツを取得
        /// </summary>
        public PanoramaManager.MediaContent GetRandomContent()
        {
            List<PanoramaManager.MediaContent> allContent = GetAllContent();
            
            if (allContent.Count == 0) return null;
            
            int randomIndex = Random.Range(0, allContent.Count);
            return allContent[randomIndex];
        }
        
        /// <summary>
        /// デフォルトコンテンツを取得
        /// </summary>
        public PanoramaManager.MediaContent GetDefaultContent()
        {
            if (defaultContent != null) return defaultContent;
            
            // デフォルトが設定されていない場合、最初のコンテンツを返す
            List<PanoramaManager.MediaContent> allContent = GetAllContent();
            return allContent.Count > 0 ? allContent[0] : null;
        }
        
        /// <summary>
        /// 全コレクション情報を取得
        /// </summary>
        public List<MediaCollection> GetAllCollections()
        {
            return new List<MediaCollection>(mediaCollections);
        }
        
        /// <summary>
        /// コレクションの有効/無効を切り替え
        /// </summary>
        public void SetCollectionEnabled(string collectionName, bool enabled)
        {
            MediaCollection collection = mediaCollections.Find(c => c.collectionName == collectionName);
            if (collection != null)
            {
                collection.isEnabled = enabled;
            }
        }
        
        /// <summary>
        /// 新しいコレクションを作成
        /// </summary>
        public MediaCollection CreateCollection(string name, string description = "")
        {
            MediaCollection newCollection = new MediaCollection
            {
                collectionName = name,
                description = description,
                isEnabled = true
            };
            
            mediaCollections.Add(newCollection);
            return newCollection;
        }
        
        /// <summary>
        /// コレクションを削除
        /// </summary>
        public bool RemoveCollection(string collectionName)
        {
            MediaCollection collection = mediaCollections.Find(c => c.collectionName == collectionName);
            if (collection != null)
            {
                mediaCollections.Remove(collection);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// メディアライブラリの統計情報を取得
        /// </summary>
        public LibraryStats GetLibraryStats()
        {
            int totalContent = 0;
            int totalImages = 0;
            int totalVideos = 0;
            int enabledCollections = 0;
            
            foreach (var collection in mediaCollections)
            {
                if (collection.isEnabled)
                {
                    enabledCollections++;
                    totalContent += collection.contents.Count;
                    
                    foreach (var content in collection.contents)
                    {
                        if (content.type == PanoramaManager.MediaType.Image)
                            totalImages++;
                        else if (content.type == PanoramaManager.MediaType.Video)
                            totalVideos++;
                    }
                }
            }
            
            return new LibraryStats
            {
                totalCollections = mediaCollections.Count,
                enabledCollections = enabledCollections,
                totalContent = totalContent,
                totalImages = totalImages,
                totalVideos = totalVideos
            };
        }
        
        [System.Serializable]
        public class LibraryStats
        {
            public int totalCollections;
            public int enabledCollections;
            public int totalContent;
            public int totalImages;
            public int totalVideos;
        }
    }
}