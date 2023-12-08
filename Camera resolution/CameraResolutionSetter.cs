using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    private Camera _camera;
    private int defaultAspectRatio;
    private int previousAspectRatio;

    private void Start()
    {
        _camera = Camera.main;
        defaultAspectRatio = 2400 / 1080;
        UpdateCameraSize();
    }

    private void Update()
    {
        int currentAspectRatio = Screen.width / Screen.height;

        if (!currentAspectRatio.Equals(previousAspectRatio))
        {
            previousAspectRatio = currentAspectRatio;
            UpdateCameraSize();
        }
    }

    private void UpdateCameraSize()
    {
        int currentAspectRatio = Screen.width / Screen.height;

        if (_camera.orthographic)
        {
            if (defaultAspectRatio.Equals(currentAspectRatio))
            {
                _camera.orthographicSize = 8;
            }
            else
            {
                int max = Mathf.Max(defaultAspectRatio, currentAspectRatio);
                int min = Mathf.Min(defaultAspectRatio, currentAspectRatio);

                _camera.orthographicSize = 14 - (max - min);
            }
        }
        else
        {
            if (defaultAspectRatio.Equals(currentAspectRatio))
            {
                _camera.fieldOfView = 20;
            }
            else
            {
                int max = Mathf.Max(defaultAspectRatio, currentAspectRatio);
                int min = Mathf.Min(defaultAspectRatio, currentAspectRatio);

                _camera.fieldOfView = 25 - (max - min);
            }
        }
    }
}
