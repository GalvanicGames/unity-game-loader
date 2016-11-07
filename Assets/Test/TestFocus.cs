using UnityEngine;
using System.Collections;
using UnityGameLoader;

namespace UnityGameLoaderTests
{
	public class TestFocus : MonoBehaviour, IAssetLoader
	{
		public float loadDuration = 10;

		private void Start()
		{
			LoadManager.CreateManager(1f / 30);
			LoadManager.instance.RegisterObject(gameObject);
			LoadManager.instance.LoadRegistered(() => Debug.Log("Loading Complete!"));
		}

		private void OnApplicationFocus(bool focus)
		{
			Debug.Log("Focus Change: " + focus);
		}

		public IEnumerator LoadAssets()
		{
			float loadTime = Time.realtimeSinceStartup + loadDuration;

			while (Time.realtimeSinceStartup < loadTime)
			{
				Debug.Log("Frame: " + Time.renderedFrameCount);

				float delayTime = Time.realtimeSinceStartup + 1f / 30;

				while (Time.realtimeSinceStartup < delayTime)
				{

				}

				yield return null;
			}
		}
	}
}