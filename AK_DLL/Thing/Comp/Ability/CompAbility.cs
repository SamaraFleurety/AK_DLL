﻿using System;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace AK_DLL
{
    public class CompAbility : CompReloadable
    {
        public new CompProperties_Ability Props =>(CompProperties_Ability)this.props;
        public OperatorAbilityDef AbilityDef => this.Props.abilityDef;
        public int MaxSummon => Props.maxSummoned;
        public void Summon()
        {
            this.summoned++;
        }

        public void SummonedDead()
        {
            this.summoned--;
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            int maxCharge_var = this.AbilityDef.maxCharge == 0 ? 1 : AbilityDef.maxCharge;
            int CD_var = 0;
            CDandCharge CDandCharge = new CDandCharge(CD_var, maxCharge_var, AbilityDef.CD);
            this.CDandChargesList.Add(CDandCharge);
            
            CDandCharge num0 = new CDandCharge(1,1,1);
            this.CDandChargesList.Append(num0);
        }

        public override void CompTick()
        {
            base.CompTick();
            List<CDandCharge> CDandCharge_var = new List<CDandCharge>();
            foreach (CDandCharge CDandCharge in this.CDandChargesList) 
            {
                if (CDandCharge.charge != CDandCharge.maxCharge)
                {
                    int CD_var = (CDandCharge.CD <= 0) ? 0 : CDandCharge.CD - 1;
                    int charge_var = CDandCharge.charge;
                    if (CD_var == 0)
                    {
                        charge_var = CDandCharge.charge + 1;
                    }
                    CDandCharge CDandCharge_Loop = new CDandCharge(CD_var, CDandCharge.maxCharge, CDandCharge.maxCD);
                    CDandCharge_Loop.charge = charge_var;
                    CDandCharge_var.Add(CDandCharge_Loop);
                }
                else 
                {
                    int CD_var = 0;
                    CDandCharge CDandCharge_Loop = new CDandCharge(CD_var, CDandCharge.maxCharge,CDandCharge.maxCD);
                    CDandCharge_var.Add(CDandCharge_Loop);
                }
            }
            this.CDandChargesList.Clear();
            this.CDandChargesList.AddRange(CDandCharge_var);
        }
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            List<Gizmo> commandList = new List<Gizmo>();
            int i = 0;
                Command_Abilities ability_Command = new Command_Abilities();
                if (AbilityDef.icon!= null) 
                {
                    ability_Command.icon = ContentFinder<Texture2D>.Get(AbilityDef.icon);
                }
                ability_Command.defaultLabel = AbilityDef.label;
                ability_Command.defaultDesc = AbilityDef.description;
                ability_Command.verb = this.GetVerb(AbilityDef.verb,i,true);
                ability_Command.iconAngle = 0f;
                ability_Command.iconOffset = new Vector2(0, 0);
                ability_Command.needTarget = AbilityDef.needTarget;
                ability_Command.pawn = ((Apparel)parent).Wearer;
                ability_Command.charge = this.CDandChargesList[i].charge;
                ability_Command.maxCharge = this.CDandChargesList[i].maxCharge;
                if (AbilityDef.abilityType == AbilityType.Summon)
                {
                    if (this.summoned == this.MaxSummon)
                    {
                        ability_Command.Disable("AK_SummonedReachedMax".Translate(this.MaxSummon.ToString()));
                    }
                    Command_Abilities reclaim = ability_Command;
                    Verb_Reclaim verb_Reclaim = this.GetVerb(AbilityDef.verb_Reclaim,i,false) as Verb_Reclaim;
                    verb_Reclaim.pawn = AbilityDef.canReclaimPawn;
                    reclaim.verb = verb_Reclaim;
                    reclaim.icon = ContentFinder<Texture2D>.Get(AbilityDef.iconReclaim);
                    reclaim.defaultLabel = AbilityDef.reclaimLabel;
                    reclaim.defaultDesc = AbilityDef.reclaimDesc;
                    reclaim.needTarget = true;
                    commandList.Add(reclaim);
                    i += 1;
                }
                else
                {
                    if (AbilityDef.needCD)
                    {
                        ability_Command.CD = this.CDandChargesList[i].CD;
                    }
                    ability_Command.maxCD = AbilityDef.CD;
                    if (this.CDandChargesList[i].charge == 0)
                    {
                        ability_Command.Disable("AK_ChargeIsZero".Translate());
                    }
                    i+= 1;
                }
                commandList.Add(ability_Command);
            return commandList;
        }

        public Verb GetVerb(VerbProperties verbProp,int num,bool isntReclaim) 
        {
            if (isntReclaim)
            {
                    Verb_Ability verb_var = (Verb_Ability)Activator.CreateInstance(verbProp.verbClass);
                    verb_var.caster = ((Apparel)parent).Wearer;
                    verb_var.verbProps = verbProp;
                    verb_var.verbTracker = new VerbTracker(this);
                    verb_var.ability = this.AbilityDef;
                    verb_var.i = num;
                    return verb_var;
            }
            else 
            {
                Verb verb = (Verb)Activator.CreateInstance(verbProp.verbClass);
                verb.caster = ((Apparel)parent).Wearer;
                verb.verbProps = verbProp;
                verb.verbTracker = new VerbTracker(this);
                return verb;
            }

        
        }

        public override string CompInspectStringExtra()
        {
            return null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look<CDandCharge>(ref CDandChargesList, "CDandChargesList", LookMode.Deep);
            Scribe_Values.Look(ref summoned, "summoned");
        }

        public List<CDandCharge> CDandChargesList = new List<CDandCharge>();
        public int summoned = 0;
    }
}