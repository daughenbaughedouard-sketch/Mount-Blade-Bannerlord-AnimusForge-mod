using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;

public class MPArmoryHeroPreviewVM : ViewModel
{
	private BasicCharacterObject _character;

	private Equipment _orgEquipmentWithoutPerks;

	private CharacterViewModel _heroVisual;

	private string _className;

	[DataSourceProperty]
	public CharacterViewModel HeroVisual
	{
		get
		{
			return _heroVisual;
		}
		set
		{
			if (value != _heroVisual)
			{
				_heroVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterViewModel>(value, "HeroVisual");
			}
		}
	}

	[DataSourceProperty]
	public string ClassName
	{
		get
		{
			return _className;
		}
		set
		{
			if (value != _className)
			{
				_className = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClassName");
			}
		}
	}

	public MPArmoryHeroPreviewVM()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		HeroVisual = new CharacterViewModel();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		BasicCharacterObject character = _character;
		ClassName = ((character != null) ? ((object)character.Name).ToString() : null) ?? "";
	}

	public unsafe void SetCharacter(BasicCharacterObject character, DynamicBodyProperties dynamicBodyProperties, int race, bool isFemale)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		_character = character;
		HeroVisual.FillFrom(character, -1, (string)null);
		CharacterViewModel heroVisual = HeroVisual;
		BodyProperties val = character.BodyPropertyRange.BodyPropertyMin;
		val = new BodyProperties(dynamicBodyProperties, ((BodyProperties)(ref val)).StaticProperties);
		heroVisual.BodyProperties = ((object)(*(BodyProperties*)(&val))/*cast due to .constrained prefix*/).ToString();
		HeroVisual.IsFemale = isFemale;
		HeroVisual.Race = race;
		ClassName = ((object)character.Name).ToString();
	}

	public void SetCharacterClass(BasicCharacterObject classCharacter)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		_character = classCharacter;
		_orgEquipmentWithoutPerks = classCharacter.Equipment;
		HeroVisual.SetEquipment(_orgEquipmentWithoutPerks);
		HeroVisual.ArmorColor1 = classCharacter.Culture.Color;
		HeroVisual.ArmorColor2 = classCharacter.Culture.Color2;
		if (NetworkMain.GameClient.PlayerData != null)
		{
			string sigil = NetworkMain.GameClient.PlayerData.Sigil;
			if (NetworkMain.GameClient.PlayerData.IsUsingClanSigil && NetworkMain.GameClient.ClanInfo != null)
			{
				sigil = NetworkMain.GameClient.ClanInfo.Sigil;
			}
			Banner val = new Banner(sigil, classCharacter.Culture.BackgroundColor1, classCharacter.Culture.ForegroundColor1);
			HeroVisual.BannerCodeText = val.BannerCode;
		}
		ClassName = ((object)classCharacter.Name).ToString();
	}

	public void SetCharacterPerks(List<IReadOnlyPerkObject> selectedPerks)
	{
		Equipment equipment = _orgEquipmentWithoutPerks.Clone(false);
		MPArmoryVM.ApplyPerkEffectsToEquipment(ref equipment, selectedPerks);
		HeroVisual.SetEquipment(equipment);
	}
}
