using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mark : MonoBehaviour
{
    private MainScriptMark script; // ссылка на основной скрипт метки  
    private SceneScript script_scene; // ссылка на скрипт сцены
    public int mark_type; // тип метки-объекта, для задания действия, например,  1 - увеличить/уменьшить,  2 - действие на 1 раз, 3 - удалить, 4 - поднять вверх, опустить вниз, 5 - ноты для пианино, 6 - кубик с подъемом
    public int mark_status; // состояние для меток с несколькими действиями, для пианино задается уникальный номер попорядку
    public GameObject attach_obj; // объект, к которому привязывается метка
    public float cof_scale; // множитель размера (размер метки на экране)
    public float distance_active;  // дистанция активного состояния
    public float distance_show;    // дистанция включения    
    public bool enable; // общая настройка, используется ли метка
    public float angle_active; // угол обзора камеры, чтобы метка стала активной
    public Color32 color_show;
    public Color32 color_active;
    public Color32 color_add; // цвет для дополнительных действий
    private Transform main_cam;
    private Transform tr;
    private SpriteRenderer render;      
    private bool priority_stop; // принудительное отключение активности из главного кода 
    private float distanse;
    private float sprite_size;
    private bool show;
    private float angle_to_object; // угол к метке
    private Color32[] arr_color;
    private int state; // состояние метки, 0 - выключена. 1 - интерактивная, 2 - видимая    
    private Tweener color_tween;

    
    // Если что непонятно (или нужно модернизировать работу скрипта), смело пишите автору (контакты в профиле Хабра). Отвечу по возможности.


    private void Start()
    {        
        main_cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        script = GameObject.FindGameObjectWithTag("SceneScript").GetComponent<MainScriptMark>();
        script_scene = GameObject.FindGameObjectWithTag("SceneScript").GetComponent<SceneScript>();
        tr = GetComponent<Transform>();  
        render = GetComponent<SpriteRenderer>();
        render.enabled = false;
        priority_stop = false;
        show = false;    
        arr_color = new Color32[] { new Color32(color_show.r, color_show.g, color_show.b, 0), color_active, color_show }; // 3 значения, 0 - цвет выключенного состояния, 1 - цвет активного, 2 - цвет просто включенного состояния
        render.color = arr_color[0];

        script.MarkReady(this.gameObject, mark_type, mark_status);  
    }


    private void LateUpdate() // выполняется после всех update, перед обновлением камеры
    {
        if (enable) // если скрипт включен, выполняется основная проверка работы меток
        {
            distanse = Vector3.Distance(main_cam.position, tr.position); // расстояние от камеры до метки                         

            if (distanse < distance_show) // видимая
            {
                if (!show) // включаем метку, если выключена
                {                   
                    render.enabled = true;
                    show = true;             
                }

                if (state == 1 || state == 2) // если метка уже видима
                angle_to_object = Mathf.Abs(Vector3.SignedAngle(tr.position - main_cam.position, main_cam.forward, Vector3.right));  // проверяем новый угол обзора к метке  

                //if (state != 1 && distanse <= distance_active && angle_to_object <= angle_active) // это условие использовать, если не нужен скрипт сцены   // активный если, расстояние до метки в пределах активности и есть нужный угол обзора.            
                if (state != 1 && distanse <= distance_active && angle_to_object <= angle_active && (script_scene == null || (script_scene != null && script_scene.GameWorking()) ) ) // активный если, расстояние до метки в пределах активности и есть нужный угол обзора. | И скрипт сцены показывает, что игра не на паузе              
                    ChangeState(1);
                else if ((state != 2 && distanse > distance_active) || (state == 1 && angle_to_object > angle_active) || (state == 1 && priority_stop)) //просто видимый если, расстояние в пределах видимости, | или было состояние активно, но угол обзора стал больше нужного, | или принудительное отключение активного состояния из основного скрипта
                    ChangeState(2);               
            }
            else if (state != 0 && distanse > distance_show + 1) //метка расположенная дальше дистанции видимости - скрываем . Здесь добавляем + 1 метр к расчету, чтобы не было постоянного включения / выключения при легком смещении игрока
                ChangeState(0);

            if (show) // если метка видимая, ставим положение параллельно камере, и меняем размер
            {
                tr.rotation = main_cam.rotation;
                sprite_size = cof_scale * distanse;  // размер метки в зависимости от расстояния до камеры и заданного коэффициента                
                //tr.localScale = Vector3.Slerp(tr.localScale, new Vector3(sprite_size, sprite_size, sprite_size), Time.deltaTime * 10);   
                Vector3 scale_cof = new Vector3(tr.localScale.x / tr.lossyScale.x, tr.localScale.y / tr.lossyScale.y, tr.localScale.z / tr.lossyScale.z); // для независимости от масштаба родителей                                        
                tr.localScale = Vector3.Slerp(tr.localScale, scale_cof * sprite_size, Time.deltaTime * 10);
            }
        }
    }


    
    private void ChangeState(int num) // переключение состояния метки, 0 - надо выключить. 1 - надо сделать активной для интерактива. 2 - надо просто сделать видимой
    {
        bool mark_access = true; 
        if (num == 1) // если нужно активировать эту метку, сначала проверяем возможность в основном скрипте
            mark_access = script.NewActiveMark(this.gameObject, angle_to_object, mark_type, mark_status); // если возвращает true - основной скрипт дал добро

        if (mark_access) // код выполняется всегда, кроме запрета активации метки из основного скрипта 
        {
            priority_stop = false; // снимаем принудительное отключение метки
            if (state == 1 && num != 1) // если метка сейчас активная, а нужно выключить
                script.RemoveActiveMark(this.gameObject); // уведомляем основной скрипт о снятии активности метки

            state = num; // новый текущий статус метки           
            ChangeColor(state);
        }   
    }

    public void ChangeColor (int num_state) // плавно меняем цвет метки
    {
        float anim_time = 0.5f;

        if (num_state == 1)
            anim_time = 0.25f;

        if (color_tween != null && color_tween.active) // останавливаем прошлую анимацию
            color_tween.Kill();

        color_tween = DOTween.To(() => render.color, x => render.color = x, arr_color[num_state], anim_time).SetEase(Ease.InOutExpo).OnComplete(() =>
        {            
            if (this != null && state == 0) // выключаем отобажение метки
            {            
                show = false;
                render.enabled = false;
            }
        });        
    }

    public float GetAngle() // возвращет угол между направлением камеры и текущей меткой
    {
        return angle_to_object;
    }

    public int GetMarkType() // возвращет тип метки
    {
        return mark_type;
    }

    public int GetMarkStatus() // возвращет состояние метки
    {
        return mark_status;
    }

    public void SetMarkStatus(int new_state) // устанавливает состояние метки
    {
      mark_status = new_state;
    }

    public GameObject GetMarkObj() // возвращает объект метки
    {
        return attach_obj;
    }

    public void RemoveActiveState() // выключаем активность метки из основного скрипта
    {
        priority_stop = true;     
    }

    public void EnableMark() // включить рабочее состояние
    {
        enable = true;
    }

    public void DisableMark() // прекращение работы с меткой
    {
        enable = false;
    }

    public void SetMarkActiveColor (int num_index, int new_color) // меняем цвет активной метки. В основном используется для работы с большим количеством действий у меток
    {
        Color32 col;
        if (new_color == 1)
            col = color_add; // ставим дополнительный цвет
        else col = color_active; // обычный цвет
        arr_color[num_index] = col;
    }

}
