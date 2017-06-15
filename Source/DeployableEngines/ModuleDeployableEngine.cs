using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

// The concepts behind this code are credited to BahamutoD's AnimatedEngine module
// can be found https://github.com/BahamutoD/BDAnimationModules/blob/master/BDAnimationModules/AnimatedEngine.cs

namespace DeployableEngines
{
    public class ModuleDeployableEngine : PartModule
	{
		[KSPField(isPersistant = false)]
		public string EngineAnimationName;
		
		[KSPField(isPersistant = false)]
		public float WaitForAnimation = 0f;

        [KSPField(isPersistant = false)]
        public bool OneAnimationForAll = false;

        [KSPField(isPersistant = false)]
        public int Layer = 1;

		private bool engineIsOn = false;
        private bool hasMultiEngine = false;

		private List<ModuleEnginesFX> engines = new List<ModuleEnginesFX>();
        private MultiModeEngine multiController;
        private string activeEngineName = "";
		private AnimationState[]  engineStates;

		[KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Toggle Animation")]
		public void ToggleAnimationEditor()
		{
			engineIsOn = !engineIsOn;
		}
		

		public void Start()
		{
			Events["ToggleAnimationEditor"].guiName = Localizer.Format("#autoLOC_502068");
			engineStates = SetUpAnimation(EngineAnimationName, this.part);
		
			if(HighLogic.LoadedSceneIsFlight)
			{
                engines = this.GetComponents<ModuleEnginesFX>().ToList();
                engineIsOn = QueryEngineOn();

                multiController = this.GetComponent<MultiModeEngine>();
                if (multiController != null)
                    hasMultiEngine = true;
			}

            
			foreach(AnimationState anim in engineStates)
			{
				if (engineIsOn)
				{
						anim.normalizedTime = 1f;
				}
				else
				{
						anim.normalizedTime = 0f;
				}
			}
			
		}
		
		
		
		public void FixedUpdate()
		{
			if(HighLogic.LoadedSceneIsFlight)
			{
                engineIsOn = QueryEngineOn();
                if (hasMultiEngine)
                    activeEngineName = multiController.mode;
			}

			foreach(var anim in engineStates)
			{
                if (engineIsOn && anim.normalizedTime < WaitForAnimation)
				{
					anim.speed = 1;
					if(HighLogic.LoadedSceneIsFlight)
					{
                        if (hasMultiEngine)
                        {
                            foreach (ModuleEnginesFX fx in engines)
                            {
                                if (fx.engineID == activeEngineName)
                                    fx.Shutdown();
                            }
                        }
                        else
                        {
                            foreach (ModuleEnginesFX fx in engines)
                            {
                                fx.Shutdown();
                            }
                        }
					}
				}
				
				
				if(HighLogic.LoadedSceneIsFlight &&  anim.normalizedTime >= WaitForAnimation && anim.speed > 0)
				{
                    if (hasMultiEngine)
                    {
                        foreach (ModuleEnginesFX fx in engines)
                        {
                            if (fx.engineID == activeEngineName)
                                fx.Activate();
                        }
                    }
                    else
                    {
                        foreach (ModuleEnginesFX fx in engines)
                        {
                            fx.Activate();
                        }
                    }
				}
				
				if(anim.normalizedTime>=1)
				{
					anim.speed = 0;
					anim.normalizedTime = 1;
				}
				
				if(anim.normalizedTime >=1 && !engineIsOn)
				{
					anim.speed = -1;
					
				}
				
				if(anim.normalizedTime <0)
				{
					anim.speed = 0;
					anim.normalizedTime = 0;
				}
				
			}
			
		}

        private bool QueryEngineOn()
        {
            foreach (ModuleEnginesFX e in engines)
            {
                if (e.EngineIgnited)
                    return true;
            }
            return false;
        }
        private bool QueryEngineOn(ModuleEnginesFX fxEng)
        {
            return false;
        }
		
		
		
		public AnimationState[] SetUpAnimation(string animationName, Part part)  //Thanks Majiir!
        {
            var states = new List<AnimationState>();
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0;
                animationState.enabled = true;
                animationState.layer = Layer;
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
				
            }
            return states.ToArray();
        }
	}
}
