using System.Collections;

/// <summary>
/// The interface classes much implement in order to used by the LoadManager.
/// </summary>
public interface IAssetLoader
{
	/// <summary>
	/// The method that will be called when it's the class' turn to load. Should yield when a frame break would be
	/// appropriate. (Like a coroutine).
	/// </summary>
	/// <returns></returns>
	IEnumerator LoadAssets();

	/// <summary>
	/// The method that will be called once all registered objects have had their assets loaded.
	/// </summary>
	void AssetsLoaded();
}
