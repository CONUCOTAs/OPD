using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamController : MonoBehaviour
{
    public SceneScript scene_script; // скрипт сцены
    private float player_speed;
    private bool player_walk_forward;
    private bool player_walk_backward;
    private bool player_walk_left;
    private bool player_walk_right;
    private float MoveMouseX;
    private float MoveMouseY;
    private Vector3 camera_rot_input;
    private float sensitivityMouse;
    private Rigidbody player_rb;
    private Transform player_tr;
    private Transform main_cam;
    private float total_speed_forward;
    private float total_speed_side; 
    private Vector3 movement;
    private Vector2 norm_movement;



    private void Start()
    {       
        player_rb = GetComponent<Rigidbody>();
        player_tr = GetComponent<Transform>();
        main_cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        player_speed = 4f;
        sensitivityMouse = 4f;
        player_walk_forward = false;
        player_walk_backward = false;
        player_walk_left = false;
        player_walk_right = false;
        MoveMouseX = 0;
        MoveMouseY = 0;
        total_speed_forward = 0;
        total_speed_side = 0;

        player_rb.isKinematic = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void FixedUpdate()
    {
        total_speed_forward = 0;
        total_speed_side = 0;

        if (player_walk_forward && !player_walk_backward)
            total_speed_forward = player_speed;
        else if (!player_walk_forward && player_walk_backward)
            total_speed_forward = -player_speed;
        if (player_walk_left && !player_walk_right)
             total_speed_side = -player_speed;
        else if (!player_walk_left && player_walk_right)
            total_speed_side = player_speed;

        player_rb.rotation = Quaternion.Euler(0, camera_rot_input.y, 0);
        if (total_speed_forward != 0 || total_speed_side != 0) // физическое смещение
        {
            movement = new Vector3(total_speed_side, player_rb.velocity.y, total_speed_forward);
            norm_movement = new Vector2(movement.x, movement.z).normalized;
            movement.x *= Mathf.Abs(norm_movement.x);
            movement.z *= Mathf.Abs(norm_movement.y);        
            player_rb.velocity = player_tr.TransformDirection(movement);      
        }
    }


    private void Update()
    {
        if (scene_script.GameWorking()) // если игра не на паузе, управление активно
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) // вперед
                player_walk_forward = true;
            else player_walk_forward = false;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) // назад
                player_walk_backward = true;
            else player_walk_backward = false;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) // влево
                player_walk_left = true;
            else player_walk_left = false;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) // вправо
                player_walk_right = true;
            else player_walk_right = false;

            if (Input.GetMouseButtonDown(1)) // скрыть курсор
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            if (Input.GetKeyDown(KeyCode.Escape)) // показать курсор
            {
                if (!Cursor.visible)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else Application.Quit(); // 2 нажатие закрывает приложение
            }

            // поворот камеры
            MoveMouseX = Input.GetAxis("Mouse X");
            MoveMouseY = Input.GetAxis("Mouse Y");

            camera_rot_input.x += -sensitivityMouse * MoveMouseY;
            camera_rot_input.y += sensitivityMouse * MoveMouseX; // поворот  

            if (camera_rot_input.x > 65) // ограничение поворота        
                camera_rot_input.x = 65;
            else if (camera_rot_input.x < -80)
                camera_rot_input.x = -80;

            camera_rot_input.z = 0;
        }
    }

    private void LateUpdate()
    {
        main_cam.eulerAngles = camera_rot_input;
        main_cam.position = transform.position + Vector3.up * 1.5f;
    }

    public void GamePause () // ставим на паузу
    {
        player_walk_forward = false;
        player_walk_backward = false;
        player_walk_left = false;
        player_walk_right = false;   
        total_speed_forward = 0;
        total_speed_side = 0;
    }



}
