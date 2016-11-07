using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityGameLoader;

namespace UnityGameLoaderExamples
{
	public class InteractWithLoadManagerExample : MonoBehaviour
	{
		public Image loadProgress;
		public Text loadState;

		public Button loadCancelButton;
		public Text loadCancelText;
		public Button pauseResumeButton;
		public Text pauseResumeText;

		public Text enumText;

		private LoadState _state;
		private bool _runRegisteredObjs = true;

		private const int STEPS_FOR_ENUMERATOR = 11;

		private enum LoadState
		{
			Ready,
			Loading,
			Paused,
			Done
		}

		// Use this for initialization
		void Start()
		{
			loadCancelButton.gameObject.SetActive(true);
			pauseResumeButton.gameObject.SetActive(false);
			loadCancelText.text = "Load";
			loadState.text = "Not Loaded";
		}

		// Update is called once per frame
		void Update()
		{
			if (_state == LoadState.Loading)
			{
				loadProgress.fillAmount = LoadManager.instance.progress;
			}
		}

		// Invoked from Unity UI
		public void LoadCancel()
		{
			if (_state == LoadState.Loading || _state == LoadState.Paused)
			{
				LoadManager.instance.CancelLoading();
				loadState.text = "Canceled!";
				pauseResumeButton.gameObject.SetActive(false);
				loadState.gameObject.SetActive(true);
				loadCancelText.text = "Load";
				_state = LoadState.Ready;
			}
			else
			{
				if (_runRegisteredObjs)
				{
					LoadManager.instance.RegisterEnumerator(EnumeratorToLoad(), STEPS_FOR_ENUMERATOR);
					LoadManager.instance.RegisterObjectDeep(gameObject);

					// Can load all the registered objects
					LoadManager.instance.LoadRegistered(OnLoadComplete);
				}
				else
				{
					// Or we can load arbitrary enumerators where we step the progress counter ourselves. In this case
					// we know we'll run 11 steps (incremented in the enumerator)
					LoadManager.instance.LoadEnumerator(EnumeratorToLoad(), OnLoadComplete, STEPS_FOR_ENUMERATOR);
				}

				loadState.text = "Loading...";
				pauseResumeButton.gameObject.SetActive(true);
				loadCancelText.text = "Cancel";
				pauseResumeText.text = "Pause";
				_state = LoadState.Loading;
			}
		}

		public void PauseResume()
		{
			if (_state == LoadState.Paused)
			{
				LoadManager.instance.ResumeLoading();
				pauseResumeText.text = "Pause";
				loadState.text = "Loading...";
				_state = LoadState.Loading;
			}
			else
			{
				LoadManager.instance.PauseLoading();
				pauseResumeText.text = "Resume";
				loadState.text = "Paused";
				_state = LoadState.Paused;
			}
		}

		public void LoadTypeChanged(bool runRegObjs)
		{
			_runRegisteredObjs = runRegObjs;
		}

		private void OnLoadComplete()
		{
			loadProgress.fillAmount = LoadManager.instance.progress;
			_state = LoadState.Done;
			loadCancelText.text = "Load";
			loadState.text = "Load Complete!";
			pauseResumeButton.gameObject.SetActive(false);
		}

		private IEnumerator EnumeratorToLoad()
		{
			enumText.color = Color.red;

			for (int i = 0; i < 10; i++)
			{
				float runUntil = Time.realtimeSinceStartup + 1;
				enumText.text = i.ToString();

                while (Time.realtimeSinceStartup < runUntil)
				{
					yield return null;
				}

				LoadManager.instance.IncrementLoadStep();
			}

			enumText.text = "10";

			// Can call into other enumerators
			yield return SpinForSecond();

			enumText.text = "Complete!";
			enumText.color = Color.green;
		}

		private IEnumerator SpinForSecond()
		{
			float runUntil = Time.realtimeSinceStartup + 1;

			while (Time.realtimeSinceStartup < runUntil)
			{
				yield return null;
			}

			LoadManager.instance.IncrementLoadStep();
		}
	}
}
