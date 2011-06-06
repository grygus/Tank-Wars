using System;
using System.Collections.Generic;
using System.Text;

namespace TanksOnAHeightmap.GameBase.Cameras
{
    public class CameraManager
    {
        // Store active light
        int activeCameraIndex;
        BaseCamera activeCamera;

        // Sorted list containing all cameras
        SortedList<string, BaseCamera> cameras;

        #region Properties

        // Index of the active camera
        public int ActiveCameraIndex
        {
            get
            {
                return activeCameraIndex;
            }
        }

        public BaseCamera ActiveCamera
        {
            get
            {
                return activeCamera;
            }
        }

        public BaseCamera this[int index]
        {
            get
            {
                return cameras.Values[index];
            }
        }

        public BaseCamera this[string id]
        {
            get
            {
                return cameras[id];
            }
        }

        public int Count
        {
            get
            {
                return cameras.Count;
            }
        }
        #endregion

        public CameraManager()
        {
            cameras = new SortedList<string, BaseCamera>(4);
            activeCameraIndex = -1;
        }

        public void SetActiveCamera(int cameraIndex)
        {
            activeCameraIndex = cameraIndex;
            activeCamera = cameras[cameras.Keys[cameraIndex]];
        }

        public void SetActiveCamera(string id)
        {
            activeCameraIndex = cameras.IndexOfKey(id);
            activeCamera = cameras[id];
        }

        public void Clear()
        {
            cameras.Clear();
            activeCamera = null;
            activeCameraIndex = -1;
        }

        public void Add(string id, BaseCamera camera)
        {
            cameras.Add(id, camera);

            if (activeCamera == null)
            {
                activeCamera = camera;
                activeCameraIndex = cameras.IndexOfKey(id);
            }
        }

        public void Remove(string id)
        {
            cameras.Remove(id);
        }
    }
}
