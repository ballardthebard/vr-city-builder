using UnityEngine;

public class BillboardAlwaysFaceCamera : MonoBehaviour
{
    // Reference to the main camera
    private Camera mainCamera;

    void Start()
    {
        // Find the main camera if not assigned
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Make the object face the camera
        FaceCamera();
    }

    void FaceCamera()
    {
        // Calculate the direction from the object to the camera
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;

        // Remove any vertical tilting by nullifying the Y-axis component of the direction vector
        directionToCamera.y = 0;

        // Rotate the object to face the camera using LookRotation
        transform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}
