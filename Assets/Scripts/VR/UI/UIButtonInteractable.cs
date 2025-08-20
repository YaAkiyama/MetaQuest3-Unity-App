using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using TMPro;

namespace MQ3VRApp.UI
{
    /// <summary>
    /// VR用UIボタンのインタラクション処理
    /// ホバーとクリック時のエフェクトを提供
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonInteractable : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color hoverColor = new Color(0.3f, 0.5f, 0.8f, 0.9f);
        [SerializeField] private Color pressedColor = new Color(0.1f, 0.3f, 0.6f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        [Header("Animation")]
        [SerializeField] private bool useScaleAnimation = true;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float pressedScale = 0.95f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Audio")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private float volume = 0.5f;
        
        [Header("Haptic Feedback")]
        [SerializeField] private bool useHapticFeedback = true;
        [SerializeField] private float hapticIntensity = 0.1f;
        [SerializeField] private float hapticDuration = 0.1f;
        
        private Button button;
        private Image buttonImage;
        private TextMeshProUGUI buttonText;
        private AudioSource audioSource;
        
        private Vector3 originalScale;
        private bool isHovering = false;
        private bool isPressed = false;
        private bool isInteractable = true;
        
        // アニメーション用
        private Coroutine currentAnimation;
        
        private void Awake()
        {
            InitializeComponents();
            SetupButton();
        }
        
        private void InitializeComponents()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            
            // AudioSourceの作成
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volume;
            audioSource.spatialBlend = 1f; // 3D Audio
            
            originalScale = transform.localScale;
        }
        
        private void SetupButton()
        {
            // ボタンの色設定
            if (buttonImage != null)
            {
                buttonImage.color = normalColor;
            }
            
            // ボタンイベントの追加
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        private void OnEnable()
        {
            UpdateVisualState();
        }
        
        /// <summary>
        /// XRレイインタラクターからのホバー開始
        /// </summary>
        public void OnHoverEnter()
        {
            if (!isInteractable) return;
            
            isHovering = true;
            PlayHoverSound();
            SendHapticFeedback(hapticIntensity * 0.5f, hapticDuration * 0.5f);
            UpdateVisualState();
        }
        
        /// <summary>
        /// XRレイインタラクターからのホバー終了
        /// </summary>
        public void OnHoverExit()
        {
            isHovering = false;
            UpdateVisualState();
        }
        
        private void OnButtonClicked()
        {
            if (!isInteractable) return;
            
            PlayClickSound();
            SendHapticFeedback(hapticIntensity, hapticDuration);
            StartPressAnimation();
        }
        
        private void UpdateVisualState()
        {
            Color targetColor;
            float targetScale = 1f;
            
            if (!isInteractable)
            {
                targetColor = disabledColor;
                targetScale = 1f;
            }
            else if (isPressed)
            {
                targetColor = pressedColor;
                targetScale = pressedScale;
            }
            else if (isHovering)
            {
                targetColor = hoverColor;
                targetScale = hoverScale;
            }
            else
            {
                targetColor = normalColor;
                targetScale = 1f;
            }
            
            // 色の変更
            if (buttonImage != null)
            {
                buttonImage.color = targetColor;
            }
            
            // スケールアニメーション
            if (useScaleAnimation)
            {
                AnimateScale(targetScale);
            }
        }
        
        private void AnimateScale(float targetScale)
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(ScaleAnimation(targetScale));
        }
        
        private System.Collections.IEnumerator ScaleAnimation(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = originalScale * targetScale;
            
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);
                
                transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
                yield return null;
            }
            
            transform.localScale = endScale;
            currentAnimation = null;
        }
        
        private void StartPressAnimation()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(PressAnimation());
        }
        
        private System.Collections.IEnumerator PressAnimation()
        {
            isPressed = true;
            
            // Press down
            Vector3 startScale = transform.localScale;
            Vector3 pressScale = originalScale * pressedScale;
            
            float elapsed = 0f;
            float halfDuration = animationDuration * 0.5f;
            
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(startScale, pressScale, progress);
                yield return null;
            }
            
            // Press up
            elapsed = 0f;
            Vector3 endScale = isHovering ? originalScale * hoverScale : originalScale;
            
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(pressScale, endScale, progress);
                yield return null;
            }
            
            transform.localScale = endScale;
            isPressed = false;
            currentAnimation = null;
        }
        
        private void PlayHoverSound()
        {
            if (hoverSound != null && audioSource != null)
            {
                audioSource.clip = hoverSound;
                audioSource.Play();
            }
        }
        
        private void PlayClickSound()
        {
            if (clickSound != null && audioSource != null)
            {
                audioSource.clip = clickSound;
                audioSource.Play();
            }
        }
        
        private void SendHapticFeedback(float intensity, float duration)
        {
            if (!useHapticFeedback) return;
            
            // 最も近いコントローラーに振動フィードバックを送信
            UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor[] controllers = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>();
            if (controllers.Length > 0)
            {
                UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor nearestController = controllers[0];
                float nearestDistance = Vector3.Distance(transform.position, nearestController.transform.position);
                
                foreach (var controller in controllers)
                {
                    float distance = Vector3.Distance(transform.position, controller.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestController = controller;
                    }
                }
                
                nearestController.SendHapticImpulse(intensity, duration);
            }
        }
        
        /// <summary>
        /// ボタンの有効/無効を設定
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            if (button != null)
            {
                button.interactable = interactable;
            }
            UpdateVisualState();
        }
        
        /// <summary>
        /// ボタンのテキストを設定
        /// </summary>
        public void SetText(string text)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }
        
        /// <summary>
        /// ボタンの色テーマを設定
        /// </summary>
        public void SetColorTheme(Color normal, Color hover, Color pressed)
        {
            normalColor = normal;
            hoverColor = hover;
            pressedColor = pressed;
            UpdateVisualState();
        }
        
        /// <summary>
        /// オーディオクリップを設定
        /// </summary>
        public void SetAudioClips(AudioClip hover, AudioClip click)
        {
            hoverSound = hover;
            clickSound = click;
        }
        
        /// <summary>
        /// ハプティック設定
        /// </summary>
        public void SetHapticSettings(bool enabled, float intensity, float duration)
        {
            useHapticFeedback = enabled;
            hapticIntensity = intensity;
            hapticDuration = duration;
        }
    }
}