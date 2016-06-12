using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DeployableEngines
{
    class ModuleMultiStateEngine:PartModule
    {
        [KSPField(isPersistant = false)]
        public string EngineAnimationName;

        [KSPField(isPersistant = false)]
        public float WaitForAnimation = 0f;

        [KSPField(isPersistant = false)]
        public float SpeedMultiplier = 0f;

        [KSPField(isPersistant = false)]
        public int Layer = 1;

        [KSPField(isPersistant = false)]
        public string EngineIDAtTimeZero = "";

        private List<ModuleEnginesFX> engines = new List<ModuleEnginesFX>();
        private MultiModeEngine multiController;

        private string activeEngineName = "";

        private AnimationState[]  engineStates;

        public void Start()
        {
            engineStates = SetUpAnimation(EngineAnimationName, this.part);

            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
            {
                //engines = this.GetComponents<ModuleEnginesFX>().ToList();
                multiController = this.GetComponent<MultiModeEngine>();
                if (multiController != null)
                {
                    
                    foreach (AnimationState anim in engineStates)
                    {
                        anim.enabled = true;
                        if (multiController.runningPrimary)
                        {
                            anim.normalizedTime = 0f;
                        }
                        else
                        {
                            anim.normalizedTime = 1f;
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight )
            {
                if (multiController != null)
                {
                    foreach (AnimationState anim in engineStates)
                    {
                        if (anim.normalizedTime >= 1.0f)
                            anim.normalizedTime = 1.0f;
                        if (anim.normalizedTime <= -1.0f)
                            anim.normalizedTime = -1.0f;

                        anim.enabled = true;
                        if (multiController.runningPrimary)
                        {
                            anim.speed = -1f * SpeedMultiplier;
                        }
                        else
                        {
                            anim.speed = 1f * SpeedMultiplier;
                        }
                    }
                }       
            }
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (multiController != null)
                {
                    foreach (AnimationState anim in engineStates)
                    {
                        
                        anim.enabled = true;
                        if (multiController.runningPrimary)
                        {

                            anim.speed = -2f * SpeedMultiplier;
                        }
                        else
                        {
                            anim.speed = 2f * SpeedMultiplier;
                        }
                        Debug.Log(anim.speed);
                    }
                }
            }
        }

        public AnimationState[] SetUpAnimation(string animationName, Part part)  //Thanks Majiir!
        {
            var states = new List<AnimationState>();
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0f;
                animationState.enabled = true;
                animationState.layer = Layer;
                animationState.wrapMode = WrapMode.ClampForever;
                animationState.blendMode = AnimationBlendMode.Blend;
                animation.Blend(animationName);
                states.Add(animationState);

            }
            return states.ToArray();
        }
    }
}
