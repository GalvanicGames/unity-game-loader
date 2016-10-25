using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityGameLoader
{
	/// <summary>
	/// Handles the loading process.
	/// </summary>
	public class LoadManager : MonoBehaviour
	{
		#region public
		/// <summary>
		/// The amount of time the loader will execute before yield to a new frame.
		/// </summary>
		public float secondsAllowedPerFrame = 1f / 30;

		/// <summary>
		/// Amount of memory System.GC.GetTotalMemory() can get to before yield to a new frame.
		/// </summary>
		[Header("WebGL")]
		public int memoryThresholdForYield = DEFAULT_WEBGL_MEMORY_THRESHOLD;

		/// <summary>
		/// Verbose print out the timing of each loaded object.
		/// </summary>
		[Header("Debug")]
		public bool verboseLogging;

		/// <summary>
		/// The progress the loader has made through the loading. From 0 to 1.
		/// </summary>
		public float progress
		{
			get { return (float)_currentStep / _totalSteps; }
		}

		/// <summary>
		/// The currently active loader.
		/// </summary>
		public static LoadManager instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<LoadManager>();

					if (_instance == null)
					{
						LogErrorFormat(
							"No instance of {0} can be found! Add the {0} component to a GameObject or invoke {0}.{1}().",
							typeof(LoadManager),
							((System.Action<float, int, bool>)CreateManager).Method.Name);
					}
				}

				return _instance;
			}
		}

		/// <summary>
		/// Creates an instance of a LoadManager. This can be done instead of having one precreated in the scene.
		/// </summary>
		/// <param name="secondsAllowedPerFrame">The amount of time the loader will execute before yield to a new frame.</param>
		/// <param name="webglMemoryThresholdForYield">Amount of memory System.GC.GetTotalMemory() can get to before yield to a new frame.</param>
		/// <param name="verboseLogging">Verbose print out the timing of each loaded object.</param>
		public static void CreateManager(
			float secondsAllowedPerFrame,
			int webglMemoryThresholdForYield = DEFAULT_WEBGL_MEMORY_THRESHOLD,
			bool verboseLogging = false)
		{
			if (_instance != null)
			{
				LogWarningFormat(
					"{0} already exists, called to {1} will be ignored!",
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}

			LoadManager newLoad = (new GameObject(CREATED_NAME)).AddComponent<LoadManager>();
			newLoad.secondsAllowedPerFrame = secondsAllowedPerFrame;
			newLoad.memoryThresholdForYield = webglMemoryThresholdForYield;
			newLoad.verboseLogging = verboseLogging;
		}

		/// <summary>
		/// Register an object to be loaded later.
		/// </summary>
		/// <param name="objToRegister">Object to register.</param>
		/// <param name="additionalSteps">How many steps does this object take to load?</param>
		public void RegisterObject(GameObject objToRegister, int additionalSteps = 0)
		{
			RegisterObject(objToRegister.GetComponent<IAssetLoader>(), additionalSteps);
		}

		/// <summary>
		/// Register the interface to be loaded later.
		/// </summary>
		/// <param name="loaderToRegister">The interface to register.</param>
		/// <param name="additionalSteps">How many steps does this interface take to load?</param>
		public void RegisterObject(IAssetLoader loaderToRegister, int additionalSteps = 0)
		{
			if (_isLoading)
			{
				LogWarningFormat(
					METHOD_INVOKE_DURING_LOADING_WARNING,
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}

			if (loaderToRegister != null)
			{
				_loaders.Add(new LoaderStep(loaderToRegister, additionalSteps));
			}
		}

		/// <summary>
		/// Deeply register a GameObject and all of it's children.
		/// </summary>
		/// <param name="objToRegister">Object to deeply register.</param>
		public void RegisterObjectDeep(GameObject objToRegister)
		{
			if (_isLoading)
			{
				LogWarningFormat(
					METHOD_INVOKE_DURING_LOADING_WARNING,
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}

			IAssetLoader[] loaders = objToRegister.GetComponentsInChildren<IAssetLoader>(true);

			for (int i = 0; i < loaders.Length; i++)
			{
				RegisterObject(loaders[i]);
			}
		}

		/// <summary>
		/// Begin the loading process. Will set up and invoke the IAssetLoader interface for all registered objects.
		/// </summary>
		/// <param name="onLoadComplete">Callback to be invoked when loading is complete.</param>
		public void StartLoading(System.Action onLoadComplete)
		{
			if (_isLoading)
			{
				LogWarningFormat(
					METHOD_INVOKE_DURING_LOADING_WARNING,
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}


			// Extra step for calling load complete
			StartCoroutine(LoadAssetsCo(onLoadComplete));
		}

		/// <summary>
		/// Pause the loading that is in progress.
		/// </summary>
		public void PauseLoading()
		{
			if (!_isLoading)
			{
				LogWarningFormat(
					METHOD_INVOKE_NOT_LOADING_WARNING,
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}

			_loadingPaused = true;
		}

		/// <summary>
		/// Resume loading that was paused.
		/// </summary>
		public void ResumeLoading()
		{
			if (!_isLoading)
			{
				LogWarningFormat(
					METHOD_INVOKE_NOT_LOADING_WARNING,
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}

			_loadingPaused = false;
		}

		/// <summary>
		/// Cancel loading that is in progress.
		/// </summary>
		public void CancelLoading()
		{
			if (!_isLoading)
			{
				LogWarningFormat(
					METHOD_INVOKE_NOT_LOADING_WARNING,
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}

			StopAllCoroutines();
			_loaders.Clear();
			_loadingPaused = false;
			_isLoading = false;
			Application.runInBackground = _originalRunInBackground;
		}

		/// <summary>
		/// Increment the progress step counter. This allows the property progress to accurately reflect its progress.
		/// </summary>
		public void IncrementLoadStep()
		{
			if (!_isLoading)
			{
				LogWarningFormat(
					METHOD_INVOKE_NOT_LOADING_WARNING,
					typeof(LoadManager),
					System.Reflection.MethodBase.GetCurrentMethod().Name);

				return;
			}

			_currentStep++;
		}

		#endregion
		#region private

		private int _totalSteps;
		private int _currentStep;

		private bool _loadingPaused;
		private bool _hasFocus = true;
		private bool _isLoading;
		private bool _originalRunInBackground;

		private List<LoaderStep> _loaders = new List<LoaderStep>();

		private static LoadManager _instance;
		private static bool _webglInitialized;

		private const int DEFAULT_WEBGL_MEMORY_THRESHOLD = 128000000;
		private const float NO_FOCUS_SECONDS_PER_FRAME = 1;
		private const int ADDITIONAL_STEPS = 2;
		private const string LOG_HEADER = "[Unity Loader]";
		private const string CREATED_NAME = "[Unity Loader]";
		private const string METHOD_INVOKE_DURING_LOADING_WARNING = "{0}.{1} invoked in the middle of a load! This isn't allowed. Invoke after the load finishes.";
		private const string METHOD_INVOKE_NOT_LOADING_WARNING = "{0}.{1} invoked when not loading! This will be ignored.";

		private struct LoaderStep
		{
			public IAssetLoader loader;
			public int additionalSteps;

			public LoaderStep(IAssetLoader loader, int additionalSteps)
			{
				this.loader = loader;
				this.additionalSteps = additionalSteps;
			}
		}

		[DllImport("__Internal", EntryPoint = "ulInitialize")]
		private static extern void JSInitialize();

		[DllImport("__Internal", EntryPoint = "ulIsTabActive")]
		private static extern bool JSIsTabActive();

		private void Awake()
		{
			if (_instance != null)
			{
				LogErrorFormat(
					"Only one {1} allowed in the scene at a time!",
					typeof(LoadManager));

				return;
			}

			_instance = this;

			if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isEditor && !_webglInitialized)
			{
				JSInitialize();
				_webglInitialized = true;
			}
		}

		private void OnApplicationFocus(bool focus)
		{
			_hasFocus = focus;
		}

		private IEnumerator LoadAssetsCo(System.Action onLoadComplete)
		{
			_originalRunInBackground = Application.runInBackground;
			Application.runInBackground = true;
			float setupTimeStart = Time.realtimeSinceStartup;
			float frameStartTime = Time.realtimeSinceStartup;
			_isLoading = true;
			_currentStep = 0;

			// We consider the set up an additional step as well invoking asset load complete a step.
			_totalSteps = _loaders.Count + ADDITIONAL_STEPS;

			for (int i = 0; i < _loaders.Count; i++)
			{
				_totalSteps += _loaders[i].additionalSteps;
			}

			if (verboseLogging)
			{
				LogFormat("Setup - Time: {0}", Time.realtimeSinceStartup - setupTimeStart);
			}

			if (ShouldYield(frameStartTime))
			{
				yield return null;
				frameStartTime = Time.realtimeSinceStartup;
			}

			IncrementLoadStep();
			Stack<IEnumerator> enumerators = new Stack<IEnumerator>();

			for (int i = 0; i < _loaders.Count; i++)
			{
				enumerators.Clear();
				enumerators.Push(_loaders[i].loader.LoadAssets());
				int preEnumSteps = _currentStep;

				float assetLoadTimeStart = Time.realtimeSinceStartup;

				while (enumerators.Count > 0)
				{
					if (_loadingPaused)
					{
						yield return null;
						continue;
					}

					bool forceYield = false;
					IEnumerator currentEnumerator = enumerators.Peek();

					if (currentEnumerator.MoveNext())
					{
						IEnumerator yieldEnum = currentEnumerator.Current as IEnumerator;

						if (yieldEnum != null)
						{
							enumerators.Push(yieldEnum);
						}
						else
						{
							forceYield = currentEnumerator.Current is ForceYield;
						}
					}
					else
					{
						enumerators.Pop();
					}

					if (forceYield || ShouldYield(frameStartTime))
					{
						yield return null;
						frameStartTime = Time.realtimeSinceStartup;
					}
				}

				if (_currentStep - preEnumSteps != _loaders[i].additionalSteps)
				{
					LogWarningFormat(
						"Progress step count mismatch! Expecting {0} to be invoked the same number supplied during registration! Fixing.",
						((System.Action)IncrementLoadStep).Method.Name);

					_currentStep = preEnumSteps + _loaders[i].additionalSteps;
				}

				IncrementLoadStep();

				if (verboseLogging)
				{
					LogFormat("Loading {0} - Time: {1}", _loaders[i].loader, Time.realtimeSinceStartup - assetLoadTimeStart);
				}
			}

			for (int i = 0; i < _loaders.Count; i++)
			{
				if (_loadingPaused)
				{
					yield return null;
					i--;
					continue;
				}

				float loadedTimeStart = Time.realtimeSinceStartup;
				_loaders[i].loader.AssetsLoaded();

				if (verboseLogging)
				{
					LogFormat("Loaded {0} - Time: {1}", _loaders[i].loader, Time.realtimeSinceStartup - loadedTimeStart);
				}

				if (ShouldYield(frameStartTime))
				{
					yield return null;
					frameStartTime = Time.realtimeSinceStartup;
				}
			}

			IncrementLoadStep();

			if (onLoadComplete != null)
			{
				onLoadComplete();
			}

			_loaders.Clear();
			_loadingPaused = false;
			_isLoading = false;
			Application.runInBackground = _originalRunInBackground;
		}

		private bool ShouldYield(float frameStartTime)
		{
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				if (!Application.isEditor)
				{
					if (System.GC.GetTotalMemory(false) > memoryThresholdForYield)
					{
						if (verboseLogging)
						{
							LogFormat("Yielding due to memory!");
						}

						return true;
					}

					if (!JSIsTabActive())
					{
						return false;
					}
				}
			}
			else
			{
				if (!Application.isEditor && !_hasFocus)
				{
					if (Time.realtimeSinceStartup - frameStartTime >= NO_FOCUS_SECONDS_PER_FRAME)
					{
						return true;
					}

					// We don't have focus, go nuts.
					return false;
				}
			}

			if ((Time.realtimeSinceStartup - frameStartTime) >= secondsAllowedPerFrame)
			{
				return true;
			}

			return false;
		}

		private static void LogFormat(string msg, params object[] args)
		{
			Debug.LogFormat(LOG_HEADER + " " + msg, args);
		}

		private static void LogWarningFormat(string msg, params object[] args)
		{
			Debug.LogWarningFormat(LOG_HEADER + " " + msg, args);
		}

		private static void LogErrorFormat(string msg, params object[] args)
		{
			Debug.LogErrorFormat(LOG_HEADER + " " + msg, args);
		}

		#endregion
	}
}
