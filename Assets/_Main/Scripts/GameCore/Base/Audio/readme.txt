--- In short ---
AudioManager - singleton, you must call it to play any sound.
It has SoundsEnabled property to toggle sound on/off


--- Setup ---
Add sound prefab (SoundPool) in PoolSystem.cs under soundPrefab menu (Pool Size depends on game type)
Create script where you want to call sound. Add SoundAudioClip field in your custom script and call AudioManager.PlaySound/OneShot/At(soundAudioData) when needed


--- 2D/3D ---
Set is2D to true to use 2D sound in SoundAudioClip and false to use 3D


--- Prefabs ---
SoundPool is used for PoolManager

SoundNotPool is used in scenes and etc (it has SoundAudioClip auto initialization on Awake())
Alternatively you can use Sound from PoolManager.ReuseSound() and play it without destroy,
but you should init it with SoundAudioClip to use Sound events (soundPlayed and etc)!