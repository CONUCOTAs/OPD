using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

public class SceneScript : MonoBehaviour
{
    public MainScriptMark mark_script; // главный скрипт метки     
    public TextMeshProUGUI pause_txt;    
    public PostProcessVolume post_process_pause;
    public SimpleCamController control_script;   
    public TextMeshProUGUI block_FPS_text;
    private int game_status; // статус игры. 1 - в игре, 0 - пауза 
    private float fps_timer;
    private float fps_value;
    public AudioClip[] snd_piano_note; // ноты для пианино
    public GameObject prefab_note_particle; // префаб частиц


    void Start()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly); // инициализация Tween
        game_status = 1;        
        pause_txt.enabled = false;
        fps_timer = 0;
        fps_value = 0;
    }
        
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // пауза
            GamePause();

        ShowFps();
    }


    private void ShowFps() // счетчик ФПС
    {
        fps_value += 1;
        fps_timer += Time.unscaledDeltaTime;
        if (fps_timer >= 1)
        {
            float val = Mathf.Round(fps_value / fps_timer);
            block_FPS_text.text = "ФПС: <color=#FFC600>" + val + "</color>";
            fps_timer -= 1;
            fps_value = 0;
        }
    }
    

    private void GamePause()
    {
        if (game_status == 1) // включаем паузу
        {
            game_status = 0;
            mark_script.RemoveAllMark();         
            pause_txt.enabled = true;
            post_process_pause.weight = 1;
            control_script.GamePause();

        }
        else // возврат в игру
        {
            game_status = 1;
            pause_txt.enabled = false;
            post_process_pause.weight = 0;            
        }


    }   

    public void PlayPiano(int num_note, Vector3 pos)
    {
        GetComponent<AudioSource>().PlayOneShot(snd_piano_note[num_note]);

        Instantiate(prefab_note_particle, pos, prefab_note_particle.transform.rotation);
    }
        
    public bool GameWorking() // дополнительная проверка, что игра дает возможность сейчас активировать метку
    {
        bool ret = true;

        if (game_status != 1) // игра на паузе, метку включать не будем
            ret = false;

        return ret;
    }
    
}
