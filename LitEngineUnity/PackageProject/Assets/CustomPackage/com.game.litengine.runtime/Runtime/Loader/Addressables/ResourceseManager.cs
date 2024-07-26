using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement;
namespace LitEngine.Addressable
{
    public class ResourcesManager : MonoManagerGeneric<ResourcesManager>
    {
        protected override void Init()
        {

        }

         /// <summary>
        /// Loads multiple assets, identified by a set of keys.
        /// </summary>
        /// <remarks>
        /// The keys in <paramref name="keys"/> are translated into lists of locations, which are merged into a single list based on
        /// the value in <paramref name="mode"/>.
        ///
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the assets
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// If any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">IEnumerable set of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        ///
        /// When true, if any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// When false, if any matching location fails, the `Result` instance in the returned operation handle contains an IList of size equal to the number of
        /// locations that the operation attempted to load. The entry in the result list corresponding to a location that failed to load is null.
        /// The entries for locations that successfully loaded are set to a valid TObject. The `Status` of the operation handle is still <see cref="AsyncOperationStatus.Failed"/>
        /// if any single asset failed to load.
        ///
        /// When <paramref name="releaseDependenciesOnFailure"/> is true, you do not need to release the <see cref="AsyncOperationHandle"/> instance on failure.
        /// When false, you must always release it.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        static public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback,
            Addressables.MergeMode mode = Addressables.MergeMode.Union, bool releaseDependenciesOnFailure = true)
        {
            var thandle = Addressables.LoadAssetsAsync(keys, callback, mode, releaseDependenciesOnFailure);
            thandle.Completed += (handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    DLog.LogError("[ResourcesManager] LoadAssetsAsync Failed.");
                }
            };
            return thandle;
        }
        
        /// <summary>
        /// Instantiate a single object.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>AsyncOperationHandle that is used to check when the operation has completed. The result of the operation is a GameObject.</returns>
        static public AsyncOperationHandle<GameObject> InstantiateAssetAsync(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            var thandle = Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
            thandle.Completed += (handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    DLog.LogError("[ResourcesManager] InstantiateAssetAsync Failed.");
                }
            };
            return thandle;
        }
        
        /// <summary>
        /// Release asset.
        /// </summary>
        /// <typeparam name="TObject">The type of the object being released</typeparam>
        /// <param name="obj">The asset to release.</param>
        public static void Release<TObject>(TObject obj)
        {
            Addressables.Release(obj);
        }

        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <typeparam name="TObject">The type of the AsyncOperationHandle being released</typeparam>
        /// <param name="handle">The operation handle to release.</param>
        public static void Release<TObject>(AsyncOperationHandle<TObject> handle)
        {
            Addressables.Release(handle);
        }
        
        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <param name="handle">The operation handle to release.</param>
        public static void Release(AsyncOperationHandle handle)
        {
            Addressables.Release(handle);
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="instance">The GameObject instance to be released and destroyed.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(GameObject instance)
        {
            return Addressables.ReleaseInstance(instance);
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(AsyncOperationHandle handle)
        {
            Addressables.Release(handle);
            return true;
        }
        
    }
}