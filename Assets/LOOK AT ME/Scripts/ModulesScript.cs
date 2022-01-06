using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModulesScript : MonoBehaviour
{
    public class Modules
    {
        public string ModuleName;
        public KMSelectable ModuleSelectable;
        public Vector3 ModuleLocalPosition;
        public Quaternion ModuleLocalRotation;
        public Vector3 ModuleLocalScale;
        public bool Selected = false;
        public bool IsSelected = false;
    }

    public class LAMModules
    {
        public int ID;
        public bool IsSelected;
        public Modules SelectedModule;
        public bool IsSolved;
    }

    public List<LAMModules> LAMModulesOnBomb = new List<LAMModules>();

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

    public List<Modules> InitializeModules(KMBombModule[] modulesOnBomb)
    {
        var modules = new List<Modules>();

        foreach (var item in modulesOnBomb)
        {
            if (ignoredModules.Contains(item.ModuleType) || item.GetComponent<KMSelectable>() == null)
                continue;

            modules.Add(new Modules()
            {
                ModuleName = item.ModuleDisplayName,
                ModuleSelectable = item.GetComponent<KMSelectable>(),
                ModuleLocalPosition = item.transform.localPosition,
                ModuleLocalRotation = item.transform.localRotation,
                ModuleLocalScale = item.transform.localScale
            });
        }
        return modules;
    }

    public bool InitializeLAMModules(int ID)
    {
        if (LAMModulesOnBomb.Any(x => x.ID == ID))
            return false;

        LAMModulesOnBomb.Add(new LAMModules() { ID = ID, IsSelected = false });
        return true;
    }
}
