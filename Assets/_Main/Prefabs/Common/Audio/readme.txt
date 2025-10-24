SoundPool is used for PoolManager

SoundNotPool is used in scenes and etc (it has SoundAudioClip auto initialization on Awake())
Alternatively you can use Sound from PoolManager.ReuseSound() and play it without destroy,
but you should init it with SoundAudioClip to use Sound events (soundPlayed and etc)!