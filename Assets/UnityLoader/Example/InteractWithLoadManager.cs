using UnityEngine;
using UnityEngine.UI;
using UnityLoader;

namespace UnityLoaderExamples
{
	public class InteractWithLoadManager : MonoBehaviour
	{
		public Image loadProgress;
		public Text loadState;

		public Button loadCancelButton;
		public Text loadCancelText;
		public Button pauseResumeButton;
		public Text pauseResumeText;

		private LoadState _state;

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
				LoadManager.instance.RegisterObjectDeep(gameObject);
				LoadManager.instance.StartLoading(OnLoadComplete);
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

		private void OnLoadComplete()
		{
			loadProgress.fillAmount = LoadManager.instance.progress;
			_state = LoadState.Done;
			loadCancelText.text = "Load";
			loadState.text = "Load Complete!";
			pauseResumeButton.gameObject.SetActive(false);
		}
	}
}
