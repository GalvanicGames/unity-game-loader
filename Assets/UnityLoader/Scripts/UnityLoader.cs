using UnityEngine;
using System.Collections.Generic;

public static class UnityLoader
{
	private static int _totalSteps;
	private static int _currentStep;

	private static List<LoaderStep> _loaders = new List<LoaderStep>();

	private static GameObject _helper;

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

	public static float progress
	{
		get { return (float)_currentStep / _totalSteps; }
	}

	public static void RegisterObject(GameObject objToRegister, int additionalSteps = 0)
	{
		RegisterObject(objToRegister.GetComponent<IAssetLoader>(), additionalSteps);
	}

	public static void RegisterObject(IAssetLoader loaderToRegister, int additionalSteps = 0)
	{
		if (loaderToRegister != null)
		{

			_loaders.Add(new LoaderStep(loaderToRegister, additionalSteps));
		}
	}

	public static void RegisterObjectDeep(GameObject objToRegister)
	{
		IAssetLoader[] loaders = objToRegister.GetComponentsInChildren<IAssetLoader>(true);

		for (int i = 0; i < loaders.Length; i++)
		{
			RegisterObject(loaders[i]);
		}
	}

	public static void StartLoading(System.Action onLoadComplete)
	{
		_totalSteps = _loaders.Count;

		for (int i = 0; i < _loaders.Count; i++)
		{
			_totalSteps += _loaders[i].additionalSteps;
		}


	}

	public static void CancelLoading()
	{
		
	}

	public static void IncrementLoadStep()
	{
		_currentStep++;
	}
}
