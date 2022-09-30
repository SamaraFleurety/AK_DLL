﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AK_DLL
{
    public static class AK_Tool
	{
		static bool doneAutoFill = false;
		static float lastVoiceTime = 0;
		public static string[] operatorTypeStiring = new string[] { "Caster", "Defender", "Guard", "Vanguard", "Specialist", "Supporter", "Medic", "Sniper" };
		public static string[] romanNumber = new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" , "XI"};
		//如果增加了要自动绑定的服装种类，只需要往这个字符串数组增加。
		public static string[] apparelType = new string[] { "Hat" };
		public static float lastVoiceLength = 0;
		public static SoundDef[] abilitySFX = new SoundDef[4] {DefDatabase<SoundDef>.GetNamed("AK_SFX_Atkboost"), DefDatabase<SoundDef>.GetNamed("AK_SFX_Defboost"), DefDatabase<SoundDef>.GetNamed("AK_SFX_Healboost"), DefDatabase<SoundDef>.GetNamed("AK_SFX_Tactboost") };

		public static Dictionary<string, OperatorDef>[] operatorDefs = new Dictionary<string, OperatorDef>[(int)OperatorType.Count];


		public static void autoFillOperators()
        {
			Log.Message("MIS.开始初始化");
			if (doneAutoFill) return;
			for (int i = 0; i <= 7; ++i)
			{
				operatorDefs[i] = new Dictionary<string, OperatorDef>();
			}
			foreach (OperatorDef i in DefDatabase<OperatorDef>.AllDefs)
			{
				i.AutoFill();
				try
				{
					operatorDefs[(int)i.operatorType].Add(i.nickname, i);
				}
				catch (Exception)
				{
					Log.Message("没加起" + i.nickname);
				}
			}
			doneAutoFill = true;
			Log.Message("MIS.初始化完成");
		}
		public static List<IntVec3> GetSector(OperatorAbilityDef ability,Pawn caster)
        {
			List<IntVec3> result = new List<IntVec3>();
			foreach (IntVec3 intVec3 in GenRadial.RadialCellsAround(caster.Position, ability.sectorRadius, false))
			{
				float mouseAngle = caster.Position.ToIntVec2.ToVector2().AngleTo(Event.current.mousePosition);
				float curAngle = caster.Position.ToVector3().AngleToFlat(intVec3.ToVector3());
				if (intVec3.InBounds(caster.Map) && curAngle > ability.minAngle + mouseAngle && curAngle < ability.maxAngle + mouseAngle)
				{
					result.Add(intVec3);
				}
			}
			return result;
		}
			
		public static string GetOperatorNameFromDefName(string defName)
        {
			string[] temp = defName.Split('_');
			if (temp.Length <= 1) return null;
			return temp[temp.Length - 1];
        }

		//要求defName格式：前缀_随便啥_人物名；物品格式：前缀_{string thingType}_人物名
		public static string GetThingsDefName(string defName, string thingType)
        {
			string[] temp = defName.Split('_');
			if (temp.Length <= 1) return null;

			return temp[0] + "_" + thingType + "_" + temp[temp.Length - 1];
		}
		public static void PlaySound(this SoundDef sound)
		{
			Log.Message($"尝试播放{sound.defName}.{Time.realtimeSinceStartup} : {lastVoiceTime}");
			if (Time.realtimeSinceStartup - lastVoiceTime <= AK_ModSettings.voiceIntervalTime / 2.0 || Time.realtimeSinceStartup - lastVoiceTime <= lastVoiceLength || !AK_ModSettings.playOperatorVoice) return;
			lastVoiceLength = sound.Duration.max;
			sound.PlayOneShotOnCamera(null);
			lastVoiceTime = Time.realtimeSinceStartup;
			return;
		}

		//播放技能语音
		public static void PlaySound(this Pawn pawn, SFXType type)
        {
			abilitySFX[(int)type].PlayOneShot(null);
			Log.Message((pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AK_Operator")) as Hediff_Operator).voicePackDef.abilitySounds.RandomElement().defName);
			(pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AK_Operator")) as Hediff_Operator).voicePackDef.abilitySounds.RandomElement().PlaySound();
        }

		public static void LoadedGame()
        {
			lastVoiceTime = 0;
			lastVoiceLength = 0;
        }
	}
}