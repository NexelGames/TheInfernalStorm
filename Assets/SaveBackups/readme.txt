Here will be created backup save files on each Application version increment. 

On version increment will be created 1 binary file in SaveBackupsFolder and 1 file in backup gdata folder with JSON format.
Make sure your save works with Newtonsoft JSON serializer or it might throw an exception and it will be handled by try catch

You can swap file that located in SaveSystem.PathToData folder and test behaviour on game update from old version!
	!!! and don't forget to rename swapped file to SaveSystem.FileName from SaveSystem.cs