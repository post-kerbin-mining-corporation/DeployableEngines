using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace DeployableEngines
{

  public class ModuleAdvancedLookAtConstraint : PartModule
  {

    [System.Serializable]
    public class LookConstraint
    {
      string rotatorsName;
      string targetName;

      // Cached components
      Transform target;
      Transform rotator;
      bool constrained = false;
      Vector3 constraintScale;
      Part part;

      public LookConstraint(ConfigNode node, Part p)
      {
        node.TryGetValue("rotatorsName", ref rotatorsName);
        node.TryGetValue("targetName", ref targetName);
        node.TryGetValue("axisConstrained", ref constrained);
        node.TryGetValue("axisScales", ref constraintScale);

        part = p;
        rotator = p.FindModelTransform(rotatorsName);
        target = p.FindModelTransform(targetName);
      }

      public void UpdateRotators()
      {
        if (rotator != null && target != null)
        {
          if (!constrained)
          {
            Vector3 targetPostition = new Vector3(target.position.x,
                                               target.position.y,
                                               target.position.z);

            Vector3 lookPos = target.position - rotator.position;
            var rotation = Quaternion.LookRotation(lookPos, target.up);
            rotator.rotation = rotation;
          }
          if (constrained)
          {
            Vector3 targetPostition = rotator.parent.InverseTransformPoint(target.position);

            Vector3 rotation = Quaternion.LookRotation(targetPostition - rotator.localPosition).eulerAngles;
            rotator.localRotation = Quaternion.Euler(
              constraintScale.x * rotator.localRotation.eulerAngles.x + (1 - constraintScale.x) * rotation.x,
              constraintScale.y * rotator.localRotation.eulerAngles.y + (1 - constraintScale.y) * rotation.y,
              constraintScale.z * rotator.localRotation.eulerAngles.z + (1 - constraintScale.z) * rotation.z);
          }
        }
      }
    }


    public List<LookConstraint> constraints;

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      constraints = new List<LookConstraint>();
      ConfigNode[] cnodes = node.GetNodes("CONSTRAINLOOKFX");
      Debug.Log(String.Format("[ModuleAdvancedLookAtConstraint]: Loading {0} constraints", cnodes.Length));

      for (int i = 0; i < cnodes.Length; i++)
      {
        constraints.Add(new LookConstraint(cnodes[i], this.part));
      }

      Debug.Log(String.Format("[ModuleAdvancedLookAtConstraint]: Loaded {0} constraints", constraints.Count));
    }
    public override void OnStart(StartState state)
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (constraints == null || constraints.Count == 0)
        {
          ConfigNode cfg;
          foreach (UrlDir.UrlConfig pNode in GameDatabase.Instance.GetConfigs("PART"))
          {
            if (pNode.name.Replace("_", ".") == part.partInfo.name)
            {
              cfg = pNode.config;
              ConfigNode node = cfg.GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
              OnLoad(node);
            }
          }
        }
      }
    }

    void LateUpdate()
    {
      if (constraints != null)
      {
        for (int i = 0; i < constraints.Count; i++)
        {
          constraints[i].UpdateRotators();
        }
      }
    }

  }
}
