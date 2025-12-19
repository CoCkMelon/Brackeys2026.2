# Unity Template

A lightweight Unity template for beginner projects.  
Focus: clean structure, small modules, easy to copy, easy to delete.

## Requirements
- Unity: Unity 6.3 
- Input: New Input System (recommended) or Both  
- UI: UGUI  
- TextMeshPro for UI text  

## Project Structure
```
Assets/  
    _Project/ # Game-specific code/assets (per project)  
    _Template/ # Reusable template modules  
        Runtime/  
        Core/ # Bootstrap, services  
        UI/ # UI root + stack  
        Scenes/ # Scene loading service  
        Input/ # Input wrapper service  
        Audio/ # Audio service (Mixer + library)  
        Save/ # JSON save service
```

## How to Run (Important)
This template uses a Bootstrap scene to initialize services.  

**Always start Play Mode from `Bootstrap` scene.**

### Scenes (recommended)
- `Bootstrap` (index 0): initializes services, creates UIRoot/@Audio, loads MainMenu  
- `MainMenu` (index 1): menu content only, pushes MainMenuWindow  
- `Game` (index 2): gameplay content, installs game systems (pause, etc.)  

### Build Settings
`File > Build Settings > Scenes In Build`:  

0. Bootstrap  
1. MainMenu  
2. Game  

## Setup Checklist (First time)
### 1) Input System
- `Edit > Project Settings > Player > Active Input Handling` = **Input System Package (New)** (or Both)  
- Create InputActions asset: `Assets/_Project/Settings/GameInput.inputactions`  
- Required action maps & actions:  
  - `Gameplay/Move` (Vector2)  
  - `Gameplay/Pause` (Button, bind Esc)  
  - `UI` (optional, for UI navigation)  

### 2) UIRoot Prefab
Create `Assets/_Project/Prefabs/UI/UIRoot.prefab` with:  
- Canvas (Screen Space Overlay)  
- UIStack object with `UIStack` component  
- LoadingScreen object with `CanvasGroup` + `LoadingScreen` component  
- EventSystem (only ONE in the whole game)  
  - Use `InputSystemUIInputModule`  
  - Remove `StandaloneInputModule` if present  

### 3) Audio
- Create an `AudioMixer` with groups:  
  - Master  
    - BGM  
    - SFX  
- Expose Volume parameters:  
  - Master group Volume -> Exposed Param: `Master`  
  - BGM group Volume -> Exposed Param: `BGM`  
  - SFX group Volume -> Exposed Param: `SFX`  
- Create `AudioLibrary` ScriptableObject and add clips with IDs:  
  - `bgm_menu`, `bgm_game`, `sfx_click` (example)  

### 4) Bootstrapper (Bootstrap scene)
In `Bootstrap` scene, create `@Bootstrap` and attach `Bootstrapper`.  
Assign:  
- `UIRoot Prefab`  
- `InputActionAsset`  
- `AudioMixer`  
- `AudioLibrary`  
- `BGM MixerGroup`, `SFX MixerGroup`  

(Optional) Add `BootstrapEntry` to auto-load `MainMenu` on start.  

## Core Systems Included
### Service Container
Global access:  
- `ServiceContainer.Get<IUIService>()`  
- `ServiceContainer.Get<IInputService>()`  
- `ServiceContainer.Get<ISceneService>()`  
- `ServiceContainer.Get<IAudioService>()`  
- `ServiceContainer.Get<ISaveService>()`  
- `ServiceContainer.Get<IPauseService>()`  

### UI Stack (Push/Pop)
- `UIRoot` holds `UIStack`  
- Windows inherit `UIWindow`  
- Push:  
```csharp
ServiceContainer.Get<IUIService>().Push(windowPrefab);
```
- Pop:  
```csharp
ServiceContainer.Get<IUIService>().Pop();
```

## Pause Service (Reference-count pause)
Use tokens to support multiple pause sources (pause menu + console + any pop up, etc.):  
```csharp
var pause = ServiceContainer.Get<IPauseService>();
var token = pause.Acquire("PauseMenu");
// ...
pause.Release(token);
```

## Scene Loading
```csharp
await ServiceContainer.Get<ISceneService>().LoadSceneAsync("Game");
```
Scene load clears UI stack (prevents menu UI sticking across scenes).  

## Settings Window
Uses sliders (0..1) and saves to JSON  

Applies AudioMixer volumes via exposed params  

Recommended: separate prefabs for menu/game if behavior differs (pause on open etc.)  

## Save System (JSON)

`ISaveService` stores JSON files under `Application.persistentDataPath`:

 - settings: `settings_audio.json` (example)
 - progress: `progress_main.json` (example)

## Testing Guide
### Test A: Only one EventSystem

In Play Mode, search Hierarchy:  

 - `t:EventSystem` => must be exactly 1  

### Test B: Bootstrap creates persistent objects

After entering MainMenu:  

 - there should be 1 `@Bootstrap`
 - 1 `UIRoot(Clone)`
 - 1 `@Audio`

### Test C: MainMenu -> Game

 - Start button triggers loading screen
 - active scene changes to Game
 - UI from menu is cleared

### Test D: Pause in Game

 - Press `Esc` => PauseMenu appears
 - Resume => returns to gameplay

### Test E: Settings Audio

 - open Settings, adjust sliders, BGM/SFX volume changes immediately
 - restart play mode, settings persists

## Common Issues & Fixes

### "Service not found: IUIService"

You started from MainMenu/Game scene directly.  
Fix: always start from Bootstrap scene, or set Bootstrap as Play Mode Start Scene.

### Multiple EventSystems warning

Remove extra EventSystems from MainMenu/Game scenes.
Keep only the one inside `UIRoot`.

## Modules to be updated in the future

 - Object Pool
 - EventBus
 - Debug Console / FPS counter
 - Progress system / achievements
 - Save migration/versioning
