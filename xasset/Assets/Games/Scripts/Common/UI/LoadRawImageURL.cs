//------------------------------------------------------------------------------
// Copyright (c) 2018-2020 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace HxpGame.UI
{
    using System.Collections;
    using System.IO;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;

    /// <summary>
    /// To load the raw texture from bytes and set to the RawImage.
    /// </summary>
    [AddComponentMenu("UI/Load Raw Image URL")]
    [RequireComponent(typeof(RawImage))]
    [ExecuteInEditMode]
    public sealed class LoadRawImageURL : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The raw image url.")]
        private string url;

        [SerializeField]
        private bool autoFitNativeSize;

        [SerializeField]
        private bool autoUpdateAspectRatio;

        [SerializeField]
        private bool autoDisable = true;

        private RawImage rawImage;

        /// <summary>
        /// Gets or sets a value indicating whether this is auto fit native
        /// size.
        /// </summary>
        public bool AutoFitNativeSize
        {
            get => this.autoFitNativeSize;
            set => this.autoFitNativeSize = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is auto update the
        /// aspect ratio.
        /// </summary>
        public bool AutoUpdateAspectRatio
        {
            get => this.autoUpdateAspectRatio;
            set => this.autoUpdateAspectRatio = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is automatic disable.
        /// </summary>
        public bool AutoDisable
        {
            get => this.autoDisable;
            set => this.autoDisable = value;
        }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string URL
        {
            get => this.url;

            set
            {
                if (!object.Equals(this.url, value))
                {
                    this.url = value;
                    if (!string.IsNullOrEmpty(this.url))
                    {
                        this.ChangeURL(this.url);
                    }
                }
            }
        }

        private void Awake()
        {
            this.rawImage = this.GetComponent<RawImage>();
        }

        private void OnEnable()
        {
            if (this.rawImage.texture == null)
            {
                if (!string.IsNullOrEmpty(this.url))
                {
                    this.ChangeURL(this.url);
                }
                else if (this.autoDisable)
                {
                    this.rawImage.enabled = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (this.rawImage != null)
            {
                this.rawImage.texture = null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.isActiveAndEnabled)
            {
                if (!string.IsNullOrEmpty(this.url))
                {
                    this.ChangeURL(this.url);
                }
            }
        }
#endif

        private void ChangeURL(string url)
        {
            if (url.Contains("://"))
            {
                var request = UnityWebRequestTexture.GetTexture(url, true);
                request.redirectLimit = 0;
                var asyncOpt = request.SendWebRequest();
                asyncOpt.completed += opt => this.OnRequestCompleted(request);
            }
            else
            {
                var bytes = File.ReadAllBytes(url);
                var texture = new Texture2D(
                    2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes, true))
                {
                    this.rawImage.texture = null;
                    if (this.autoDisable)
                    {
                        this.rawImage.enabled = false;
                    }

                    return;
                }

                texture.wrapMode = TextureWrapMode.Clamp;
                this.rawImage.texture = texture;
                if (this.autoDisable)
                {
                    this.rawImage.enabled = true;
                }

                if (this.autoFitNativeSize)
                {
                    this.rawImage.SetNativeSize();
                }

                if (this.autoUpdateAspectRatio)
                {
#if UNITY_2019_2_OR_NEWER
                    if (this.rawImage.TryGetComponent<AspectRatioFitter>(
                        out var ratioFitter))
#else
                    var ratioFitter =
                        this.rawImage.GetComponent<AspectRatioFitter>();
                    if (ratioFitter != null)
#endif
                    {
                        ratioFitter.aspectRatio =
                            (float)texture.width / texture.height;
                    }
                }
            }
        }

        private void OnRequestCompleted(UnityWebRequest request)
        {
            if (request.isNetworkError)
            {
                Debug.LogError("Load image from url failed: " + request.error);
                return;
            }

            if (this == null)
            {
                return;
            }

            if (request.url != this.url)
            {
                return;
            }

            var texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                this.rawImage.texture = texture;
                if (this.autoDisable)
                {
                    this.rawImage.enabled = true;
                }

                if (this.autoFitNativeSize)
                {
                    this.rawImage.SetNativeSize();
                }

                if (this.autoUpdateAspectRatio)
                {
#if UNITY_2019_2_OR_NEWER
                    if (this.rawImage.TryGetComponent<AspectRatioFitter>(
                        out var ratioFitter))
#else
                    var ratioFitter =
                        this.rawImage.GetComponent<AspectRatioFitter>();
                    if (ratioFitter != null)
#endif
                    {
                        ratioFitter.aspectRatio =
                            (float)texture.width / texture.height;
                    }
                }
            }
            else
            {
                this.rawImage.texture = null;
                if (this.autoDisable)
                {
                    this.rawImage.enabled = false;
                }
            }
        }
    }
}
