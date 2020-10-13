using System;
using Unity.XRTools.ModuleLoader;

namespace Unity.MARS
{
    /// <summary>
    /// Defines the API for a Cloud Data Storage Provider
    /// This functionality provider is responsible for providing a storage in the cloud for
    /// </summary>
    public interface IProvidesCloudDataStorage : IFunctionalityProvider
    {
        /// <summary>
        /// Get the current state of the connection to the cloud storage
        /// </summary>
        /// <returns>True if the Cloud Storage is connected to this client, false otherwise.</returns>
        bool IsConnected();

        /// <summary>
        /// Set the current authentication token
        /// </summary>
        void SetAPIKey(string key);

        /// <summary>
        /// Get the current authentication token
        /// </summary>
        string GetAPIKey();

        /// <summary>
        /// Set the current project identifier
        /// </summary>
        void SetProjectIdentifier(string id);

        /// <summary>
        /// Set the current project identifier
        /// </summary>
        string GetProjectIdentifier();

        /// <summary>
        /// Save to the cloud asynchronously the data of an object of a certain type with a specified key
        /// </summary>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="serializedObject"> string serialization of the object being saved. </param>
        /// <param name="callback"> a callback when the asynchronous call is done to show whether it was successful,
        /// with the response code and string. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        void CloudSaveAsync(string key, string serializedObject, Action<bool, long, string> callback, ProgressCallback progress = null);

        /// <summary>
        /// Save to the cloud asynchronously data in a byte array with a specified key
        /// </summary>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="bytesObject">Bytes array of the object being saved</param>
        /// <param name="callback"> a callback when the asynchronous call is done to show whether it was successful,
        /// with the response code and string. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        void CloudSaveAsync(string key, byte[] bytesObject, Action<bool, long, string> callback, ProgressCallback progress = null);

        /// <summary>
        /// Load from the cloud asynchronously the data of an object of a certain type which was saved with a known key
        /// </summary>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="callback">a callback which returns whether the operation was successful, as well as the
        /// response code and serialized string of the object if it was. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        void CloudLoadAsync(string key, Action<bool, long, string> callback, ProgressCallback progress = null);

        /// <summary>
        /// Load from the cloud asynchronously the byte array which was saved with a known key
        /// </summary>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="callback">a callback which returns whether the operation was successful, as well as the
        /// response code and byte array if it was. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        void CloudLoadAsync(string key, Action<bool, long, byte[]> callback, ProgressCallback progress = null);
    }
}
