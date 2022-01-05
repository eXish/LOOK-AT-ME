using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;

public class ModuleNameScript : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;

    int moduleId;
    static int moduleIdCounter = 1;
    private bool moduleSolved = false;

    void Start()
    {
        moduleId = moduleIdCounter++;
    }

    void Log(string msg, params object[] fmtArgs)
    {
        Debug.LogFormat(@"[Module Name #{0}] {1}", moduleId, string.Format(msg, fmtArgs));
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} [] | !{0} []";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        if (moduleSolved)
        {
            yield return "sendtochaterror The module is already solved.";
            yield break;
        }
        else if ((m = Regex.Match(command, @"^\s*()\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            yield return null;
            // Code goes here
            yield break;
        }
        else if (Regex.IsMatch(command, @"^\s*()\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            // Code goes here
            yield break;
        }
        else
        {
            yield return "sendtochaterror Invalid Command";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        Log("Module was force solved by TP", moduleId);
        //Code goes here
        yield break;
    }
}