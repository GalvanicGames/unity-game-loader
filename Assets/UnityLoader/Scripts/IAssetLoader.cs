using System.Collections;

public interface IAssetLoader
{
	IEnumerator LoadAssetsCo();
	void AssetsLoaded();
}
