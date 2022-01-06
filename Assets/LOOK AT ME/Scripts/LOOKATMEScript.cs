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
    private string curSN;
    private Selector selector = new Selector();

    private static readonly string[] ignoredModules = new string[]
    {
        "GSEight",
        "idExchange",
        "forgetMorseNot",
        "soulsong",
        "forgetOurVoices",
        "TwisterModule",
        "ConcentrationModule",
        "duckKonundrum",
        "BoardWalk",
        "tetrahedron",
        "qkCubeSynchronization",
        "soulscream",
        "plus",
        "forgetMazeNot",
        "blackArrowsModule",
        "FloorLights",
        "ShoddyChessModule",
        "SecurityCouncil",
        "KeypadDirectionality",
        "ForgetAnyColor",
        "whiteout",
        "busyBeaver",
        "ANDmodule",
        "kugelblitz",
        "omegaForget",
        "iconic",
        "TheTwinModule",
        "RPSJudging",
        "forgetInfinity",
        "ForgetTheColors",
        "brainf",
        "simonForgets",
        "14",
        "forgetItNot",
        "ubermodule",
        "forgetMeLater",
        "qkForgetPerspective",
        "organizationModule",
        "forgetUsNot",
        "forgetEnigma",
        "tallorderedKeys",
        "forgetThemAll",
        "PurgatoryModule",
        "forgetThis",
        "simonsStages",
        "HexiEvilFMN",
        "SouvenirModule",
        "MemoryV2",
        "LOOKATME"
    };

    private class Modules
    {
        public string ModuleName;
        public KMSelectable ModuleSelectable;
        public Vector3 ModuleLocalPosition;
        public Quaternion ModuleLocalRotation;
        public Vector3 ModuleLocalScale;
        public bool Selected;
        public bool IsSelected;
    }

    private List<Modules> modules = new List<Modules>();
    private bool _responsibleForSolves;

    private static readonly Dictionary<string, List<Modules>> infos = new Dictionary<string, List<Modules>>();

    void Start()
    {
        moduleId = moduleIdCounter++;

        LAMPos = transform.localPosition;
        LAMRot = transform.localRotation;
        LAMSca = transform.localScale;

        transform.GetComponent<KMSelectable>().OnDefocus += LAMDeselected();
        Button.OnInteract += ButtonPressed();

        Button.transform.parent.gameObject.SetActive(false);

        curSN = Bomb.GetSerialNumber();

        if (!infos.ContainsKey(curSN))
        {
            _responsibleForSolves = true;
            infos[curSN] = new List<Modules>();
            foreach (var module in FindObjectsOfType<KMBombModule>())
            {
                if (ignoredModules.Contains(module.ModuleType) || module.GetComponent<KMSelectable>() == null)
                    continue;
                infos[curSN].Add(new Modules()
                {
                    ModuleName = module.ModuleDisplayName,
                    ModuleSelectable = module.GetComponent<KMSelectable>(),
                    ModuleLocalPosition = module.transform.localPosition,
                    ModuleLocalRotation = module.transform.localRotation,
                    ModuleLocalScale = module.transform.localScale,
                    Selected = false,
                    IsSelected = false
                });
            }
            foreach (var m in infos[curSN])
                m.ModuleSelectable.GetComponent<KMSelectable>().OnInteract += SelectListener(m);
        }
        StartCoroutine(ModuleChecker());
    }

    private KMSelectable.OnInteractHandler SelectListener(Modules module)
    {
        return delegate
        {
            module.Selected = true;
            return true;
        };
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
            Log("Pressed the button {0}/{1} time(s)", buttonPresses, infos[Bomb.GetSerialNumber()].Count / 2);

            if (buttonPresses == infos[Bomb.GetSerialNumber()].Count / 2)
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

    private IEnumerator ModuleChecker()
    {
        while (!moduleSolved)
        {
            foreach (var module in infos[curSN])
                if (module.Selected && !module.IsSelected)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        module.IsSelected = true;
                        yield return StartCoroutine(LookAtMe(module));
                    }
                    else
                        module.Selected = false;
                }

            if (_responsibleForSolves)
                infos[curSN].RemoveAll(x => x.ModuleSelectable.GetComponent(ReflectionHelper.FindType("BombComponent")).GetValue<bool>("IsSolved"));
            yield return new WaitForSeconds(.2f);
        }
    }

    private IEnumerator LookAtMe(Modules module)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        Log("LOOK AT ME! I'M THE MODULE NOW! IGNORE {0}", module.ModuleName.ToUpperInvariant());
        yield return StartCoroutine(LAMMover(module, true));
        selector.Select(transform.GetComponent<KMSelectable>());
        StartCoroutine(ButtonMover(true));
        strikeChecker = true;
        yield return new WaitUntil(() => buttonPressed);
        buttonPressed = false;
        module.Selected = false;
        yield return StartCoroutine(LAMMover(module, false));
        module.IsSelected = false;
    }

    private IEnumerator LAMMover(Modules module, bool LAM)
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

    private class Selector
    {
        private static Type selectableType = ReflectionHelper.FindType("Selectable");
        private static Type inputManagerType = ReflectionHelper.FindType("KTInputManager");

        public void Select(KMSelectable kmSelectable)
        {
            var selectable = kmSelectable.GetComponent(selectableType);
            selectable.CallMethod("HandleSelect", true);
            var selectableManager = inputManagerType.GetValue<object>("Instance").GetValue<object>("SelectableManager");
            selectableManager.CallMethod("Select", selectable, true);
            selectableManager.CallMethod("HandleInteract");
            selectable.CallMethod("OnInteractEnded");
        }
    }
}