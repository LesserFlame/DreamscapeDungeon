using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public int id;
    public bool unlocked;
    public bool selected;

    //public TMP_Text titleText;
    //public TMP_Text descriptionText;
    public string skillName;
    [TextArea(3, 10)]
    public string skillDescription;
    public int skillCost;

    public BattleActionData battleAction;

    public List<Skill> prerequisites;

    public Image skillIcon;
    public enum SkillType
    { 
        ACTION,
        PASSIVE
    }
    public SkillType skillType;

    public void UpdateUI()
    {
        //update associated ui
        if (selected) skillIcon.color = Color.cyan;
        else if (CheckUnlockable()) skillIcon.color = Color.green;
        else if (unlocked) skillIcon.color = Color.white;
        else skillIcon.color = Color.gray;
    }
    public bool UnlockSkill()
    {
        if (CheckUnlockable())
        {
            //unlock skill stuffs
            var player = FindAnyObjectByType<PlayerController>();
            //splayer.skills[id] = true;
            //if (skillType == SkillType.ACTION && battleAction != null) player.actions.Add(battleAction);
            if (player != null) 
            { 
                player.data.SKILLS[id] = true;
            }
            unlocked = true;
            return true;
        }
        return false;
    }
    public bool CheckUnlockable()
    {
        foreach (var skill in prerequisites)
        {
            if (!skill.unlocked)
            {
                //Debug.Log("1");
                return false;
            }
        }
        var data = SaveSystem.LoadPlayer();
        //Debug.Log(data.POINTS);
        if (data.POINTS < skillCost)
        {
            //Debug.Log("2");
            return false;
        }
        if (unlocked)
        {
            //Debug.Log("3"); 
            return false;
        }

        return true;
    }
}
