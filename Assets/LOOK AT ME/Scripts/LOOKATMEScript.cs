using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;

public class LOOKATMEScript : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable Button;

    private int moduleId;
    private int buttonPresses = 0;
    private static int moduleIdCounter = 1;
    private bool moduleSolved = false;
    private bool strikeChecker = false;
    private bool buttonPressed = false;
    private Vector3 LAMPos;
    private Vector3 LAMSca;
    private Quaternion LAMRot;
    private int privateID = 0;

    public LOOKATMEServiceScript LAMService;

    void Start()
    {
        moduleId = moduleIdCounter++;

        LAMPos = transform.localPosition;
        LAMRot = transform.localRotation;
        LAMSca = transform.localScale;

        transform.GetComponent<KMSelectable>().OnDefocus += LAMDeselected();
        Button.OnInteract += ButtonPressed();

        Button.transform.parent.gameObject.SetActive(false);

        LAMService = FindObjectOfType<LOOKATMEServiceScript>();

        while (!LAMService.LAMAdder(privateID))
            privateID++;

        Log("{0}", privateID);

        if (privateID == 0)
        {
            LAMService.ListenerAdder(FindObjectsOfType<KMBombModule>());
            Log("{0} - Initalized Modules", privateID);
        }
        StartCoroutine(ServiceListener());
    }

    private IEnumerator ServiceListener()
    {
        while (!moduleSolved)
        {
            if (LAMService.LAMModules.Where(x => x.ID == privateID).FirstOrDefault().IsSelected)
            {
                yield return StartCoroutine(LookAtMe(LAMService.LAMModules.Where(x => x.ID == privateID).FirstOrDefault().SelectedModule));
                LAMService.LAMModules.Where(x => x.ID == privateID).FirstOrDefault().IsSelected = false;
            }
            yield return new WaitForSeconds(.2f);
        }
    }

    private Action LAMDeselected()
    {
        return delegate
        {
            if (strikeChecker)
            {
                Log("NOOO! LOOK AT MEE!!!");
                Module.HandleStrike();
            }
            return;
        };
    }

    private KMSelectable.OnInteractHandler ButtonPressed()
    {
        return delegate
        {
            if (moduleSolved)
                return false;

            buttonPresses++;
            Log("Pressed the button {0}/{1} time(s)", buttonPresses, LAMService.Modules.Count / 2);

            if (buttonPresses == LAMService.Modules.Count / 2)
            {
                Log("Module Solved");
                Module.HandlePass();
                moduleSolved = true;
            }
            else
                StartCoroutine(ButtonMover(false));

            buttonPressed = true;
            strikeChecker = false;

            return false;
        };
    }

    private IEnumerator LookAtMe(ModulesScript.Modules module)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        Log("LOOK AT ME! I'M THE MODULE NOW! IGNORE {0}", module.ModuleName.ToUpperInvariant());
        yield return StartCoroutine(LAMMover(module, true));
        LAMService.Select(transform.GetComponent<KMSelectable>());
        StartCoroutine(ButtonMover(true));
        strikeChecker = true;
        yield return new WaitUntil(() => buttonPressed);
        buttonPressed = false;
        module.Selected = false;
        yield return StartCoroutine(LAMMover(module, false));
    }

    private IEnumerator LAMMover(ModulesScript.Modules module, bool LAM)
    {
        var elapsed = 0f;
        var duration = .25f;
        var zeroScale = new Vector3(0f, 0f, 0f);
        if (LAM)
        {
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                module.ModuleSelectable.transform.localScale = Vector3.Lerp(module.ModuleLocalScale, zeroScale, elapsed / duration);
                transform.localScale = Vector3.Lerp(LAMSca, zeroScale, elapsed / duration);
            }
            elapsed = 0f;
            transform.localPosition = module.ModuleLocalPosition;
            transform.localRotation = module.ModuleLocalRotation;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(zeroScale, LAMSca, elapsed / duration);
            }
        }
        else
        {
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(LAMSca, zeroScale, elapsed / duration);
            }
            elapsed = 0f;
            transform.localPosition = LAMPos;
            transform.localRotation = LAMRot;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(zeroScale, LAMSca, elapsed / duration);
                module.ModuleSelectable.transform.localScale = Vector3.Lerp(zeroScale, module.ModuleLocalScale, elapsed / duration);
            }
        }
    }

    private IEnumerator ButtonMover(bool up)
    {
        var elapsed = 0f;
        var duration = 1f;
        if (up)
            Button.transform.parent.gameObject.SetActive(true);
        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            Button.transform.localPosition = Vector3.Lerp(new Vector3(0f, 0f, up ? 0f : 0.0095f), new Vector3(0f, 0f, up ? 0.0095f : 0f), elapsed / duration);
        }
        if (!up)
            Button.transform.parent.gameObject.SetActive(false);
    }

    void Log(string msg, params object[] fmtArgs)
    {
        Debug.LogFormat(@"[LOOK AT ME #{0}] {1}", moduleId, string.Format(msg, fmtArgs));
    }
}