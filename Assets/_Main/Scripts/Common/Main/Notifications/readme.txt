<uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED" />    - add to AndroidManifest to keep notifications after device restart
Also set Reschedule on Device Restart bool to true in Mobile Notifications settings (in Project settings -> Mobile Notifications -> Android)

called from AppStart.cs at Start(), but call line commented by default (uncomment it to use)