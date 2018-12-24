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
            PersistentEngine.AddStatefulItem(this);

            LoadState();
        }

        protected virtual void InitialiseGlobal()
        {
            sceneName = "globalScene";
            PersistentEngine.AddStatefulItem(this);
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
                    if (PersistentEngine.GetState(GetObjectIdentifier(sceneName) + field.Name) != null)
                        field.SetValue(this, PersistentEngine.GetState(GetObjectIdentifier(sceneName) + field.Name));
                }
                else
                if (PersistentEngine.IsNumericType(field))
                {
                    if (PersistentEngine.GetState(GetObjectIdentifier(sceneName) + field.Name) != null)
                        field.SetValue(this, PersistentEngine.GetState(GetObjectIdentifier(sceneName) + field.Name));
                }
                else
                if (PersistentEngine.IsBooleanType(field))
                {
                    if (PersistentEngine.GetState(GetObjectIdentifier(sceneName) + field.Name) != null)
                    {
                        var val = PersistentEngine.GetState(GetObjectIdentifier(sceneName) + field.Name);
                        GetObjectIdentifier(sceneName);
                        field.SetValue(this, val == 1 ? true : false);
                    }
                }
                else
                if (PersistentEngine.IsStringType(field))
                {
                    var val = PersistentEngine.GetStateStr(GetObjectIdentifier(sceneName) + field.Name);
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
                    PersistentEngine.SetState(GetObjectIdentifier(sceneName) + field.Name, (int)field.GetValue(this), saveFile);
                }
                else
                if (PersistentEngine.IsNumericType(field))
                {
                    PersistentEngine.SetState(GetObjectIdentifier(sceneName) + field.Name, (int)field.GetValue(this), saveFile);
                }
                else
                if (PersistentEngine.IsBooleanType(field))
                {
                    var val = (bool)field.GetValue(this) == true ? 1 : 0;
                    GetObjectIdentifier(sceneName);
                    PersistentEngine.SetState(GetObjectIdentifier(sceneName) + field.Name, val, saveFile);
                }
                else if (PersistentEngine.IsStringType(field))
                {
                    var val = field.GetValue(this);
                    GetObjectIdentifier(sceneName);
                    try
                    {
                        PersistentEngine.SetStateStr(GetObjectIdentifier(sceneName) + field.Name, (string)val, saveFile);
                    }
                    catch
                    {
                        PersistentEngine.SetState(GetObjectIdentifier(sceneName) + field.Name, null, saveFile);
                    }
                }
            }
        }
    }
}