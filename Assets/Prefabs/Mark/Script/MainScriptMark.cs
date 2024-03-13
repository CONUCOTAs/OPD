using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class MainScriptMark : MonoBehaviour
{
    public SceneScript scene_script; // скрипт сцены
    public TextMeshProUGUI notice_txt; // текст подсказок метки
    private GameObject active_mark; // текущий активный объект с меткой    
    private Tweener scale_tween; // для эффекта масштабирования надписи
    private List<List<string>> action_type_txt; // текст для активной метки, тип метки определяется по индексу массива

    private void Start()
    {
        active_mark = null;
        if (notice_txt != null)
        notice_txt.enabled = false;
        action_type_txt = new List<List<string>>();
        SetNoticeTxt();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) // использовать метку
        {
            if (active_mark != null) //если метка существует, выполняем действие
            UseMark();
        }
    }


    private void SetNoticeTxt() // заполняем данные текстовых подсказок
    {

        // заполняем текстовые значения подсказок через запятую. В порядке mark_status, например, для mark_status 0 будет показываться самая первая подсказка, для mark_status 1 - следующая и т.д.
        AddNoticeTxt(new string[] { "Уменьшить", "Увеличить" }); // для 1 mark_type
        AddNoticeTxt(new string[] { "Поднять вверх 1 раз" }); // 2
        AddNoticeTxt(new string[] { "Удалить" }); // 3
        AddNoticeTxt(new string[] { "Сдвинуть вверх","Сдвинуть вниз" }); // 4



        // для пианино       
        string[] piano_str = new string[] { "Ля #", "Си", "До", "До #", "Ре", "Ре #", "Ми", "Фа", "Фа #", "Соль", "Соль #", "Ля", "Ля #", "Си", "До", "До #" };
        for (int i = 0; i < piano_str.Length; i++)
            piano_str[i] += " <size=25>сыграть</size> ";
        AddNoticeTxt(piano_str); // 5

        // Для кубиков
        AddNoticeTxt(new string[] { "середина", "верх", "низ" }); // 6
        AddNoticeTxt(new string[] { "Активировать задание" }); // 10


    }

    public void MarkReady (GameObject mark, int mark_type, int mark_status) // выполняем начальную установку для новой метки
    {
        if (mark_type == 4) // для 4 типа метки меняем начальный цвет
        {
            if (mark_status == 0)
                mark.GetComponent<Mark>().SetMarkActiveColor(1, 0); // задаем другой цвет для состояния активности (индекс 1)
            else mark.GetComponent<Mark>().SetMarkActiveColor(1, 1);
        }

        if (mark_type == 6) // меняем начальные положения кубиков
        {
            Transform cubic_tr = mark.GetComponent<Mark>().attach_obj.transform;
            if (mark_status == 0)
                cubic_tr.localScale = new Vector3(1, 1.5f, 1);
            else if (mark_status == 1)
                cubic_tr.localScale = new Vector3(1, 2f, 1);
            else if (mark_status == 2)
                cubic_tr.localScale = new Vector3(1, 2.5f, 1);
        }
    }



    //!!! Действия с кубами - только для примера работы меток. Из функции можно запускать другие скрипты, действия и т.д.

    private void UseMark() // активировать метку,  здесь размещаем код для действия над объектами, к которым присоединена метка
    {
        if (notice_txt != null)
            notice_txt.enabled = false; // сразу после действия, скрываем подсказку

        Mark mark = active_mark.GetComponent<Mark>(); // кешируем компонент метки

        // получаем данные о текущей метке
        int cur_mark_type = mark.GetMarkType(); // тип метки
        int cur_mark_status = mark.GetMarkStatus(); // состояние метки
        GameObject cur_obj = mark.GetMarkObj(); // присоединенный объект

        //тип метки
        if (cur_mark_type == 1) // метка с двумя действиями, для примера будем менять масштаб объекта к которому прикреплена эта метка
        {
            float new_scaleY = 0.5f;
            if (cur_mark_status == 1)
                new_scaleY = 1f;

            cur_obj.transform.DOScale(new Vector3(1, new_scaleY, 1), 0.5f);

            if (cur_mark_status == 0) // меняем цвет метки
            {
                mark.SetMarkActiveColor(1, 1);             
                cur_mark_status = 1;
            }
            else
            {
                mark.SetMarkActiveColor(1, 0);          
                cur_mark_status = 0;
            }

            mark.ChangeColor(1); // сохраняем новый цвет активной метки
            mark.SetMarkStatus(cur_mark_status); // ставим новый статус метки
            RefreshTxtNotice(action_type_txt[cur_mark_type - 1][cur_mark_status]); // обновляем текст подсказки
        }
        else if (cur_mark_type == 2) // для примера, сдвинем куб, и удалим метку
        {
            cur_obj.transform.DOMoveY(1.2f, 0.5f);
            mark.DisableMark(); // отключаем скрипт метки
            Destroy(mark.gameObject); // удаляем объект метки
        }
        else if (cur_mark_type == 3) // для примера, удаляем весь объект
        {
            mark.DisableMark();
            Destroy(cur_obj.transform.parent.gameObject);           
        }
        else if (cur_mark_type == 4) // метка с двумя действиями, для примера будем смещать объект по вертикали
        {
            float new_posY = 1.5f;
            if (cur_mark_status == 1)
                new_posY = 0.5f;

            cur_obj.transform.parent.transform.DOMoveY(new_posY, 1.5f);


            if (cur_mark_status == 0) // меняем цвет метки на противоположный
            {
                mark.SetMarkActiveColor(1, 1);
                cur_mark_status = 1;
            }
            else
            {
                mark.SetMarkActiveColor(1, 0);                
                cur_mark_status = 0;
            }

            mark.ChangeColor(1);
            mark.SetMarkStatus(cur_mark_status);
            RefreshTxtNotice(action_type_txt[cur_mark_type - 1][cur_mark_status]); // обновляем текст подсказки


        }
        else if (cur_mark_type == 5) // ноты пианино, здесь не будем менять статус метки, а просто сыграем нужный звук ноты
        {
            GetComponent<SceneScript>().PlayPiano(cur_mark_status, mark.transform.position + Vector3.up * 0.1f); // запустим функцию из другого скрипта
        }

        else if (cur_mark_type == 6) // кубики, меняем их высоту 
        {
            // меняем статус метки
            cur_mark_status += 1;
            if (cur_mark_status > 2)
                cur_mark_status = 0;

            float new_scaleY = 1.5f; // для mark_status = 0
            if (cur_mark_status == 1)
                new_scaleY = 2f; // для 1
            if (cur_mark_status == 2)
                new_scaleY = 2.5f; // для 2

            cur_obj.transform.DOScale(new Vector3(1, new_scaleY, 1), 0.5f);
            
            mark.SetMarkStatus(cur_mark_status); // ставим новый статус метки
            RefreshTxtNotice(action_type_txt[cur_mark_type - 1][cur_mark_status]); // обновляем текст подсказки
        }

        else if (cur_mark_type == 7) // кубики, меняем их высоту 
        {
            float new_scale = 15f; // для mark_status = 0

            cur_obj.transform.DOScale(new Vector3(new_scale, new_scale, 1), 0.5f);

            mark.DisableMark(); // отключаем скрипт метки
            Destroy(mark.gameObject); // удаляем объект метки

        }


    }
        

    public bool NewActiveMark(GameObject obj, float angle, int mark_type, int mark_status) // сделать метку активной, возвращает true - если успешно. Вызывается напрямую из скрипта метки
    {
        bool ret = false;
        bool game_pause = false;

        if (scene_script != null && !scene_script.GameWorking())
            game_pause = true;

        if (!game_pause) // если игра не на паузе. Здесь можно убрать это условие, если оно вам не понадобится !
        {
            if (active_mark != obj) // если новая метка еще не активна
            {
                if (active_mark != null) // если сейчас есть другая активная метка, проверяем
                {
                    if (angle < active_mark.GetComponent<Mark>().GetAngle()) // Если угол обзора новой метки будет меньше, то включаем теперь ее
                    {
                        active_mark.GetComponent<Mark>().RemoveActiveState(); // принудительно отключаем прошлую активную метку в скрипте этой метки                     
                        ret = true;
                    }
                }
                else ret = true;
            }

            if (ret) // метку нужно активировать
            {
                active_mark = obj; // новая метка     
                if (notice_txt != null) // есть текстовой элемент для вывода
                {
                    string type_str = action_type_txt[mark_type - 1][mark_status];
                    notice_txt.text = type_str + " - <b><color=#ffffff>E</color></b>";
                    notice_txt.enabled = true;
                    EffectScaleTxt(); // анимируем подсказку
                }
            }
        }

        return ret;
    }


    private void AddNoticeTxt (string[] notice_val) // переводим строковые данные в массив
    {      
        action_type_txt.Add(new List<string>()); 
        for (int i = 0; i < notice_val.Length; i++)
            action_type_txt[action_type_txt.Count - 1].Add(notice_val[i]);        
    }

    public void RefreshTxtNotice(string type_str) // обновить текст подсказки сразу после действия
    {
        if (notice_txt != null) 
        {            
            notice_txt.text = type_str + " - <b><color=#ffffff>E</color></b>";
            notice_txt.enabled = true;
            EffectScaleTxt();            
        }
    }

    private void EffectScaleTxt() // эффект масштабирования при появлении новой подсказки
    {
        if (scale_tween != null && scale_tween.active)
            scale_tween.Kill();

        notice_txt.GetComponent<Transform>().localScale = new Vector3(1.2f, 1.2f, 1.2f);
        scale_tween = notice_txt.GetComponent<Transform>().DOScale(1f, 1f);
    }

    
    public void RemoveActiveMark(GameObject obj)
    {
        if (active_mark == obj) // снимаем активную метку по запросу скрипта с этой меткой
        {
            active_mark = null;
            if (notice_txt != null)
                notice_txt.enabled = false;
        }
    }

    public void RemoveAllMark() // временно отключаем работу меток, для паузы игры
    {
        if (active_mark != null) // снимаем активную метку
        {
            active_mark.GetComponent<Mark>().RemoveActiveState();
            active_mark = null;
            notice_txt.enabled = false;
        }


    }


}
