# enkidu: simple adventure game framework for unity

This is a simple Unity-based framework for 2D point and click adventure games. 

### Features
- fully animated 2d characters (body, head, face, mouth, eyebrows, eyes, emotions, ...)
- lip sync (Preston-Blair phoneme/viseme set, Rogo Lipsync Pro)
- navmesh control
- multi-point character scaling
- dynamic camera (with drag character follow)
- save games
- cutscene tools
- inventory
- broad interactive item support (hotspots, doors, switches, full screen puzzles, item combines, popups, custom actions, ...)

### Used In 
[Elroy and the Aliens](https://elroythegame.com)

### TODO
refactoring, add a sample scene, documentation, testing

## main classes

### Player
[PlayerBrain](Assets/Motiviti/Enkidu/character/PlayerBrain.cs), [PlayerHead](Assets/Motiviti/Enkidu/character/PlayerHead.cs), [PlayerMouth](Assets/Motiviti/Enkidu/character/PlayerMouth.cs), [CharacterBrain](Assets/Motiviti/Enkidu/character/CharacterBrain.cs), [CharacterHead](Assets/Motiviti/Enkidu/character/CharacterHead.cs), [CharacterMouth](Assets/Motiviti/Enkidu/character/CharacterMouth.cs), [Player](Assets/Motiviti/Enkidu/character/Player.cs)

Inheritance: PlayerBrain<-CharacterBrain, PlayerHead<-CharacterHead, PlayerMouth<-CharacterMouth

### Environment 
[InteractiveItem](Assets/Motiviti/Enkidu/environment/InteractiveItem.cs), [InteractiveItemAction](Assets/Motiviti/Enkidu/environment/InteractiveItemAction.cs), [InteractiveItem***](Assets/Motiviti/Enkidu/environment)

Inheritance: InteractiveItem***<-InteractiveItemAction

### GUI
[Inventory](Assets/Motiviti/Enkidu/gui/Inventory.cs)

### System 
[StatefulItem](Assets/Motiviti/Enkidu/system/StatefulItem.cs), [PersistentEngine/Deprecate](Assets/Motiviti/Enkidu/system/PersistentEngine.cs)
