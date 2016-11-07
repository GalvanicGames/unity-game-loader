using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityGameLoader;

namespace UnityGameLoaderExamples
{
	public class AssetLoaderImplementer : MonoBehaviour, IAssetLoader
	{
		public Image image;
		public Text text;

		private int _steps;

		private void Start()
		{
			image.color = Color.red;
			text.text = "0";
		}

		public IEnumerator LoadAssets()
		{
			// We load our assets here and yield at appropriate frame breaks!
			image.color = Color.red;
			_steps = 0;

			text.text = _steps.ToString();

			yield return null;

			// We might take quite a bit of time to load stuff, who knows!
			float timeToLoad = Time.realtimeSinceStartup + Random.value * 2;

			while (Time.realtimeSinceStartup < timeToLoad)
			{
				// Doing super important load stuff!
				yield return null;
			}

			_steps++;
			text.text = _steps.ToString();

			// We can also call other enumerators to let them do their job.
			yield return LoadOtherAssetsFunction();

			_steps++;
			text.text = _steps.ToString();

			// There's no guarantee that yield will force a frame to pass. Maybe we want to? We can!
			yield return new ForceYield();

			_steps++;
			text.text = _steps.ToString();
		}

		private IEnumerator LoadOtherAssetsFunction()
		{
			_steps++;
			text.text = _steps.ToString();

			// This can go as deep as we want
			yield return LoadYetAnotherFunction();

			// And we'll resume when that enumerator completely finishes!
			_steps++;
			text.text = _steps.ToString();
		}

		private IEnumerator LoadYetAnotherFunction()
		{
			// Good times.
			yield return null;
			_steps++;
			text.text = _steps.ToString();
		}
	}
}