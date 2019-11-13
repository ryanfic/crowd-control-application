using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MousePositionUtil{
    public class MousePosition
    {
        private static float DefaultPlaneDistance = 0f;
        public static Vector3 GetMouseWorldPosition() {
            Vector3 pos = Input.mousePosition;
            pos.z = 0.01f;
            return GetMouseWorldPosition(pos, Camera.main);
        }

        public static Vector3 GetMouseWorldPositionOnPlane(float dstToPlane){
            Vector3 pos = Input.mousePosition;
            pos.z += dstToPlane;
            return GetMouseWorldPosition(pos, Camera.main);
        }

        public static Vector3 GetMouseWorldPositionWithoutZ() {
            Vector3 pos = Input.mousePosition;
            pos.z = 0.01f;
            Vector3 vec = GetMouseWorldPosition(pos, Camera.main);
            vec.z = 0f;
            return vec;
        }
        public static Vector3 GetMouseWorldPosition(Camera worldCamera) {
            Vector3 pos = Input.mousePosition;
            pos.z = 0.01f;
            return GetMouseWorldPosition(pos, worldCamera);
        }
        public static Vector3 GetMouseWorldPosition(Vector3 screenPosition, Camera worldCamera) {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

    }
}

