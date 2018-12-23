using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Reflection;

namespace Motiviti.Enkidu
{
    public class StatefulItem : MonoBehaviour
    {
        public bool allowSaving = true;

        const string levelObjectDelimiter = "$";

        protected string sceneName = null;

        protected virtual void Initialise()
        {
            Global.AddStatefulItem(this);

            LoadState();
        }

        protected virtual void InitialiseGlobal()
        {
            sceneName = "globalScene";
            Global.AddStatefulItem(this);
            LoadState();
        }

        protected string GetObjectIdentifier(string sceneName = null)
        {
            if (sceneName == null) sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return sceneName + levelObjectDelimiter + gameObject.name + "[" + this.GetType().Name + "] ";
        }

        public void LoadState()
        {
            if (!allowSaving) return;

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(field => field.IsDefined(typeof(SaveStateAttribute), true));

            foreach (var field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    if (Global.GetState(GetObjectIdentifier(sceneName) + field.Name) != null)
                        field.SetValue(this, Global.GetState(GetObjectIdentifier(sceneName) + field.Name));
                }
                else
                if (Global.IsNumericType(field))
                {
                    if (Global.GetState(GetObjectIdentifier(sceneName) + field.Name) != null)
                        field.SetValue(this, Global.GetState(GetObjectIdentifier(sceneName) + field.Name));
                }
                else
                if (Global.IsBooleanType(field))
                {
                    if (Global.GetState(GetObjectIdentifier(sceneName) + field.Name) != null)
                    {
                        var val = Global.GetState(GetObjectIdentifier(sceneName) + field.Name);
                        GetObjectIdentifier(sceneName);
                        field.SetValue(this, val == 1 ? true : false);
                    }
                }
                else
                if (Global.IsStringType(field))
                {
                    var val = Global.GetStateStr(GetObjectIdentifier(sceneName) + field.Name);
                    GetObjectIdentifier(sceneName);
                    field.SetValue(this, val);
                }
            }
        }

        public void SaveState(bool saveFile = false)
        {
            if (!allowSaving) return;

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(field => field.IsDefined(typeof(SaveStateAttribute), true));

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    Global.SetState(GetObjectIdentifier(sceneName) + field.Name, (int)field.GetValue(this), saveFile);
                }
                else
                if (Global.IsNumericType(field))
                {
                    Global.SetState(GetObjectIdentifier(sceneName) + field.Name, (int)field.GetValue(this), saveFile);
                }
                else
                if (Global.IsBooleanType(field))
                {
                    var val = (bool)field.GetValue(this) == true ? 1 : 0;
                    GetObjectIdentifier(sceneName);
                    Global.SetState(GetObjectIdentifier(sceneName) + field.Name, val, saveFile);
                }
                else if (Global.IsStringType(field))
                {
                    var val = field.GetValue(this);
                    GetObjectIdentifier(sceneName);
                    try
                    {
                        Global.SetStateStr(GetObjectIdentifier(sceneName) + field.Name, (string)val, saveFile);
                    }
                    catch
                    {
                        Global.SetState(GetObjectIdentifier(sceneName) + field.Name, null, saveFile);
                    }
                }
            }
        }
    }
}