﻿using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Provides cloud data storage service that can save and load data from the cloud.
    /// </summary>
    public interface IUsesCloudDataStorage : IFunctionalitySubscriber<IProvidesCloudDataStorage>
    {
    }

    /// <summary>
    /// Extension methods for cloud data storage users
    /// </summary>
    public static class IUsesCloudDataStorageMethods
    {
        /// <summary>
        /// Get the current state of the connection to the cloud storage
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <returns>True if the Cloud Storage is connected to this client, false otherwise.</returns>
        public static bool IsConnected(this IUsesCloudDataStorage user)
        {
            return user.provider.IsConnected();
        }

        /// <summary>
        /// Set the current API key
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="token">The API key to set</param>
        public static void SetAPIKey(this IUsesCloudDataStorage user, string token)
        {
            user.provider.SetAPIKey(token);
        }

        /// <summary>
        /// Get the current API key
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <returns>The current API key</returns>
        public static string GetAPIKey(this IUsesCloudDataStorage user)
        {
            return user.provider.GetAPIKey();
        }


        /// <summary>
        /// Set the current project identifier
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="id">The project identifier to set</param>
        public static void SetProjectIdentifier(this IUsesCloudDataStorage user, string id)
        {
            user.provider.SetProjectIdentifier(id);
        }

        /// <summary>
        /// Get the current project identifier
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <returns>The current project identifier</returns>
        public static string GetProjectIdentifier(this IUsesCloudDataStorage user)
        {
            return user.provider.GetProjectIdentifier();
        }

        /// <summary>
        /// Save to the cloud asynchronously the data of an object with a specified key
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="serializedObject"> string serialization of the object being saved. </param>
        /// <param name="callback"> a callback when the asynchronous call is done to show whether it was successful. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        public static void CloudSaveAsync(this IUsesCloudDataStorage user, string key, string serializedObject,
            Action<bool, long, string> callback, ProgressCallback progress = null)
        {
            user.provider.CloudSaveAsync(key, serializedObject, callback, progress);
        }

        /// <summary>
        /// Load from the cloud asynchronously the data of an object which was saved with a known key
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="callback">a callback which returns whether the operation was successful, as well as the
        /// serialized string of the object if it was. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        public static void CloudLoadAsync(this IUsesCloudDataStorage user, string key, Action<bool, long, string> callback,
            ProgressCallback progress = null)
        {
            user.provider.CloudLoadAsync(key, callback, progress);
        }

        /// <summary>
        /// Save to the cloud asynchronously the data of an object of a certain type with a specified key
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="bytes"> bytes to save. </param>
        /// <param name="callback"> a callback when the asynchronous call is done to show whether it was successful. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        public static void CloudSaveAsync(this IUsesCloudDataStorage user, string key, byte[] bytes,
            Action<bool, long, string> callback, ProgressCallback progress = null)
        {
            user.provider.CloudSaveAsync(key, bytes, callback, progress);
        }

        /// <summary>
        /// Load from the cloud asynchronously the data of an object which was saved with a known key
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="key"> string that uniquely identifies this instance of the type. </param>
        /// <param name="callback">a callback which returns whether the operation was successful, as well as the
        /// response payload if it was. </param>
        /// <param name="progress">Called every frame while the request is in progress with two 0-1 values indicating
        /// upload and download progress, respectively</param>
        public static void CloudLoadAsync(this IUsesCloudDataStorage user, string key, Action<bool, long, byte[]> callback,
            ProgressCallback progress = null)
        {
            user.provider.CloudLoadAsync(key, callback, progress);
        }
    }
}
