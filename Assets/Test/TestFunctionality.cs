using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using UnityGameLoader;

namespace UnityGameLoaderTests
{
	public class TestFunctionality : MonoBehaviour
	{
		public int targetFPS = 30;

		private void Awake()
		{
			LoadManager.CreateManager(targetFPS);
		}

		//
		// Test Correct Steps
		//

		private const int STEPS_USED = 10;

		public class TestCorrectStepsClass : IAssetLoader
		{
			public IEnumerator LoadAssets()
			{
				for (int i = 0; i < STEPS_USED; i++)
				{
					LoadManager.instance.IncrementLoadStep();
					yield return null;
				}

				yield return null;
			}

			public void AssetsLoaded()
			{

			}
		}

		public void TestCorrectSteps()
		{
			TestCorrectStepsClass test = new TestCorrectStepsClass();
			LoadManager.instance.RegisterObject(test, STEPS_USED);
			LoadManager.instance.StartLoading(() => Assert.IsTrue(LoadManager.instance.progress == 1f));
		}

		//
		// Test Too Few Steps
		//

		public class TestTooFewStepsClass : IAssetLoader
		{
			public IEnumerator LoadAssets()
			{
				for (int i = 0; i < STEPS_USED; i++)
				{
					if (i % 2 == 0)
					{
						LoadManager.instance.IncrementLoadStep();
					}
					
					yield return null;
				}

				yield return null;
			}

			public void AssetsLoaded()
			{

			}
		}

		public void TestTooFewSteps()
		{
			TestTooFewStepsClass test = new TestTooFewStepsClass();
			LoadManager.instance.RegisterObject(test, STEPS_USED);
			LoadManager.instance.StartLoading(() => Assert.IsTrue(LoadManager.instance.progress == 1f));
		}

		//
		// Test Too Few Steps
		//

		public class TestTooManyStepsClass : IAssetLoader
		{
			public IEnumerator LoadAssets()
			{
				for (int i = 0; i < STEPS_USED; i++)
				{
					LoadManager.instance.IncrementLoadStep();
					yield return null;
				}

				LoadManager.instance.IncrementLoadStep();
				yield return null;
			}

			public void AssetsLoaded()
			{

			}
		}

		public void TestTooManySteps()
		{
			TestTooManyStepsClass test = new TestTooManyStepsClass();
			LoadManager.instance.RegisterObject(test, STEPS_USED);
			LoadManager.instance.StartLoading(() => Assert.IsTrue(LoadManager.instance.progress == 1f));
		}

		//
		// Test Force Yield
		//

		public class TestForceYieldClass : IAssetLoader
		{
			public IEnumerator LoadAssets()
			{
				float time = Time.realtimeSinceStartup + 10;

				while (Time.realtimeSinceStartup < time)
				{
					Debug.Log("Frame: " + Time.renderedFrameCount);
					yield return new ForceYield();
				}
			}

			public void AssetsLoaded()
			{

			}
		}

		public void TestForceYield()
		{
			TestForceYieldClass test = new TestForceYieldClass();
			LoadManager.instance.secondsAllowedPerFrame = 100;
			LoadManager.instance.RegisterObject(test);
			LoadManager.instance.StartLoading(() => Assert.IsTrue(LoadManager.instance.progress == 1f));
		}
	}
}