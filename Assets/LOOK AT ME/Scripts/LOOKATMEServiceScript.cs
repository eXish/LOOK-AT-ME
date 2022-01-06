using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;
using UnityEngine.AI;

public class LOOKATMEServiceScript : MonoBehaviour
{
    private static Type selectableType = ReflectionHelper.FindType("Selectable");
    private static Type inputManagerType = ReflectionHelper.FindType("KTInputManager");

    private ModulesScript moduleScript = new ModulesScript();

    public List<ModulesScript.Modules> Modules = new List<ModulesScript.Modules>();
    public List<ModulesScript.LAMModules> LAMModules;

    void Start()
    {
        var LAMCount = FindObjectsOfType<LOOKATMEScript>().Length;
        while (moduleScript.LAMModulesOnBomb.Count < LAMCount)
        {
            Log("Loading...");
        }
        Log("Loading done.");
        LAMModules = moduleScript.LAMModulesOnBomb;
        StartCoroutine(ModuleSelector());
    }

    private IEnumerator ModuleSelector()
    {
        while (LAMModules.Any(x => !x.IsSolved))
        {
            Modules.Where(x => x.Selected).Select(x=>
            {
                if (LAMModules.Where(y => !y.IsSelected).Count() > 0)
                {
                    LAMModules.Where(y => !y.IsSelected).PickRandom().IsSelected = true;
                    LAMModules.Where(y => !y.IsSelected).PickRandom().SelectedModule = x;
                    x.Selected = false;
                    return x;
                }
                return x;
            });
            yield return new WaitForSeconds(.2f);
        }
    }

    private KMSelectable.OnInteractHandler SelectListener(ModulesScript.Modules module)
    {
        return delegate
        {
            if (module.Selected)
                return true;

            module.Selected = true;

            return true;
        };
    }

    public bool LAMAdder(int ID)
    {
        return moduleScript.InitializeLAMModules(ID);
    }

    public void ListenerAdder(KMBombModule[] modules)
    {
        Modules = moduleScript.InitializeModules(modules);
        foreach (var module in Modules)
            module.ModuleSelectable.OnInteract += SelectListener(module);
    }

    public void Select(KMSelectable kmSelectable)
    {
        var selectable = kmSelectable.GetComponent(selectableType);
        selectable.CallMethod("HandleSelect", true);
        var selectableManager = inputManagerType.GetValue<object>("Instance").GetValue<object>("SelectableManager");
        selectableManager.CallMethod("Select", selectable, true);
        selectableManager.CallMethod("HandleInteract");
        selectable.CallMethod("OnInteractEnded");
    }


    private void Log(string msg, params object[] fmtArgs)
    {
        Debug.LogFormat(@"[LOOK AT ME SERVICE] {0}", string.Format(msg, fmtArgs));
    }
}