using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Heroes.ReplayParser;

namespace Parser.Shared
{

    public class ReplayFile
    {
        public string? RandomValue { get; set; }

        public string? ReplayFingerPrint{ get; set; }

        public string? BattlegroundName { get; set; }

        public string? ShortName { get; set; }

        public int? WinningTeam { get; set; }

        public int? FirstToTen { get; set; }

        public int DraftFirstTeam { get; set; }

        public string? GameType { get; set; }

        public string? GameLength { get; set; }

        public double GameLengthTimestamp { get; set; }

        public string? GameDate { get; set; }

        public string? GameDateFormatted { get; set; }

        public string? VersionBuild { get; set; }

        public string? VersionMajor { get; set; }

        public string? Version { get; set; }

        public List<object>? Draft { get; set; }

        public List<object>? Teams { get; set; } = new List<object>();

        public string? ReplayFileName { get; set; }

        public string? ReplayFileNameFormatted { get; set; }

        public string? ReplayFilePath { get; set; }


    }

    public class ReplayTeam
    {
        public int? TeamId { get; set; }

        public string? RandomValue { get; set; }

        public string? ReplayFingerPrint{ get; set; }

        public List<object>? Players { get; set; }

        public List<object> TeamPeriodicXPBreakdown { get; set; } = new List<object>();

        //public List<object>? TeamObjectives { get; set; } = new List<object>();

        //public List<object>? TeamLevels { get; set; } = new List<object>();

    }

    public class ReplayPlayer
    {
        public string? RandomValue { get; set; }

        public string? ReplayFingerPrint{ get; set; }

        public string? Battletag { get; set; }

        public string? HeroId { get; set; }

        public string? AttributeId { get; set; }

        public int Team { get; set; }

        public long Party { get; set; }

        public bool IsWinner { get; set; }

        public string? Character { get; set; }

        public int? CharacterLevel { get; set; }

        public int? AccountLevel { get; set; }

        public bool? FirstToTen { get; set; }

        public string? PlayerType { get; set; }

        public object ScoreResult { get; set; } = new Object();

        public int? Region { get; set; }

        public int? BlizzardId { get; set; }

        public List<object> Talents { get; set; } = new List<object>();

    }

    public class ReplayPlayerTalent
    {
        public string? RandomValue { get; set; }

        public string? ReplayFingerPrint{ get; set; }

        public string? Battletag { get; set; }

        public int TalentIndex { get; set; }

        public string? TalentTier { get; set; }

        public int? TalentId { get; set; }

        public string? TalentName { get; set; }

        public string? TimeSpanSelectedString { get; set; }

        public double? TimeSpanSelected { get; set; } = 0;

    }

    public class ReplayDraftPick
    {
        public string? RandomValue { get; set; }

        public string? ReplayFingerPrint{ get; set; }

        public int DraftIndex { get; set; }

        public int Team { get; set; }

        public string? Battletag { get; set; }

        public string? AltName { get; set; }

        public int SelectedPlayerSlotId { get; set; }

        public string? PickType { get; set; }
    }
  
    public enum TeamSide
    {
        left = 0,
        right = 1,
        none = -1
    }

    public class ParserCachedFile
    {
        public string? DocumentFileName { get; set; }

        public string? ReplayFileName { get; set; }

        public string? RandomValue { get; set; }

        public string? ParsedDateTime { get; set; }

        public string? ReplayFingerPrint { get; set; }

        public bool? IsError { get; set; } = false;

        public string? ReplayParseResult { get; set; }

    }

    public class PlayerScoreResult
    {
        public int Level { get; set; } = 0;

        public int Takedowns { get; set; } = 0;
        public int SoloKills { get; set; } = 0;
        public int Assists { get; set; } = 0;
        public int Deaths { get; set; } = 0;

        public int HeroDamage { get; set; } = 0;
        public int SiegeDamage { get; set; } = 0;
        public int StructureDamage { get; set; } = 0;
        public int MinionDamage { get; set; } = 0;
        public int CreepDamage { get; set; } = 0;
        public int SummonDamage { get; set; } = 0;

        public double TimeCCdEnemyHeroes { get; set; } = 0;

        public int? Healing { get; set; } = null;
        public int SelfHealing { get; set; } = 0;
        public int RegenGlobes { get; set; } = 0;
        public int? DamageTaken { get; set; } = null;
        public int? DamageSoaked { get; set; } = null;

        public int ExperienceContribution { get; set; } = 0;
        public int TownKills { get; set; } = 0;

        public double TimeSpentDead { get; set; } = 0;

        public int MercCampCaptures { get; set; } = 0;
        public int WatchTowerCaptures { get; set; } = 0;

        public int MetaExperience { get; set; } = 0; // Exp added to the player's Account and Hero level after the match

        public int HighestKillStreak { get; set; } = 0;
        public int ProtectionGivenToAllies { get; set; } = 0;
        public int TimeSilencingEnemyHeroes { get; set; } = 0;
        public int TimeRootingEnemyHeroes { get; set; } = 0;
        public int TimeStunningEnemyHeroes { get; set; } = 0;
        public int ClutchHealsPerformed { get; set; } = 0;
        public int EscapesPerformed { get; set; } = 0;
        public int VengeancesPerformed { get; set; } = 0;
        public int OutnumberedDeaths { get; set; } = 0;
        public int TeamfightEscapesPerformed { get; set; } = 0;
        public int TeamfightHealingDone { get; set; } = 0;
        public int TeamfightDamageTaken { get; set; } = 0;
        public int TeamfightHeroDamage { get; set; } = 0;

        public int Multikill { get; set; } = 0;
        public int? PhysicalDamage { get; set; } = null;
        public int? SpellDamage { get; set; } = null;

        /// <summary>
        /// Gets or sets the amount of time the player was on fire.
        /// </summary>
        public double OnFireTimeonFire { get; set; } = 0;
        public List<object> MatchAwards { get; set; } = new List<object>();

    }

    public class ReplayTeamPeriodicXPBreakdown
    {
        public int TeamLevel { get; set; }
        public double TimeSpan { get; set; } = 0;
        public int MinionXP { get; set; }
        public int CreepXP { get; set; }
        public int StructureXP { get; set; }
        public int HeroXP { get; set; }
        public int TrickleXP { get; set; }
        public int TotalXP => this.MinionXP + this.CreepXP + this.StructureXP + this.HeroXP + this.TrickleXP;
    }
}

