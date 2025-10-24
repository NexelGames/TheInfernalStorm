@Author base - Sebastian Lague
@Tweaks - Vladislav Kurhei
Last updated 05.07.2023.

How to use guide:
	- Inherit from PoolObject for certain objects, check example of ParticleSystemPoolObject 
	(override OnObjectReuse for your needs, it's automaticaty called when you call ReuseObject from PoolManager)

	- use cast if you need, for example I want to get duration of Reused particle, here is example with ParticleSystemPoolObject:
	float duration = ((ParticleSystemPoolObject)PoolManager.instance.ReuseObject(particlePrefab)).Duration;

	- use DeactivateActivePoolObjects() in PoolManager.Instance to deactivate all active PoolObjects (that inherited from PoolObject.cs)
	- use CreateConstPool() or CreateAutoFillPool() in PoolManager to add pools runtime

----|----|----
Once you created pool object prefabs (they inherit from PoolObject) you can setup pool system:
1) PoolSystem put in load scene of your game and setup it the inspector - set const and auto fill pools
2) PoolSystemCreator put in test scenes (it will create PoolSystem prefab with all your pool objects) but if it's already pool system in game it will be auto-destroyed
3) You can use const size pool or auto fill.
	- Const size will reuse object even if it's in use.
	- Auto fill pool checks if we can reuse free object, if there is no free objects -> new objects will be instantiated.
----|----|----