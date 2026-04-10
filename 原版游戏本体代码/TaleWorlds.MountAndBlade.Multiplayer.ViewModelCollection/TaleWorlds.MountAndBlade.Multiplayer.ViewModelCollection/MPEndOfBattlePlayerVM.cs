using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MPEndOfBattlePlayerVM : MPPlayerVM
{
	private readonly int _placement;

	private readonly int _displayedScore;

	private TextObject _scoreTextObj = new TextObject("{=Kvqb1lQR}{SCORE} Score", (Dictionary<string, object>)null);

	private string _placementText;

	private string _scoreText;

	[DataSourceProperty]
	public string PlacementText
	{
		get
		{
			return _placementText;
		}
		set
		{
			if (value != _placementText)
			{
				_placementText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlacementText");
			}
		}
	}

	[DataSourceProperty]
	public string ScoreText
	{
		get
		{
			return _scoreText;
		}
		set
		{
			if (value != _scoreText)
			{
				_scoreText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ScoreText");
			}
		}
	}

	public MPEndOfBattlePlayerVM(MissionPeer peer, int displayedScore, int placement)
		: base(peer)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		_placement = placement;
		_displayedScore = displayedScore;
		BasicCharacterObject val = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
		val.UpdatePlayerCharacterBodyProperties(((PeerComponent)peer).Peer.BodyProperties, ((PeerComponent)peer).Peer.Race, ((PeerComponent)peer).Peer.IsFemale);
		BodyProperties bodyProperties = ((PeerComponent)peer).Peer.BodyProperties;
		val.Age = ((BodyProperties)(ref bodyProperties)).Age;
		bodyProperties = ((PeerComponent)peer).Peer.BodyProperties;
		RefreshPreview(val, ((BodyProperties)(ref bodyProperties)).DynamicProperties, ((PeerComponent)peer).Peer.IsFemale);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		_scoreTextObj.SetTextVariable("SCORE", _displayedScore);
		ScoreText = ((object)_scoreTextObj).ToString();
		PlacementText = Common.ToRoman(_placement);
	}
}
