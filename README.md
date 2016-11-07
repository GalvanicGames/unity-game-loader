# Unity Game Loader
=======================
A package that allows for easy loading of game assets across frames to allow for animation/progress bar while the game is loading.

####Obtain!####
[Releases](https://github.com/GalvanicGames/unity-game-loader/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/GalvanicGames/unity-game-loader/archive/master.zip) and copy the files in Assets to your project's Assets folder.

## Setup

Once the Unity Game Loader asset has been imported into the project then add the LoadManager component to a GameObject in the scene.

### Load Manager Properties ###

**Seconds Allowed Per Frame** - The time allowed before the loader will yield to the next frame allowed a render frame to update. This is only used if the application has focus.

**Memory Threshold For Yield** - WebGL specific. Since garbage generation counts against the total memory allowed for webgl (and the collector only runs in between frames), this specifies the amount of memory (in bytes) that can be used before the loader will force a yield to the next frame (regardless of how much time has passed). Experimentation is needed to find a good value here.

**Verbose Logging** - Logs information about the loader including how long objects take to load.

### Create Load Manager Through Code ###

As an option, instead of adding a LoadManager component to an object, the following function can be invoked to automatically create a LoadManager in the scene during runtime.

```csharp
static void CreateManager(
  float secondsAllowedPerFrame,
  int webglMemoryThresholdForYield = DEFAULT_WEBGL_MEMORY_THRESHOLD, // ~128MB
  bool verboseLogging = false);
```

**Singleton**

Once created (or when Awake() is invoked) the Load Manager instance can be obtained through the instance property.

```csharp
static LoadManager instance
```

## Usage

The load manager works by taking enumerators and advancing them automatically if the frame doesn't need to yield yet and yielding to the next frame when appropriate.

When implementing a loading enumerator, use 'yield return null' to indicate that a frame could yield here if it is need (a good place would be to place a 'yield return null' between objects that are created). A loading enumerator can also yield another enumerator which will then be loaded.

**Example**

```csharp
public class Example : MonoBehaviour
{
	public GameObject prefabToCreate;
	public int numberToCreate;

	// Use this for initialization
	void Start()
	{
		LoadManager.instance.RegisterEnumerator(EnumeratorToLoad());
		LoadManager.instance.LoadRegistered(OnLoadComplete);
	}

	private void OnLoadComplete()
	{
		Debug.Log("Load Complete!");
	}

	private IEnumerator EnumeratorToLoad()
	{
		for (int i = 0; i < numberToCreate; i++)
		{
			Instantiate(prefabToCreate);
			yield return null;
		}

		// Can call into other enumerators
		yield return LoadOtherEnumerator();
    
    // Can continue to load here, will be invoked when LoadOtherEnumerator finishes
	}

	private IEnumerator LoadOtherEnumerator()
	{
		// Can do whatever loading here too
		yield return null;
	}
}
```

## Load Manager Members

### Registration ###

Enumerators and Components (and classes) that implement IAssetLoader can be registered with the Load Manager and then loaded in the order that they are registered.

```csharp
void RegisterEnumerator(IEnumerator enumeratorToRegister, int additionalSteps = 0)
```
Register the enumerator to be loaded later. See section 'Progress Steps' for more information about additionalSteps.

```csharp
void RegisterObject(GameObject objToRegister, int additionalSteps = 0)
```
Register the GameObject to be loaded later. Will grab all components that implement IAssetLoader.

```csharp
void RegisterObject(IAssetLoader loaderToRegister, int additionalSteps = 0)
```
Register the IAssetLoader interface to be loaded later. Can be any class that implements the interface.

```csharp
void RegisterObjectDeep(GameObject objToRegister)
```
Grabs all the components of the GameObject and it's hierarchy and registers all components that implement IAssetLoader. Includes objects that are inactive.

```csharp
void ClearRegistration()
```
Clear all registered GameObjects and enumerators. This happens automatically after loading has been completed.

### Loading ###

Once the known enumerators and GameObjects have been registered with the Load Manager, loading can be started.

```csharp
void LoadRegistered(System.Action onLoadComplete)
```
Loads the registered enumerator and GameObjects in the order they were received and invokes onLoadComplete once loading has finished. Further registration is disabled until loading has been canceled or completed.

```csharp
void LoadEnumerator(IEnumerator enumeratorToLoad, System.Action onLoadComplete, int additionalSteps)
```
Loads a single enumerator. This allows bypassing registration if only a single enumerator needs to be loaded. This will not load any currently registered objects.

**Progress**

```csharp
float progress
```
Between 0 and 1, represents the currently progress of the loading.

### Control ###

```csharp
void PauseLoading()
```
Pauses if the Load Manager is currently loading.

```csharp
void ResumeLoading()
```
Resumes loading if the Load Manager was paused.

```csharp
void CancelLoading()
```
Cancels loading if the Load Manager is currently loading. This does not clear the registered objects.

### Progress Steps ###

The Load Manager's progress will tick up with every registered enumerator and Game Object that is completed. For finer control within a given enumerator or GameObject, a number for additionalSteps can be supplied during registration. The expectation from the Load Manager is that the enumerator or GameObject will then increment the step counter during its load using the following function:

```csharp
void IncrementLoadStep()
```

## IAssetLoader Interface

Components that implement the IAssetLoader interface will be registered with the containing GameObject is registered with the Load Manager. The interface only has only member to implement.

```csharp
IEnumerator LoadAssets()
```

## Custom Yield Instructions

```csharp
class ForceYield : YieldInstruction
```

Can yield return this class to force a render frame to occur. The Load Manager will not skip this yield.

## Multiple Phases?

In order for the progress bar to make sense, the number of steps is necessary to know before loading begins. Because of this registration is disabled while the Load Manager is loading. If multiple phases is necessary (say creating objects and then registering the created objects to load additional content) then load an enumerator that creates the objects, then register the created objects, and then load the registered objects.

For an example, see the ExampleMultiplePhases scene.
