using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityGameLoader;

namespace UnityGameLoaderExamples
{
	public class LoadMultiplePhasesExample : MonoBehaviour
	{
		public Image loadProgress;
		public Transform parentObject;

		[Space]
		[Range(0f, 1f)]
		public float firstPhasePercentage;

		[Space]
		public GameObject prefabToCreate;
		public int numberToCreate;

		private LoadPhase _loadPhase = LoadPhase.NotStarted;

		private enum LoadPhase
		{
			NotStarted,
			First,
			Second,
			Complete
		}

		public void StartLoading()
		{
			if (_loadPhase == LoadPhase.NotStarted)
			{
				LoadManager.instance.LoadEnumerator(CreateObjects(), FirstPhaseComplete, numberToCreate);
				_loadPhase = LoadPhase.First;
			}
		}

		private void Update()
		{
			if (_loadPhase == LoadPhase.First)
			{
				loadProgress.fillAmount = Mathf.Lerp(0, firstPhasePercentage, LoadManager.instance.progress);
			}
			else if (_loadPhase == LoadPhase.Second)
			{
				loadProgress.fillAmount = Mathf.Lerp(firstPhasePercentage, 1f, LoadManager.instance.progress);
			}
		}

		private void FirstPhaseComplete()
		{
			// Now we register objects that were created. We can't do this while loading.
			LoadManager.instance.RegisterObjectDeep(parentObject.gameObject);
			LoadManager.instance.LoadRegistered(SecondPhaseComplete);
			_loadPhase = LoadPhase.Second;
		}

		private void SecondPhaseComplete()
		{
			_loadPhase = LoadPhase.Complete;
			loadProgress.fillAmount = 1f;
		}

		private IEnumerator CreateObjects()
		{
			for (int i = 0; i < numberToCreate; i++)
			{
				GameObject objCreated = Instantiate(prefabToCreate);
				objCreated.transform.SetParent(parentObject);
				LoadManager.instance.IncrementLoadStep();

				// Pad the timing to show the progress better (only for example!)
				float runUntil = Time.realtimeSinceStartup + 0.5f;

				while (Time.realtimeSinceStartup < runUntil)
				{
					yield return null;
				}
			}
		}
	}
}