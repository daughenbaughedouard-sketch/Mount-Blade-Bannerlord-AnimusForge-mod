using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CampaignBehaviors;

public class ShipNameCampaignBehavior : CampaignBehaviorBase
{
	[Flags]
	private enum NameTrait
	{
		None = 0,
		Aserai = 2,
		Battania = 4,
		Empire = 8,
		Khuzait = 0x10,
		Nord = 0x20,
		Sturgia = 0x40,
		Vlandia = 0x80,
		Light = 0x100,
		Medium = 0x200,
		Heavy = 0x400,
		Trade = 0x800,
		LightAndMedium = 0x300
	}

	private MBReadOnlyList<(TextObject, NameTrait, float)> _fullNames = new MBReadOnlyList<(TextObject, NameTrait, float)>((IEnumerable<(TextObject, NameTrait, float)>)new List<(TextObject, NameTrait, float)>
	{
		(new TextObject("{=p4zJbD3a}Righteous {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=EQHW6TPk}Glorious {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=FUMvrsE2}Angelic {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=obOVM8pM}Holy {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=N6CT6M1E}Sacred {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=M1Q36S4d}Divine {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=GYiAqvCR}Enduring {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=oIG8QbiK}Invincible {NAME}", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=3VaHDxBO}{NAME} of the Senate", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 4f),
		(new TextObject("{=RrI6uJAN}Royal {NAME}", (Dictionary<string, object>)null), NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=NT9EcONe}King's {NAME}", (Dictionary<string, object>)null), NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=IQ1Q0ncJ}Sable {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=LTa1b6T1}Crimson {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=l1Rs5EKR}Scarlet {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=aaYhWD7n}Azure {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=IgPHnuWN}Red {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 4f),
		(new TextObject("{=IgPHnuWN}Red {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=IgPHnuWN}Red {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade, 4f),
		(new TextObject("{=DqMfR4H9}Green {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 4f),
		(new TextObject("{=DqMfR4H9}Green {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=DqMfR4H9}Green {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade, 4f),
		(new TextObject("{=rqlsyT28}Golden {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 4f),
		(new TextObject("{=rqlsyT28}Golden {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=rqlsyT28}Golden {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade, 4f),
		(new TextObject("{=WDuVTmua}Black {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 4f),
		(new TextObject("{=WDuVTmua}Black {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=WDuVTmua}Black {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade, 4f),
		(new TextObject("{=YCHKJWPH}Silver {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 4f),
		(new TextObject("{=YCHKJWPH}Silver {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=YCHKJWPH}Silver {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade, 4f),
		(new TextObject("{=vseUmK09}Gray {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 4f),
		(new TextObject("{=vseUmK09}Gray {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=vseUmK09}Gray {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade, 4f),
		(new TextObject("{=4W6VIFQy}White {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 4f),
		(new TextObject("{=4W6VIFQy}White {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=4W6VIFQy}White {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade, 4f),
		(new TextObject("{=5h7uC3ea}Sea {NAME}", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Heavy, 4f),
		(new TextObject("{=T6M299YZ}Iron {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Khuzait | NameTrait.Heavy, 4f),
		(new TextObject("{=vBBVysYn}Bronze {NAME}", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Khuzait | NameTrait.Heavy, 4f),
		(new TextObject("{=YK07f3P5}{NAME} of the Ice", (Dictionary<string, object>)null), NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=MmJLmoTG}{NAME} of the North Wind", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=vGOdCk10}{NAME} of the West Wind", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=Gv8QS2ir}{NAME} of the South Wind", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=GUGb3elb}{NAME} of the Desert Wind", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 4f),
		(new TextObject("{=DDO8zNWb}{NAME} of the Steppe Wind", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=errJ9sPD}{NAME} of the East Wind", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=C5eXktem}{NAME} of the Tempest", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=PPxkvzaI}{NAME} of the Seven Seas", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=aBpPqNSV}{NAME} of the Oceans", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=o4f2am1S}{NAME} of the Four Winds", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=zGXMh3cK}{NAME} of the Summer Wind", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=7wacHLoB}{NAME} of the Monsoons", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 4f),
		(new TextObject("{=Sr5g7eGT}{NAME} of the Hidden Isles", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=2bpDbXgH}{NAME} of the Southern Isles", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=vaKsHBk4}{NAME} of the Jade Sea", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 4f),
		(new TextObject("{=X2hW2ZK8}{NAME} of the Lysian Gates", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=7SNvOuJZ}{NAME} of the Perfumed Isles", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=8nOygNES}{NAME} of the North Star", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=jXkBO9cE}{NAME} of the Southern Stars", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 4f),
		(new TextObject("{=YdrcIKM4}{NAME} of the Evening Star", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=KHvKnQlq}{NAME} of Balion", (Dictionary<string, object>)null), NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=D1FhdIi9}{NAME} of Geroia", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=BoahRhBP}{NAME} of the Biscan", (Dictionary<string, object>)null), NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=upUpjeLB}{NAME} of Charas", (Dictionary<string, object>)null), NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=AkL8Au7C}{NAME} of Vostrum", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=1a7vKHgp}{NAME} of Zeonica", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Trade, 4f),
		(new TextObject("{=1FIatE6X}{NAME} of Ostican", (Dictionary<string, object>)null), NameTrait.Vlandia | NameTrait.Trade, 4f),
		(new TextObject("{=RFus9sqB}Ouroboros", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=boyQwJ1m}Houndfish", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=lTbqQ9bz}Dogfish", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=jQpgsw8r}Swordfish", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Vlandia, 1f),
		(new TextObject("{=87bHj9A2}Sawfish", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire, 1f),
		(new TextObject("{=27YBAeBC}Blackfish", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=Rq9LsTZd}Codfish", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Trade, 1f),
		(new TextObject("{=9awY7d7g}Mergus", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Vlandia | NameTrait.Trade, 1f),
		(new TextObject("{=W1UHaqXn}Storm-Petrel", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=9YgoQgfF}Mermaid", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 1f),
		(new TextObject("{=ObA0FlxH}Golden Mermaid", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade, 1f),
		(new TextObject("{=7fvh1EnT}Silver Mermaid", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Trade, 1f),
		(new TextObject("{=lGJttwHt}Golden Dromedary", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 1f),
		(new TextObject("{=1IBdhv6m}White Dromedary", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 1f),
		(new TextObject("{=bGZjuUDa}Black Dromedary", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 1f),
		(new TextObject("{=AYMp00V6}Camel of the Nahasa", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade, 1f),
		(new TextObject("{=bKIfIrpa}Fighting Cockerel", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=aPhXFPcT}Red Rooster", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=WJX03gKf}Golden Eel", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=aBveRvWW}Silver Eel", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Sturgia | NameTrait.Vlandia, 1f),
		(new TextObject("{=UDgMFtzL}Moray Eel", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Vlandia, 1f),
		(new TextObject("{=xQi4P54b}Beluga", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia, 1f),
		(new TextObject("{=sTEQvac6}Kraken", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=MsLvZKOY}Stingray", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire, 1f),
		(new TextObject("{=5R17a1JM}Lobster", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Trade, 1f),
		(new TextObject("{=dOGq1Kna}Mullet", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Trade, 1f),
		(new TextObject("{=pTEJQmFt}Mackerel", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Vlandia, 1f),
		(new TextObject("{=7hp485w6}Herring", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Trade, 1f),
		(new TextObject("{=z7K50H9r}Albacore", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade, 1f),
		(new TextObject("{=9u7Xc1Ut}Senate and People", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=vLmdGlBp}Thalassarch", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=9AiyiCb1}Great Tethys", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=fXHy3mOb}Might of Cetus", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=6Cdlb2cd}Banner of Calradios", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=868vxgZt}Sun of Alixenios", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=YlOkpsf8}Autokrator", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=fupLwOHL}Vasileos", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=JxZ5IMJp}Princess Sarpea", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=ghoX4M9O}Mount Aracathos", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=uKvTvMP4}Mount Erithrys", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=Gn4eEJOa}Wrath of Typhon", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=6LlLXwOR}Smile of Akhileos", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=qQpwMEVO}Revenge of Serapeos", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=FYmdME8j}Transtemean Wind", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=MBAesOd0}Zeonic Wind", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=7sqi9P0w}Zephyr", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=cVbeR6AW}Lycanthropos", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=ACzfQv3T}Vrykolakas", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=0jgcowNU}Nereid", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=0RUL4jh8}Lamia", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=9Pr1gVrR}Myrmidon", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=RDKUfi9l}Hippalectryon", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire, 1f),
		(new TextObject("{=Qn2xEoOz}Scourge of the Barbarians", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=VNtVAxQF}Tamer of the Myzead", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=H1rQdt0h}Subduer of the Perassic", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy, 1f),
		(new TextObject("{=FNcuiONh}Sea Tsar", (Dictionary<string, object>)null), NameTrait.Sturgia | NameTrait.Heavy, 1f),
		(new TextObject("{=840RUJwg}Bogatyr", (Dictionary<string, object>)null), NameTrait.Sturgia | NameTrait.Heavy, 1f),
		(new TextObject("{=Xbr5wKmo}Archangel", (Dictionary<string, object>)null), NameTrait.Sturgia | NameTrait.Heavy, 1f),
		(new TextObject("{=eSbZi6xs}Moryana", (Dictionary<string, object>)null), NameTrait.Sturgia | NameTrait.Heavy, 1f),
		(new TextObject("{=wUrg3H2w}Chernobog's Laughter", (Dictionary<string, object>)null), NameTrait.Sturgia | NameTrait.Heavy, 1f),
		(new TextObject("{=V1t5aRMl}Stallion of Tyal", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia | NameTrait.Heavy, 1f),
		(new TextObject("{=p4OZyYgh}Vodyanoy", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia, 1f),
		(new TextObject("{=U1Qn1JZM}Karakaz", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia, 1f),
		(new TextObject("{=8zEWkLAE}Rusalka", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia, 1f),
		(new TextObject("{=dQwETpdC}Scythe of Nav", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia, 1f),
		(new TextObject("{=7ASz5f1a}Bear of Velos", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Sturgia, 1f),
		(new TextObject("{=Ciat3lsP}Mandate of the Great Sky", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Heavy, 1f),
		(new TextObject("{=TmUmadhP}Sons of the She-Wolf", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Heavy, 1f),
		(new TextObject("{=DrsB9HMG}Will of the Kurultai", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Heavy, 1f),
		(new TextObject("{=u51uZVjs}Arrow of Urkhun", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Heavy, 1f),
		(new TextObject("{=aCocYZSt}Steed of the Ultaiga", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=zjZpbQ1z}Gift of Bura Khan", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Heavy, 1f),
		(new TextObject("{=LUnfNHnM}Sword of Matyr", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=iPSzZd9B}Shyngay's Delight", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=aN2WQlbB}Sign of Ulgen", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Heavy, 1f),
		(new TextObject("{=gm5hsK25}Fury of Erlik", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=QSw6aJGV}Blessing of Ulukayin", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=qXb7YoGc}Asaligat", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=K4XxeGbc}Tulpar", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=Q33lWCaj}Talon of the Zilant", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=LGCKTQy2}Konrul", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=ofecuaPj}Guiding Star", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=RUv05qsg}Light of Dawn", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=xmz9r8P1}Wind Horse", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=5bv0Hc84}Storm-Spirit", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=C0fTWQGk}Ironskin", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=aBtcQVtm}Simurgh", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait, 1f),
		(new TextObject("{=LtGauLC1}Sigil of Queen Eshora", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=woCajT5W}Consort of Tiamat", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=2ReQJtZI}Invincible Sun", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=WAwkgdCj}Feather of Truth", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=P4IE7fXb}Lamassu", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=DKz1aPbN}Warding Hand", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=Z1x9wz33}Steed of Asera", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=cdmwGRa4}Haboob", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=5aFO3p3l}Simoom", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=q5Lfs1qS}Ghibli", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=KgSyBBFP}Pharaoh's Eye", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=oHPSr4VW}Khamsin", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=o1RLBGsT}Golden Rukh", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=TZ5CIXBH}Rukh's Talons", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=SaiEuTfS}Bird of Jebel Qaf", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=tnDzXWd5}Whirlwind", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=PWZQUadt}Moon Upon Clouds", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=nJBxI8hg}Anqa of the Sunset", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Heavy, 1f),
		(new TextObject("{=VPevzPsM}Saluqi Hound", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=yE3Y8IO8}Ghula's Kiss", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=no4fOW8Y}Water of Ziram", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=9aozS6Mk}Lord of the Horns", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=0jnH1Uza}Djinn-King", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=5jeB6Sxa}Djinn's Cavalcade", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=6JqD7i7C}Blue Flame", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=oHeJRau1}Breath of the Djinn", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=SgBaUv2r}Red Planet", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=5fFzNCdZ}Malaq's Defiance", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=pYhlkp70}Raging Hamadryas", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=Q4sO8AZd}Nahasawi", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai, 1f),
		(new TextObject("{=QHft3oRr}Rock of Glanys", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Heavy, 1f),
		(new TextObject("{=vYLbwCeF}Battle-Howl of Curlac", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Heavy, 1f),
		(new TextObject("{=gJ8JqhaQ}Mare of Eria", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Heavy, 1f),
		(new TextObject("{=imQbR7Cg}Boar of Torc Lugh", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Heavy, 1f),
		(new TextObject("{=nATHwmtR}Queen Tara", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Heavy, 1f),
		(new TextObject("{=SZPRJAAf}Bull of Cul", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Heavy, 1f),
		(new TextObject("{=LNDMIMVb}Lir's Wrath", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania, 1f),
		(new TextObject("{=tUvmEb8T}Dornal of the Harp", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania, 1f),
		(new TextObject("{=eHZguHKP}Hound of the Otherworld", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania, 1f),
		(new TextObject("{=AdNm9ieF}Bellow of Tryth", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania, 1f),
		(new TextObject("{=NLQvMD8L}Ark of the Gal", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania, 1f),
		(new TextObject("{=IpVZvz06}Shriek of Cathern", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania, 1f),
		(new TextObject("{=he6PPJIv}Ocean-Steed", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=Ln6EAz7S}Wave-Breaker", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=QRbebVR6}Salt Mare", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=1ybOm0PV}Woe-Bringer", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=tCzb3orN}Widow-Maker", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=Xj2dvQZe}Barrow-Filler", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=4FIUt3SN}Ran's Doorman", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=Wr1aXEU6}Hull-Biter", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=cba5bHbj}Stormcrow", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=yqBo2714}Gale-Rider", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=eV8ZaxVK}Eel-Feeder", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=n2c3mOar}Oaken Serpent", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=2o9iEiZD}Naglfar-Builder", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=5vRL70It}Devouring Wolf", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=nGUBW6v0}Fryr's Pocket-Contents", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=O3HM2T4Y}Corpse-Forger", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=5mxFBDao}Wind's Teeth", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=5bo6R8Pj}Bloody Wake", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=rMFwEIBG}Terror's Envoy", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=GF8NI4ak}Scythe of Men", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=b4dbbGSO}Foe-Scatterer", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=gazxad3b}Death's Harbringer", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=y8t3Hbq0}Breath-Quencher", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=d0ribjEv}Hralnar's Bane", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=4lCPiaXb}Utgard's Joke", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=uCCz0lIr}Keel-Snapper", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=qfmqjQVg}Draugr", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=u8xi68be}Frost-Giant", (Dictionary<string, object>)null), NameTrait.Nord | NameTrait.Heavy, 1f),
		(new TextObject("{=2ZLW2bGS}Steed of the Whale-Road", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Nord | NameTrait.Heavy | NameTrait.Trade, 1f),
		(new TextObject("{=1gVLf0pN}Ale-Cask", (Dictionary<string, object>)null), NameTrait.Nord | NameTrait.Trade, 1f),
		(new TextObject("{=vwV4IPbX}Rorqual", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=yFOn97b1}Cachalot", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=oil0bod6}Wyvern", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=Zp0vicNN}Salamander", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=TcNaPjpT}Basilisk", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=cvQAbYTz}Cameleopard", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=Ob41ouaL}Draconopedes", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=nX3uZGNy}Jackdaw", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=JqzQyz4P}Manticore", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=5rBfXrWW}Zedrosis", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=G9gL45wV}Hippocampus", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=AvdcY9xx}Porbeagle", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=B2jB58ID}Gatopard", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=buCTTnvU}Bold Vilund", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=dVOdKBvE}Good King Bonneric", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=0YhJD3ei}Worthy Rotbard", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=miaiftLB}Paladin Aganalt", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=oEuc40lO}Loyal Gundelm", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=Q0U5s3wP}Bayard", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=OyoWQKCf}Vigilant", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=T5LYBako}Pale Horseman", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=XiND8dN3}Saucy Gallard", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia | NameTrait.Heavy, 1f),
		(new TextObject("{=RBKgTtug}Cunning Tarsil", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f),
		(new TextObject("{=MBypc4Tk}Alerion-Bird", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Vlandia, 1f)
	});

	private MBReadOnlyList<(TextObject, NameTrait)> _firstNames = new MBReadOnlyList<(TextObject, NameTrait)>((IEnumerable<(TextObject, NameTrait)>)new List<(TextObject, NameTrait)>
	{
		(new TextObject("{=n4V81LNV}Jackal", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Khuzait),
		(new TextObject("{=R8I4QRvS}Gazelle", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=Llbz2iqf}Leopard", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Khuzait),
		(new TextObject("{=oshK5hAJ}Panther", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=oe6XS1cg}Hound", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=lG0KHx9d}Lynx", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=eo1F3Ghs}Cheetah", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai),
		(new TextObject("{=mdFJonjK}Ibex", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai),
		(new TextObject("{=wRtDPT3i}Falcon", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=AUaUGDaS}Kestrel", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=aSuWXMiM}Eagle", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Vlandia),
		(new TextObject("{=4dDCLq6Y}Ostrich", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=NVKwvl1G}Raven", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=VKFTub9a}Hawk", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Vlandia),
		(new TextObject("{=3dFnXRau}Heron", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=aSuWXMiM}Eagle", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=spgbMr1c}Parrot", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=4D0Y25hE}Owl", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=6RvO9UVG}Serpent", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire),
		(new TextObject("{=LTOaBiw3}Viper", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=usWAF8Wz}Asp", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Khuzait),
		(new TextObject("{=MbwwhiBo}Wolf", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=jjLUSzAk}Fox", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=ZQ4yL6gm}Hind", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=GUZRA5FT}Mare", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=TamM3Dpt}Unicorn", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=MbwwhiBo}Wolf", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=v4IDh2rE}Ghost", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Battania | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=shipNameRam}Ram", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai),
		(new TextObject("{=C5bsSTdu}Witch", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=TvbM2SMy}Centaur", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire),
		(new TextObject("{=jINLipTa}Scorpion", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=xerBRVAL}Wasp", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=IM1Fbb2V}Hornet", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=sxkwm8qn}Palmatian", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire),
		(new TextObject("{=PnfGEvfu}Canterion", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire),
		(new TextObject("{=nIfwtTXx}Ibis", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire),
		(new TextObject("{=hLqXTb5N}Badger", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Empire | NameTrait.Sturgia | NameTrait.Vlandia),
		(new TextObject("{=0bjYJLMo}Ferret", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Empire),
		(new TextObject("{=cqPflDvo}Pelican", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=cqPflDvo}Pelican", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=1TBEbQbp}Dolphin", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=1TBEbQbp}Dolphin", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=S51n3cnJ}Gull", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=S51n3cnJ}Gull", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=ZTfLT4dD}Cormorant", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=ZTfLT4dD}Cormorant", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=Ludz63ZI}Albatross", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=Ludz63ZI}Albatross", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=dqD0HRje}Osprey", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=dqD0HRje}Osprey", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=tqgCWg4i}Marlin", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=nFFRwbCy}Barracuda", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia),
		(new TextObject("{=39zy02Jd}Hare", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=iW7EXqiS}Roebuck", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=Zyi2ILYy}Antelope", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=o61Aevoo}Spoonbill", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Trade),
		(new TextObject("{=96uWB0JQ}Kingfisher", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=ZgVtOLFQ}Otter", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=LivxbamB}Marten", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=xpWEXt4K}Heifer", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=ZSA1mySL}Swan", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Battania | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=ZQ4yL6gm}Hind", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Trade),
		(new TextObject("{=8Aa4J5VU}Bear", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Khuzait | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=sRGUcmGT}Buffalo", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Heavy | NameTrait.Trade),
		(new TextObject("{=ecbh2GPS}Stallion", (Dictionary<string, object>)null), NameTrait.LightAndMedium | NameTrait.Aserai | NameTrait.Khuzait | NameTrait.Sturgia | NameTrait.Heavy),
		(new TextObject("{=0OrIliBh}Boar", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=gXKRAWmN}Behemoth", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=0bR3n4TR}Leviathan", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=GkvX7z6Y}Dragon", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Nord | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=iB1OFdgG}Troll", (Dictionary<string, object>)null), NameTrait.Nord | NameTrait.Heavy),
		(new TextObject("{=cfn3pbPM}Giant", (Dictionary<string, object>)null), NameTrait.Nord | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=LeN5ab67}Griffin", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=lZJGAUmb}Crocodile", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=nEIeg8bj}Wyrm", (Dictionary<string, object>)null), NameTrait.Nord | NameTrait.Heavy),
		(new TextObject("{=1EUJ4F5o}Bull", (Dictionary<string, object>)null), NameTrait.Battania | NameTrait.Khuzait | NameTrait.Nord | NameTrait.Heavy),
		(new TextObject("{=D0SX1cFQ}Lion", (Dictionary<string, object>)null), NameTrait.Khuzait | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=VMaalDyk}Elephant", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Khuzait | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=54SsKRD0}Walrus", (Dictionary<string, object>)null), NameTrait.Nord | NameTrait.Sturgia | NameTrait.Heavy | NameTrait.Trade),
		(new TextObject("{=8qMm3VIB}Majesty", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=aoY4ekls}Imperium", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=MWUzIGTJ}Destiny", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=uUEvDtIY}Wrath", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=4yqIAUZa}Concord", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=azoX77Hp}Wisdom", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=nrkcgic9}Triumph", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=BvD7h8gD}Mandate", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=YIStPMzW}Justice", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=nVhc43US}Guardian", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=9KPCUcTL}Sovereignty", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Heavy),
		(new TextObject("{=shipNameFury}Fury", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=f5WWBvGQ}Splendor", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Heavy),
		(new TextObject("{=v5dpjybs}Bounty", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=L3bOOJ7Q}Treasure", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=Zpds5B8d}Chalice", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=WxxVi13T}Pearl", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=FPyhdxJl}Jewel", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=yW3FevJR}Diamond", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=bUNpw29g}Emerald", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=MnzHURUf}Fortune", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=G9zmdS4J}Blessing", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=oMP4RhpF}Luck", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=BMm6tsRm}Princess", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=smlzWHsW}Maiden", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=eodelMzf}Lady", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=SHWI20zH}Queen", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=4TKA4kbv}Bride", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=ZAbwnp54}Fragrance", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=FLa5OuyK}Wanderer", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=xKXE1YrD}Pilgrim", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Sturgia | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=KTYNd9ps}Angel", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=JS17OAwM}Beacon", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=f8b5go27}Flower", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade),
		(new TextObject("{=jLcl52Vw}Rose", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=7EUZITUE}Lotus", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Trade),
		(new TextObject("{=wJRNbgRJ}Jasmine", (Dictionary<string, object>)null), NameTrait.Aserai | NameTrait.Empire | NameTrait.Trade),
		(new TextObject("{=oKLSbtdr}Lily", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade),
		(new TextObject("{=6hdP6O2N}Nymph", (Dictionary<string, object>)null), NameTrait.Empire | NameTrait.Vlandia | NameTrait.Trade)
	});

	public override void SyncData(IDataStore dataStore)
	{
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
	}

	private void OnShipOwnerChanged(Ship ship, PartyBase owner, ShipOwnerChangeDetail detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)detail == 3 || (int)detail == 4)
		{
			AssignNameToShip(ship);
		}
	}

	private TextObject GetRandomFullName(List<int> availableWeights, float totalWeight)
	{
		float num = MBRandom.RandomFloatRanged(totalWeight);
		for (int i = 0; i < availableWeights.Count; i++)
		{
			num -= ((List<(TextObject, NameTrait, float)>)(object)_fullNames)[availableWeights[i]].Item3;
			if (num < 0f)
			{
				return ((List<(TextObject, NameTrait, float)>)(object)_fullNames)[availableWeights[i]].Item1;
			}
		}
		return null;
	}

	private void AssignNameToShip(Ship ship)
	{
		float num = 0f;
		NameTrait nameFlags = GetNameFlags(ship);
		List<int> list = new List<int>();
		for (int i = 0; i < ((List<(TextObject, NameTrait, float)>)(object)_fullNames).Count; i++)
		{
			if (Extensions.HasAllFlags<NameTrait>(((List<(TextObject, NameTrait, float)>)(object)_fullNames)[i].Item2, nameFlags))
			{
				list.Add(i);
				num += ((List<(TextObject, NameTrait, float)>)(object)_fullNames)[i].Item3;
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		TextObject val = GetRandomFullName(list, num).CopyTextObject();
		list.Clear();
		for (int j = 0; j < ((List<(TextObject, NameTrait)>)(object)_firstNames).Count; j++)
		{
			if (Extensions.HasAllFlags<NameTrait>(((List<(TextObject, NameTrait)>)(object)_firstNames)[j].Item2, nameFlags))
			{
				list.Add(j);
			}
		}
		if (list.Count > 0)
		{
			TextObject val2 = ((List<(TextObject, NameTrait)>)(object)_firstNames)[Extensions.GetRandomElement<int>((IReadOnlyList<int>)list)].Item1.CopyTextObject();
			val.SetTextVariable("NAME", val2);
			ship.SetName(val);
		}
	}

	private static NameTrait GetNameFlags(Ship ship)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Invalid comparison between Unknown and I4
		NameTrait nameTrait = NameTrait.None;
		if (ship.ShipHull.IsTradeShip)
		{
			nameTrait |= NameTrait.Trade;
		}
		else if ((int)ship.ShipHull.Type == 0)
		{
			nameTrait |= NameTrait.Light;
		}
		else if ((int)ship.ShipHull.Type == 1)
		{
			nameTrait |= NameTrait.Medium;
		}
		else if ((int)ship.ShipHull.Type == 2)
		{
			nameTrait |= NameTrait.Heavy;
		}
		CultureObject culture = ship.Owner.Culture;
		if (((MBObjectBase)culture).StringId == "aserai")
		{
			nameTrait |= NameTrait.Aserai;
		}
		else if (((MBObjectBase)culture).StringId == "khuzait")
		{
			nameTrait |= NameTrait.Khuzait;
		}
		else if (((MBObjectBase)culture).StringId == "vlandia")
		{
			nameTrait |= NameTrait.Vlandia;
		}
		else if (((MBObjectBase)culture).StringId == "sturgia")
		{
			nameTrait |= NameTrait.Sturgia;
		}
		else if (((MBObjectBase)culture).StringId == "battania")
		{
			nameTrait |= NameTrait.Battania;
		}
		else if (((MBObjectBase)culture).StringId == "empire")
		{
			nameTrait |= NameTrait.Empire;
		}
		else if (((MBObjectBase)culture).StringId == "nord")
		{
			nameTrait |= NameTrait.Nord;
		}
		return nameTrait;
	}
}
