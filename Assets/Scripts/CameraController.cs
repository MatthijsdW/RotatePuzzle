using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public float clickDelay;
    public float moveSpeed;
    public float dragSensitivity;

    private float  minX, minZ, maxX, maxZ;
    private new Camera camera;
    private Vector3 cameraPositionOnMouseDown;
    private Vector3 cameraBasePosition;

    private void Start()
    {
        camera = GetComponent<Camera>();
        cameraBasePosition = camera.transform.position;
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float xPosition = Mathf.Clamp(transform.position.x + x * moveSpeed * Time.deltaTime, minX, maxX);
        float zPosition = Mathf.Clamp(transform.position.z + z * moveSpeed * Time.deltaTime, minZ, maxZ);

        transform.position = new Vector3(xPosition, transform.position.y, zPosition);


        if (GameManager.hasWon || EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            cameraPositionOnMouseDown = transform.position;
        }

        if (Input.GetMouseButton(0))
        {
            x = -Input.GetAxis("Mouse X") * dragSensitivity;
            z = -Input.GetAxis("Mouse Y") * dragSensitivity;

            xPosition = Mathf.Clamp(transform.position.x + x * moveSpeed * Time.deltaTime, minX, maxX);
            zPosition = Mathf.Clamp(transform.position.z + z * moveSpeed * Time.deltaTime, minZ, maxZ);

            transform.position = new Vector3(xPosition, transform.position.y, zPosition);
        }

        if (Input.GetMouseButtonUp(0) && (transform.position - cameraPositionOnMouseDown).magnitude < 0.1)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                hit.collider.GetComponentInParent<Tile>()?.LeftClick();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                hit.collider.GetComponentInParent<Tile>()?.RightClick();
            }
        }
    }

    public void InitCamera(int tileX, int tileY)
    {
        camera.transform.position = cameraBasePosition + Vector3.right * ((tileX - 1) / 2f) + Vector3.forward * ((tileY - 1) / 2f);

        minX = cameraBasePosition.x;
        minZ = cameraBasePosition.z;
        maxX = cameraBasePosition.x + tileX - 1;
        maxZ = cameraBasePosition.z + tileY - 1;
    }
}
