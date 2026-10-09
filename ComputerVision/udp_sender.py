import socket

import cv2
import mediapipe as mp
import numpy as np
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

# =========================
# UDP
# =========================
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# =========================
# TUNING
# =========================
SMOOTHING = 0.35          # 0..1, higher = more responsive, lower = smoother
DEAD_ZONE = 2.0           # degrees ignored around centre (soft, no jump)
YAW_LIMIT = 70
PITCH_LIMIT = 35

# =========================
# MEDIAPIPE
# =========================
base_options = python.BaseOptions(model_asset_path="face_landmarker.task")
options = vision.FaceLandmarkerOptions(
    base_options=base_options,
    num_faces=1,
    min_face_detection_confidence=0.5,
    min_face_presence_confidence=0.5,
    min_tracking_confidence=0.5,
)
detector = vision.FaceLandmarker.create_from_options(options)

# 3D face model (same order as image_points below)
MODEL_POINTS = np.array([
    [0.0, 0.0, 0.0],           # nose
    [0.0, -330.0, -65.0],      # chin
    [-225.0, 170.0, -135.0],   # landmark 263
    [225.0, 170.0, -135.0],    # landmark 33
    [-150.0, -150.0, -125.0],  # landmark 291
    [150.0, -150.0, -125.0],   # landmark 61
], dtype=np.float64)
LANDMARK_IDS = [1, 152, 263, 33, 291, 61]


def wrap(angle):
    """Wrap an angle difference into -180..180 so it never jumps by 360."""
    return (angle + 180.0) % 360.0 - 180.0


def soft_dead_zone(value, dz):
    """Subtract the dead zone instead of snapping to 0 (keeps motion continuous)."""
    return np.sign(value) * max(abs(value) - dz, 0.0)


# =========================
# CAMERA
# =========================
cap = cv2.VideoCapture(0)
if not cap.isOpened():
    print("Could not open webcam")
    raise SystemExit(1)

print("Webcam connected. First face seen is auto-calibrated.")
print("Press C to re-calibrate (look straight ahead), Q to quit.")

center_yaw = 0.0
center_pitch = 0.0
calibrated = False
yaw = pitch = 0.0
smooth_yaw = 0.0
smooth_pitch = 0.0
face_found = False

while True:
    ret, frame = cap.read()
    if not ret:
        print("Failed to read webcam")
        break

    h, w, _ = frame.shape
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
    result = detector.detect(mp_image)

    face_found = False

    if result.face_landmarks:
        lm = result.face_landmarks[0]
        image_points = np.array(
            [[lm[i].x * w, lm[i].y * h] for i in LANDMARK_IDS], dtype=np.float64
        )

        camera_matrix = np.array(
            [[w, 0, w / 2], [0, w, h / 2], [0, 0, 1]], dtype=np.float64
        )

        success, rvec, tvec = cv2.solvePnP(
            MODEL_POINTS,
            image_points,
            camera_matrix,
            np.zeros((4, 1)),
            flags=cv2.SOLVEPNP_ITERATIVE,
        )

        if success:
            face_found = True
            rmat, _ = cv2.Rodrigues(rvec)
            angles = cv2.RQDecomp3x3(rmat)[0]
            pitch, yaw = angles[0], angles[1]

            # auto-calibrate on the very first detected face
            if not calibrated:
                center_yaw, center_pitch = yaw, pitch
                calibrated = True

            rel_yaw = wrap(yaw - center_yaw)
            rel_pitch = wrap(pitch - center_pitch)

            # smoothing (state is kept clean - the dead zone is NOT fed back in)
            smooth_yaw = (1 - SMOOTHING) * smooth_yaw + SMOOTHING * rel_yaw
            smooth_pitch = (1 - SMOOTHING) * smooth_pitch + SMOOTHING * rel_pitch

            out_yaw = soft_dead_zone(smooth_yaw, DEAD_ZONE)
            out_pitch = soft_dead_zone(smooth_pitch, DEAD_ZONE)

            out_yaw = float(np.clip(out_yaw, -YAW_LIMIT, YAW_LIMIT))
            out_pitch = float(np.clip(out_pitch, -PITCH_LIMIT, PITCH_LIMIT))

            sock.sendto(
                f"{out_yaw:.2f},{out_pitch:.2f}".encode("utf-8"),
                (UDP_IP, UDP_PORT),
            )

            cv2.putText(frame, f"Yaw: {out_yaw:.1f}", (20, 40),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)
            cv2.putText(frame, f"Pitch: {out_pitch:.1f}", (20, 75),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)

    if not face_found:
        cv2.putText(frame, "FACE LOST", (20, 40),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 0, 255), 2)

    cv2.imshow("Dark Hunt - Head Tracking", frame)

    # ONE waitKey per loop (two calls make key presses get missed)
    key = cv2.waitKey(1) & 0xFF

    if key == ord("c") and face_found:
        center_yaw, center_pitch = yaw, pitch
        smooth_yaw = 0.0
        smooth_pitch = 0.0
        calibrated = True
        print(f"Calibrated. Center yaw {center_yaw:.2f}, pitch {center_pitch:.2f}")

    if key == ord("q"):
        break

cap.release()
detector.close()
cv2.destroyAllWindows()
sock.close()
print("UDP sender stopped.")