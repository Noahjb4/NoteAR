using System;
using System.Collections.Generic;
using System.Text;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.MARS
{
    [ScriptableSettingsPath(MARSCore.UserSettingsFolder)]
    public class GoogleCloudDataStorageModule : ScriptableSettings<GoogleCloudDataStorageModule>, IModuleBehaviorCallbacks, IProvidesCloudDataStorage
    {
#if UNITY_EDITOR
        [Serializable]
        struct APIKeyResponse
        {
#pragma warning disable 649
            public string apiKey;
#pragma warning restore 649
        }

        const string k_APIKeyURL = "https://build-api.cloud.unity3d.com/api/v1/users/me/apiKey";
#endif

        const string k_PathToBucket = "https://storage.googleapis.com/mars-g9m0l3hvla";
        const string k_GoogleCloudStorageCacheControlHeaderOptions = "public, no-cache, no-store, max-age=0";

#pragma warning disable 649, 414
        [SerializeField]
        bool m_SyncAPIKey = true;

        [SerializeField]
        string m_ProjectIdentifier;

        [SerializeField]
        string m_APIKey;

        [SerializeField]
        bool m_OfflineMode;

        [SerializeField]
        float m_TestingDelay;
#pragma warning restore 649, 414

        long m_LastHttpResponseCode = -1;
        readonly List<RequestHandle> m_Requests = new List<RequestHandle>();

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<RequestHandle> k_RequestsCopy = new List<RequestHandle>();

        enum HttpMethods
        {
            // HTTP Methods are case-sensitive
            GET,
            PUT,
            DELETE
        }

        static class HttpContentTypes
        {
            public const string ApplicationOctetStream = "application/octet-stream";
            public const string TextPlain = "text/plain";
        }

        enum ContentTypes
        {
            ByteArray,
            String
        }

        struct RequestHandle
        {
            public UnityWebRequest Request;
            public Action<bool, long, byte[]> Callback;
            public ProgressCallback Progress;
            public float StartTime;

            public RequestHandle(UnityWebRequest request, Action<bool, long, byte[]> callback, ProgressCallback progress)
            {
                Request = request;
                Callback = callback;
                Progress = progress;
                StartTime = Time.realtimeSinceStartup;
            }
        }

        public void SetAPIKey(string key) { m_APIKey = key; }
        public string GetAPIKey() { return m_APIKey; }

        public void SetProjectIdentifier(string id)
        {
            m_ProjectIdentifier = id;
        }

        public string GetProjectIdentifier()
        {
            return m_ProjectIdentifier;
        }

        public void CloudSaveAsync(string key, string serializedObject, Action<bool, long, string> callback, ProgressCallback progress = null)
        {
            CloudSaveAsyncBytes(key, Encoding.UTF8.GetBytes(serializedObject), callback, progress, ContentTypes.String);
        }

        public void CloudSaveAsync(string key, byte[] bytesObject, Action<bool, long, string> callback, ProgressCallback progress = null)
        {
            CloudSaveAsyncBytes(key, bytesObject, callback, progress);
        }

        void CloudSaveAsyncBytes(string key, byte[] bytesObject, Action<bool, long, string> callback, ProgressCallback progress, ContentTypes contentType = ContentTypes.ByteArray)
        {
            if (m_OfflineMode)
            {
                callback?.Invoke(false, 0, null);
                return;
            }

            var httpMethod = HttpMethods.PUT;
            if (bytesObject== null || bytesObject.Length == 0)
            {
                httpMethod = HttpMethods.DELETE;
            }

            var request = RequestBuilder(httpMethod, key, contentType, bytesObject);

            void ConvertResponseToString(bool success, long responseCode, byte[] bytes)
            {
                string response = null;
                if (bytes != null)
                    response = Encoding.UTF8.GetString(bytes);

                callback(success, responseCode, response);
            }

            SendWebRequest(request, ConvertResponseToString, progress);
        }

        public void CloudLoadAsync(string key, Action<bool, long, string> callback, ProgressCallback progress = null)
        {
            void ConvertResponseToString(bool success, long responseCode, byte[] bytes)
            {
                string response = null;
                if (bytes != null)
                    response = Encoding.UTF8.GetString(bytes);

                callback(success, responseCode, response);
            }

            CloudLoadAsyncBytes(key, ConvertResponseToString, progress, ContentTypes.String);
        }

        public void CloudLoadAsync(string key, Action<bool, long, byte[]> callback, ProgressCallback progress = null)
        {
            CloudLoadAsyncBytes(key, callback, progress);
        }

        void CloudLoadAsyncBytes(string key, Action<bool, long, byte[]> callback, ProgressCallback progress, ContentTypes contentType = ContentTypes.ByteArray)
        {
            if (m_OfflineMode)
            {
                progress?.Invoke(1, 1);
                callback?.Invoke(false, 0, null);
                return;
            }

            var request = RequestBuilder(HttpMethods.GET, key, contentType);
            SendWebRequest(request, callback, progress);
        }

        void SendWebRequest(UnityWebRequest request, Action<bool, long, byte[]> callback, ProgressCallback progress = null)
        {
            request.SendWebRequest();
            m_Requests.Add(new RequestHandle(request, callback, progress));
        }

        UnityWebRequest RequestBuilder(HttpMethods httpMethod, string key, ContentTypes contentType, byte[] payload = null)
        {
            var projectId = m_ProjectIdentifier;

            if (!ProjectIdentifierIsValid(projectId))
            {
                Debug.Log("Invalid project identifier");
            }

            var uri = new Uri($"{k_PathToBucket}/projects/{projectId}/{key}");
            var request = new UnityWebRequest(uri, httpMethod.ToString());
            request.SetRequestHeader("Content-Type", contentType != ContentTypes.String ?
                HttpContentTypes.ApplicationOctetStream : HttpContentTypes.TextPlain);

            request.SetRequestHeader("Cache-Control", k_GoogleCloudStorageCacheControlHeaderOptions);
            if (httpMethod == HttpMethods.GET)
            {
                var downloadHandler = new DownloadHandlerBuffer();
                request.downloadHandler = downloadHandler;
            }

            if (payload != null && payload.Length > 0)
            {
                request.uploadHandler = new UploadHandlerRaw(payload);
            }

            return request;
        }

        /// <summary>
        /// Checks if the provided Project Identifier is valid
        /// </summary>
        /// <param name="projectID"></param>
        /// <returns>True if the project identifier is valid, false otherwise.</returns>
        static bool ProjectIdentifierIsValid(string projectID)
        {
            // Basic check, to be made more comprehensive with Genesis
            return (projectID != null && projectID.Length > 32);
        }

        void UpdateRequests()
        {
            if (m_Requests.Count == 0)
                return;

            k_RequestsCopy.Clear();
            k_RequestsCopy.AddRange(m_Requests);
            var time = Time.realtimeSinceStartup;
            foreach (var requestHandle in k_RequestsCopy)
            {
                var request = requestHandle.Request;
                requestHandle.Progress?.Invoke(request.uploadProgress, request.downloadProgress);
                if (time - requestHandle.StartTime < m_TestingDelay)
                    continue;

                if (request.isDone)
                {
                    var callback = requestHandle.Callback;
                    var responseCode = request.responseCode;
                    m_LastHttpResponseCode = responseCode;
                    try
                    {
                        var downloadHandler = request.downloadHandler;
                        if (request.isNetworkError)
                            callback(false, responseCode, Encoding.UTF8.GetBytes(request.error));
                        else
                            callback(responseCode == 200, responseCode, downloadHandler?.data);

                        downloadHandler?.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    m_Requests.Remove(requestHandle);
                }
            }
        }

        public void LoadModule()
        {
#if UNITY_EDITOR
            EditorApplication.update += UpdateRequests;
#endif

            if (string.IsNullOrEmpty(m_ProjectIdentifier))
            {
                m_ProjectIdentifier = Application.cloudProjectId;
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(m_ProjectIdentifier))
                    EditorUtility.SetDirty(this);
#endif
            }

#if UNITY_EDITOR
            if (m_SyncAPIKey && string.IsNullOrEmpty(m_APIKey))
                SyncAPIKey();
#endif
        }

        public void UnloadModule()
        {
#if UNITY_EDITOR
            EditorApplication.update -= UpdateRequests;
#endif
        }

        public void OnBehaviorAwake() { }

        public void OnBehaviorEnable() { }

        public void OnBehaviorStart() { }

        public void OnBehaviorUpdate()
        {
#if !UNITY_EDITOR
            UpdateRequests();
#endif
        }

        public void OnBehaviorDisable() { }

        public void OnBehaviorDestroy() { }

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesCloudDataStorage>(obj);
        }

        public void UnloadProvider() { }

        public bool IsConnected()
        {
            // Check for 0 internal function failed, 408 timeout, and 429 too many requests
            // Other errors may be present, but may not be connection specific (e.g. 404 not found)
            // TODO: Increase coverage of test
            return !m_OfflineMode && m_LastHttpResponseCode != 0 && m_LastHttpResponseCode != 408 && m_LastHttpResponseCode != 429;
        }

        public void AuthenticatedGetRequest(string uri, string authToken, Action<bool, string> callback)
        {
            var request = new UnityWebRequest(uri, "GET");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.downloadHandler = new DownloadHandlerBuffer();
            SendWebRequest(request, (success, responseCode, bytes) =>
            {
                if (success)
                {
                    var str = Encoding.UTF8.GetString(bytes);
                    callback(true, str);
                    return;
                }

                callback(false, null);
            });
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_SyncAPIKey)
                SyncAPIKey();
        }

        void SyncAPIKey()
        {
            GetAPIKey((success, apiKey) =>
            {
                if (success)
                {
                    m_APIKey = apiKey;
                    EditorUtility.SetDirty(this);
                }
            });
        }

        void GetAPIKey(Action<bool, string> callback)
        {
            if (callback == null)
            {
                Debug.LogError("GetAPIKey called with no callback");
                return;
            }

            AuthenticatedGetRequest(k_APIKeyURL, CloudProjectSettings.accessToken, (success, response) =>
            {
                if (success)
                {
                    var apiKeyResponse = JsonUtility.FromJson<APIKeyResponse>(response);
                    callback(true, apiKeyResponse.apiKey);
                    return;
                }

                callback(false, null);
            });
        }
#endif
    }
}
