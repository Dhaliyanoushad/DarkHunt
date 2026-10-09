# Dark Hunt

A first-person 3D game where **your real head movement controls the flashlight**. A webcam tracks your face, Python works out where you are looking, and Unity points the torch in the same direction.

```
Webcam -> MediaPipe Face Landmarker (Python) -> yaw,pitch over UDP (127.0.0.1:5005) -> Unity
```

## Features

- Dark 3D room lit only by a spot-light flashlight with a slight flicker
- Head tracking with MediaPipe (yaw and pitch) sent to Unity over UDP
- Calibration, smoothing and a dead zone for stable tracking
- First-person movement with W/A/S/D, plus mouse or Q/E turning

> Status: the head-controlled flashlight and player movement work. Hidden objects, timer, levels and win/game-over screens are still in progress.

## Repository layout

```
DarkHunt/
├── Assets/
│   ├── Scenes/SampleScene.unity     <- the game scene
│   └── Scripts/
│       ├── HeadFlashlight.cs        <- turns camera + torch from head data
│       ├── PlayerMove.cs            <- WASD movement and mouse/Q/E turning
│       └── UDPReceiver.cs           <- listens for yaw,pitch on port 5005
├── ComputerVision/
│   ├── udp_sender.py                <- webcam -> head pose -> UDP
│   ├── face_landmarker.task         <- MediaPipe model file
│   └── requirements.txt
├── Packages/
└── ProjectSettings/
```

## Requirements

| Tool | Version |
|---|---|
| Unity Hub + Unity Editor | Unity 6 (6000.6.0f1 was used) |
| Python | **3.12** (3.11 also works; 3.13 is not reliably supported by MediaPipe) |
| Git + Git LFS | any recent version |
| Webcam | any, with decent lighting |
| OS | Windows 10/11 (tested) |

## Installation

### 1. Clone the repository

This repo uses Git LFS for some binary files, so install it first:

```bash
git lfs install
git clone https://github.com/Dhaliyanoushad/DarkHunt.git
cd DarkHunt
```

### 2. Set up the Python head tracker

```bash
cd ComputerVision
py -3.12 -m venv .venv
.venv\Scripts\activate.bat
python -m pip install --upgrade pip
python -m pip install -r requirements.txt
```

If you use PowerShell and activation is blocked, run `Set-ExecutionPolicy -Scope Process Bypass` first, then `.venv\Scripts\Activate.ps1`.

`requirements.txt` should contain:

```
opencv-python
numpy
mediapipe
```

Check the install:

```bash
python -c "import cv2, numpy, mediapipe; from mediapipe.tasks import python; from mediapipe.tasks.python import vision; print('all good')"
```

If `face_landmarker.task` is missing from the `ComputerVision` folder, download it from:
`https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task`

### 3. Open the Unity project

1. Open **Unity Hub**, click **Add -> Add project from disk**, and choose the cloned `DarkHunt` folder.
2. Open it with Unity 6 (accept the version prompt if it differs slightly). The first load takes a few minutes while Unity rebuilds its `Library` folder.
3. Open `Assets/Scenes/SampleScene`.
4. Go to **Edit -> Project Settings -> Player -> Other Settings** and set **Active Input Handling** to **Both** (Unity restarts).

### 4. Check the scene setup

If anything was lost while cloning, make sure the scene looks like this:

- **Player** (capsule) with a **Character Controller** and **Player Move (Script)**, at position `0, 1, 0`
- **Main Camera** as a child of Player
- **Flashlight** (Spot Light) as a child of Main Camera, with **Head Flashlight (Script)**:
  - **View Camera** = Main Camera
  - **Use Fake Input** = off
- An empty object named **UDP** with **UDP Receiver (Script)**, **Port** = `5005`

Attach scripts with **Add Component** in the Inspector. Dragging a script into the Hierarchy creates a separate empty object instead.

## Running the game

Always start Unity first, then Python.

1. In Unity, press **Play**.
2. In a terminal inside `ComputerVision` (with the virtual environment activated):
   ```bash
   python udp_sender.py
   ```
   A webcam window opens showing Yaw and Pitch.
3. **Click once inside the Unity Game view.** The webcam window steals keyboard focus, so W/A/S/D only works after you click Unity.
4. In the top-left of the Game view you should see `Yaw ... Pitch ... packets ...`, with `packets` counting up.
5. Look straight at the screen and press **C in the webcam window** to centre. The first face seen is also auto-calibrated.
6. Turn your head left, right, up and down. The flashlight follows.
7. Stop with **Q** in the webcam window, then press Play again in Unity to stop the game.

## Controls

| Input | Action |
|---|---|
| W / A / S / D | Walk |
| Mouse, or Q / E | Turn the player |
| Head left / right | Turn the view and flashlight |
| Head up / down | Look up / down |
| C (webcam window) | Re-centre head tracking |
| Q (webcam window) | Quit the head tracker |
| Esc | Release the mouse cursor (click the Game view to lock again) |

## Settings you can tune

**In Unity (Flashlight -> Head Flashlight):**

- `Smoothing` - higher is snappier, lower is smoother
- `Invert Yaw` / `Invert Pitch` - fix reversed left/right or up/down
- `Head Turns Body` - on: turning your head also turns the player; off: head only moves the camera and torch

**In `ComputerVision/udp_sender.py`:**

- `SMOOTHING` (default `0.35`) - responsiveness of the tracker
- `DEAD_ZONE` (default `2.0`) - degrees ignored around the centre
- `UDP_PORT` (default `5005`) - must match the port on the UDP object in Unity

## Troubleshooting

| Problem | Fix |
|---|---|
| `packets` stays at 0 in Unity | Make sure Python is running and showing values, the port is 5005 on both sides, and nothing else (for example a test receiver script) is using port 5005. Allow Unity Editor and `python.exe` through Windows Firewall. |
| W/A/S/D does nothing | Click inside the Game view. Check that the Player has **Player Move (Script)** and a Character Controller, and that Active Input Handling is set to **Both**. |
| Left and right feel reversed | Tick **Invert Yaw** on the Flashlight. |
| Up and down feel reversed | Tick **Invert Pitch**. |
| Flashlight is jittery | Lower `smoothing` in Unity or raise `SMOOTHING` in the sender. Also check frame rate with the Stats button. |
| Game looks laggy | Set Game view **Scale** to 1x, set flashlight shadows to Hard, and close other heavy apps. |
| `Cannot find module numpy` or MediaPipe import errors | Use Python 3.12 in a virtual environment. Do not use the MSYS2 Python. In VS Code, run **Python: Select Interpreter** and choose `.venv`. |
| Webcam does not open | Close other apps using the camera, or change `VideoCapture(0)` to `VideoCapture(1)` in `udp_sender.py`. |
| Red errors in the Unity Console | A compile error in any script stops all scripts. Fix the first error shown. |

## Tech stack

- Unity 6 (URP), C#
- Python 3.12, OpenCV, NumPy
- MediaPipe Face Landmarker
- UDP sockets (localhost)

## Credits

Built as a mini project. Head pose estimation uses OpenCV `solvePnP` on MediaPipe face landmarks.