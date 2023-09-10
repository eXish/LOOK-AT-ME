using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private bool onlyOneStrikeGoshDangit = true;
    private bool responsibleForSolves;
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
            responsibleForSolves = true;
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
                m.ModuleSelectable.GetComponent<KMSelectable>().OnFocus += SelectListener(m);
        }
        StartCoroutine(ModuleChecker());
    }

    private Action SelectListener(Modules module)
    {
        return delegate
        {
            module.Selected = true;
            return;
        };
    }

    private Action LAMDeselected()
    {
        return delegate
        {
            if (strikeChecker)
            {
                if (onlyOneStrikeGoshDangit)
                {
                    Log("NOOO! LOOK AT MEE!!!");
                    Module.HandleStrike();
                }
                onlyOneStrikeGoshDangit = !onlyOneStrikeGoshDangit;
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

            if (infos[curSN].Count == 0)
            {
                Log("Module Solved");
                Module.HandlePass();
                moduleSolved = true;
                return false;
            }

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
            if (infos[curSN].Count == 0)
            {
                Log("YEES! LOOK ONLY AT MEE!!!");
                StartCoroutine(ButtonMover(true));
                break;
            }
            var random = UnityEngine.Random.Range(0, 2);
            foreach (var module in infos[curSN])
                if (module.Selected && !module.IsSelected)
                {
                    if (random == 0)
                    {
                        module.IsSelected = true;
                        yield return StartCoroutine(LookAtMe(module));
                    }
                    else
                        module.Selected = false;
                }

            if (responsibleForSolves)
                infos[curSN].RemoveAll(x => x.ModuleSelectable.GetComponent(ReflectionHelper.FindType("BombComponent")).GetValue<bool>("IsSolved"));
            yield return new WaitForSeconds(.2f);
        }

        while (responsibleForSolves)
        {
            infos[curSN].RemoveAll(x => x.ModuleSelectable.GetComponent(ReflectionHelper.FindType("BombComponent")).GetValue<bool>("IsSolved"));
            yield return new WaitForSeconds(.2f);
        }
    }

    private IEnumerator LookAtMe(Modules module)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        Log("LOOK AT ME! I'M THE MODULE NOW! IGNORE {0}", module.ModuleName.ToUpperInvariant());
        yield return StartCoroutine(LAMMover(module, true));
        selector.Select(transform.GetComponent<KMSelectable>(), module.ModuleSelectable);
        StartCoroutine(ButtonMover(true));
        strikeChecker = true;
        buttonPressed = false;
        yield return new WaitUntil(() => buttonPressed);
        yield return StartCoroutine(LAMMover(module, false));
        selector.Select(module.ModuleSelectable, transform.GetComponent<KMSelectable>());
        module.Selected = false;
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

        private object origFace = null;
        private int origIndex = -1;

        #pragma warning disable CS0252
        public void Select(KMSelectable thisSelectable, KMSelectable newSelectable)
        {
            var selectable = thisSelectable.GetComponent(selectableType);
            var selectable2 = newSelectable.GetComponent(selectableType);
            var selectableManager = inputManagerType.GetValue<object>("Instance").GetValue<object>("SelectableManager");
            if (origFace != null)
            {
                var newFace = selectable2.GetValue<object>("Parent");
                var childSels = origFace.GetValue<object[]>("Children");
                childSels[origIndex] = selectable2;
                origIndex = -1;
                origFace.SetValue("Children", childSels);
                var childSels2 = newFace.GetValue<object[]>("Children");
                for (int i = 0; i < childSels2.Length; i++)
                {
                    if (childSels2[i] == selectable2)
                    {
                        childSels2[i] = selectable;
                        newFace.SetValue("Children", childSels2);
                        break;
                    }
                }
                selectable2.SetValue("Parent", origFace);
                origFace = null;
            }
            else
            {
                origFace = selectable.GetValue<object>("Parent");
                var newFace = selectable2.GetValue<object>("Parent");
                var childSels = origFace.GetValue<object[]>("Children");
                for (int i = 0; i < childSels.Length; i++)
                {
                    if (childSels[i] == selectable)
                    {
                        childSels[i] = null;
                        origFace.SetValue("Children", childSels);
                        origIndex = i;
                        break;
                    }
                }
                var childSels2 = newFace.GetValue<object[]>("Children");
                for (int i = 0; i < childSels2.Length; i++)
                {
                    if (childSels2[i] == selectable2)
                    {
                        childSels2[i] = selectable;
                        newFace.SetValue("Children", childSels2);
                        break;
                    }
                }
                selectable.SetValue("Parent", selectable2.GetValue<object>("Parent"));
            }
            selectable.CallMethod("HandleSelect", true);
            selectableManager.CallMethod("Select", selectable, true);
            selectableManager.CallMethod("HandleInteract");
            selectable.CallMethod("OnInteractEnded");
        }
        #pragma warning restore CS0252
    }
}