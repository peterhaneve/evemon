using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Serialization.Datafiles;
using EVEMon.XmlGenerator.Interfaces;
using EVEMon.XmlGenerator.Providers;
using EVEMon.XmlGenerator.StaticData;
using EVEMon.XmlGenerator.Utils;

namespace EVEMon.XmlGenerator.Datafiles
{
    internal static class Skills
    {
        /// <summary>
        /// Generate the skills datafile.
        /// </summary>
        internal static void GenerateDatafile()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Util.ResetCounters();

            Console.WriteLine();
            Console.Write(@"Generating skills datafile... ");

            // Export skill groups
            List<SerializableSkillGroup> listOfSkillGroups = new List<SerializableSkillGroup>();

            foreach (InvGroups group in Database.InvGroupsTable.Where(
                x => x.CategoryID == DBConstants.SkillCategoryID && x.ID != DBConstants.FakeSkillsGroupID).OrderBy(x => x.Name))
            {
                SerializableSkillGroup skillGroup = new SerializableSkillGroup
                {
                    ID = group.ID,
                    Name = group.Name,
                };

                // Add skills in skill group
                skillGroup.Skills.AddRange(ExportSkillsInGroup(group).OrderBy(x => x.Name));

                // Add skill group
                listOfSkillGroups.Add(skillGroup);
            }

            // Serialize
            SkillsDatafile datafile = new SkillsDatafile();
            datafile.SkillGroups.AddRange(listOfSkillGroups);

            Util.DisplayEndTime(stopwatch);

            Util.SerializeXml(datafile, DatafileConstants.SkillsDatafile);
        }

        /// <summary>
        /// Exports the skills in the skill group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private static IEnumerable<SerializableSkill> ExportSkillsInGroup(IHasID group)
        {
            List<SerializableSkill> listOfSkillsInGroup = new List<SerializableSkill>();

            foreach (InvTypes skill in Database.InvTypesTable.Where(x => x.GroupID == group.ID))
            {
                Util.UpdatePercentDone(Database.SkillsTotalCount);

                SerializableSkill singleSkill = new SerializableSkill
                {
                    ID = skill.ID,
                    Name = skill.Name,
                    Description = skill.Description,
                    Public = skill.Published,
                    Cost = (long)skill.BasePrice,
                    AlphaLimit = (alphaLimit.ContainsKey(skill.ID)) ? alphaLimit[skill.ID] : 0,
                };

                // Export skill atributes
                Dictionary<int, long> skillAttributes = Database.DgmTypeAttributesTable.Where(
                    x => x.ItemID == skill.ID).ToDictionary(
                        attribute => attribute.AttributeID, attribute => attribute.GetInt64Value);

                singleSkill.Rank = skillAttributes.ContainsKey(DBConstants.SkillTimeConstantPropertyID) &&
                                   skillAttributes[DBConstants.SkillTimeConstantPropertyID] > 0
                    ? skillAttributes[DBConstants.SkillTimeConstantPropertyID]
                    : 1;

                singleSkill.PrimaryAttribute = skillAttributes.ContainsKey(DBConstants.PrimaryAttributePropertyID)
                    ? IntToEveAttribute(skillAttributes[DBConstants.PrimaryAttributePropertyID])
                    : EveAttribute.None;
                singleSkill.SecondaryAttribute = skillAttributes.ContainsKey(DBConstants.SecondaryAttributePropertyID)
                    ? IntToEveAttribute(
                        skillAttributes[DBConstants.SecondaryAttributePropertyID])
                    : EveAttribute.None;

                // Export prerequesities
                List<SerializableSkillPrerequisite> listOfPrerequisites = new List<SerializableSkillPrerequisite>();

                for (int i = 0; i < DBConstants.RequiredSkillPropertyIDs.Count; i++)
                {
                    if (!skillAttributes.ContainsKey(DBConstants.RequiredSkillPropertyIDs[i]) ||
                        !skillAttributes.ContainsKey(DBConstants.RequiredSkillLevelPropertyIDs[i]))
                        continue;

                    InvTypes prereqSkill = Database.InvTypesTable[skillAttributes[DBConstants.RequiredSkillPropertyIDs[i]]];

                    SerializableSkillPrerequisite preReq = new SerializableSkillPrerequisite
                    {
                        ID = prereqSkill.ID,
                        Level =
                            skillAttributes[DBConstants.RequiredSkillLevelPropertyIDs[i]],
                        Name = prereqSkill.Name
                    };

                    // Add prerequisites
                    listOfPrerequisites.Add(preReq);
                }

                // Add prerequesites to skill
                singleSkill.SkillPrerequisites.AddRange(listOfPrerequisites);

                // Add skill
                listOfSkillsInGroup.Add(singleSkill);
            }
            return listOfSkillsInGroup;
        }

        /// <summary>
        /// Gets the Eve attribute.
        /// </summary>        
        private static EveAttribute IntToEveAttribute(long attributeValue)
        {
            switch (attributeValue)
            {
                case DBConstants.CharismaPropertyID:
                    return EveAttribute.Charisma;
                case DBConstants.IntelligencePropertyID:
                    return EveAttribute.Intelligence;
                case DBConstants.MemoryPropertyID:
                    return EveAttribute.Memory;
                case DBConstants.PerceptionPropertyID:
                    return EveAttribute.Perception;
                case DBConstants.WillpowerPropertyID:
                    return EveAttribute.Willpower;
                default:
                    return EveAttribute.None;
            }
        }

        /// <summary>
        /// Hard coded list of alpha limits, since this is not available anywhere in the SDE or from ESI.
        /// </summary>
        private static Dictionary<int, int> alphaLimit = new Dictionary<int, int>()
        {
            {3339, 4}, // Amarr Battleship
            {33095, 4}, // Amarr Battlecruiser
            {3335, 4}, // Amarr Cruiser
            {33091, 4}, // Amarr Destroyer
            {3331, 4}, // Amarr Frigate
            {3343, 1}, // Amarr Industrial
            {3338, 4}, // Caldari Battleship
            {33096, 4}, // Caldari Battlecruiser
            {3334, 4}, // Caldari Cruiser
            {33092, 4}, // Caldari Destroyer
            {3330, 4}, // Caldari Frigate
            {3342, 1}, // Caldari Industrial
            {3336, 4}, // Gallente Battleship
            {33097, 4}, // Gallente Battlecruiser
            {3332, 4}, // Gallente Cruiser
            {33093, 4}, // Gallente Destroyer
            {3328, 4}, // Gallente Frigate
            {3340, 1}, // Gallente Industrial
            {3337, 4}, // Minmatar Battleship
            {33098, 4}, // Minmatar Battlecruiser
            {3333, 4}, // Minmatar Cruiser
            {33094, 4}, // Minmatar Destroyer
            {3329, 4}, // Minmatar Frigate
            {3341, 1}, // Minmatar Industrial
            {32918, 4}, // Mining Frigate
            {3327, 4}, // Spaceship Command
            {3452, 3}, // Acceleration Control
            {3450, 3}, // Afterburner
            {3453, 3}, // Evasive Maneuvering
            {3454, 3}, // High Speed Maneuvering
            {3449, 4}, // Navigation
            {3455, 3}, // Warp Drive Operation
            {11207, 3}, // Advanced Weapon Upgrades
            {3426, 5}, // CPU Management
            {3423, 4}, // Capacitor Emission Systems
            {3418, 4}, // Capacitor Management
            {3417, 3}, // Capacitor Systems Operation
            {3432, 5}, // Electronics Upgrades
            {3424, 5}, // Energy Grid Upgrades
            {3421, 2}, // Energy Pulse Weapons
            {3413, 5}, // Power Grid Management
            {28164, 4}, // Thermodynamics
            {3318, 5}, // Weapon Upgrades
            {33078, 1}, // Armor Layering
            {22806, 2}, // EM Armor Compensation
            {22807, 2}, // Explosive Armor Compensation
            {3394, 5}, // Hull Upgrades
            {22808, 2}, // Kinetic Armor Compensation
            {3392, 5}, // Mechanics
            {16069, 3}, // Remote Armor Repair Systems
            {27902, 2}, // Remote Hull Repair Systems
            {3393, 5}, // Repair Systems
            {22809, 2}, // Thermal Armor Compensation
            {12365, 2}, // EM Shield Compensation
            {12367, 2}, // Explosive Shield Compensation
            {12366, 2}, // Kinetic Shield Compensation
            {3422, 3}, // Shield Emission Systems
            {21059, 4}, // Shield Compensation
            {3419, 4}, // Shield Management
            {3416, 4}, // Shield Operation
            {3425, 4}, // Shield Upgrades
            {3420, 4}, // Tactical Shield Manipulation
            {11566, 2}, // Thermal Shield Compensation
            {3428, 3}, // Long Range Targeting
            {3431, 3}, // Signature Analysis
            {3429, 4}, // Target Management
            {3316, 4}, // Controlled bursts
            {3300, 5}, // Gunnery
            {3309, 4}, // Large Energy Turret
            {3307, 4}, // Large Hybrid Turret
            {3308, 4}, // Large Projectile Turret
            {12202, 3}, // Medium Artillery Specialization
            {12208, 3}, // Medium Autocannon Specialization
            {12204, 3}, // Medium Beam Laser Specialization
            {12211, 3}, // Medium Blaster Specialization
            {3306, 5}, // Medium Energy Turret
            {3304, 5}, // Medium Hybrid Turret
            {3305, 5}, // Medium Projectile Turret
            {12214, 3}, // Medium Pulse Laser Specialization
            {12206, 3}, // Medium Rail Specialization
            {3312, 4}, // Motion Prediction
            {3310, 4}, // Rapid Firing
            {3311, 4}, // Sharpshooter
            {12201, 3}, // Small Artillery Specialization
            {11084, 3}, // Small Autocannon Specialization
            {11083, 3}, // Small Beam Laser Specialization
            {12210, 3}, // Small Blaster Specialization
            {3303, 5}, // Small Energy Turret
            {3301, 5}, // Small Hybrid Turret
            {3302, 5}, // Small Projectile Turret
            {12213, 3}, // Small Pulse Laser Specialization
            {11082, 3}, // Small Rail Specialization
            {3315, 4}, // Surgical Strike
            {3317, 4}, // Trajectory Analysis
            {3326, 4}, // Cruise Missiles
            {20312, 3}, // Guided Missile Precision
            {25718, 3}, // Heavy Assault Missile Specialization
            {25719, 5}, // Heavy Assault Missiles
            {20211, 3}, // Heavy Missile Specialization
            {3324, 5}, // Heavy Missiles
            {20210, 3}, // Light Missile Specialization
            {3321, 5}, // Light Missiles
            {12441, 4}, // Missile Bombardment
            {3319, 5}, // Missile Launcher Operation
            {12442, 2}, // Missile Projection
            {21071, 4}, // Rapid Launch
            {20209, 3}, // Rocket Specialization
            {3320, 5}, // Rockets
            {20314, 3}, // Target Navigation Prediction
            {3325, 4}, // Torpedoes
            {20315, 3}, // Warhead Upgrades
            {3437, 4}, // Drone Avionics
            {23618, 4}, // Drone Durability
            {3442, 3}, // Drone Interfacing
            {12305, 4}, // Drone Navigation
            {23606, 4}, // drone Sharpshooting
            {3436, 5}, // Drones
            {24241, 5}, // Light Drone Operation
            {33699, 5}, // Medium Drone Operation
            {3439, 2}, // Repair Drone Operation
            {3441, 4}, // Heavy Drone Operation
            {12484, 2}, // Amarr Drone Specialization
            {12487, 2}, // Caldari Drone Specialization
            {12486, 2}, // Gallente Drone Specialization
            {12485, 2}, // Minmatar Drone Specialization
            {3427, 4}, // Electronic Warfare
            {3435, 4}, // Propulsion Jamming
            {3434, 4}, // Weapon Disruption
            {3433, 3}, // Sensor Linking
            {19921, 3}, // Target Painting
            {13278, 3}, // Archaeology
            {25811, 2}, // Astrometric Acquisition
            {25739, 2}, // Astrometric Rangefinding
            {3412, 3}, // Astrometrics
            {21718, 3}, // Hacking
            {3551, 3}, // Survey
            {26253, 3}, // Armor Rigging
            {26254, 3}, // Astronautics Rigging
            {26259, 3}, // Hybrid Weapon Rigging
            {26260, 3}, // Launcher Rigging
            {26257, 3}, // Shield Rigging
            {26261, 3}, // Projectile Weapon Rigging
            {26255, 3}, // Drones Rigging
            {26256, 3}, // Electronic superiority Rigging
            {26258, 3}, // Energy Weapon Rigging
            {26252, 3}, // Jury Rigging
            {3359, 2}, // Connections
            {3361, 2}, // Criminal Connections
            {3357, 3}, // Diplomacy
            {3894, 2}, // Distribution Connections
            {3893, 2}, // Mining Connections
            {3356, 2}, // Negotiation
            {3895, 2}, // Security Connections
            {3355, 3}, // Social
            {3348, 3}, // Leadership
            {3363, 1}, // Corporation Management
            {3446, 2}, // Broker Relations
            {16598, 2}, // Marketing
            {3443, 3}, // Trade
            {3405, 3}, // Biology
            {3411, 3}, // Cybernetics
            {24242, 1}, // Infomorph Psychology
            {3380, 5}, // Industry
            {3387, 3}, // Mass Production
            {3402, 4}, // Science
            {3386, 4}, // Mining
            {22578, 4}, // Mining Upgrades
            {45746, 1}, // Reactions
            {3385, 3}, // Reprocessing
            {25544, 2}, // Gas Cloud Harvesting
            {25863, 3}, // Salvaging
            {11584, 1} // Anchoring
        };

    }
}
