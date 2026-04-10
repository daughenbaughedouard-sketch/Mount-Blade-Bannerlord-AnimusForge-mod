using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.Inquiries;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletQueryManager : GlobalLayer
{
	private bool _isInitialized;

	private Queue<Tuple<Type, object, bool, bool>> _inquiryQueue;

	private bool _isLastActiveGameStatePaused;

	private GauntletLayer _gauntletLayer;

	private SingleQueryPopUpVM _singleQueryPopupVM;

	private MultiSelectionQueryPopUpVM _multiSelectionQueryPopUpVM;

	private TextQueryPopUpVM _textQueryPopUpVM;

	private static PopUpBaseVM _activeDataSource;

	private static object _activeQueryData;

	private GauntletMovieIdentifier _movie;

	private Dictionary<Type, Action<object, bool, bool>> _createQueryActions;

	public void Initialize()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		if (!_isInitialized)
		{
			_isInitialized = true;
			_inquiryQueue = new Queue<Tuple<Type, object, bool, bool>>();
			InformationManager.OnShowInquiry += CreateQuery;
			InformationManager.OnShowTextInquiry += CreateTextQuery;
			MBInformationManager.OnShowMultiSelectionInquiry += CreateMultiSelectionQuery;
			InformationManager.OnHideInquiry += CloseQuery;
			InformationManager.IsAnyInquiryActiveInternal = (Func<bool>)Delegate.Combine(InformationManager.IsAnyInquiryActiveInternal, new Func<bool>(OnIsAnyInquiryActiveQuery));
			_singleQueryPopupVM = new SingleQueryPopUpVM((Action)CloseQuery);
			_multiSelectionQueryPopUpVM = new MultiSelectionQueryPopUpVM((Action)CloseQuery);
			_textQueryPopUpVM = new TextQueryPopUpVM((Action)CloseQuery);
			_gauntletLayer = new GauntletLayer("QueryManager", 19501, false);
			_createQueryActions = new Dictionary<Type, Action<object, bool, bool>>
			{
				{
					typeof(InquiryData),
					delegate(object data, bool pauseState, bool prioritize)
					{
						//IL_0002: Unknown result type (might be due to invalid IL or missing references)
						//IL_000e: Expected O, but got Unknown
						CreateQuery((InquiryData)data, pauseState, prioritize);
					}
				},
				{
					typeof(TextInquiryData),
					delegate(object data, bool pauseState, bool prioritize)
					{
						//IL_0002: Unknown result type (might be due to invalid IL or missing references)
						//IL_000e: Expected O, but got Unknown
						CreateTextQuery((TextInquiryData)data, pauseState, prioritize);
					}
				},
				{
					typeof(MultiSelectionInquiryData),
					delegate(object data, bool pauseState, bool prioritize)
					{
						//IL_0002: Unknown result type (might be due to invalid IL or missing references)
						//IL_000e: Expected O, but got Unknown
						CreateMultiSelectionQuery((MultiSelectionInquiryData)data, pauseState, prioritize);
					}
				}
			};
			((GlobalLayer)this).Layer = (ScreenLayer)(object)_gauntletLayer;
			((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)this, true);
		}
		ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, true);
	}

	private bool OnIsAnyInquiryActiveQuery()
	{
		return _activeDataSource != null;
	}

	internal void InitializeKeyVisuals()
	{
		((PopUpBaseVM)_singleQueryPopupVM).SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		((PopUpBaseVM)_singleQueryPopupVM).SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		((PopUpBaseVM)_multiSelectionQueryPopUpVM).SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		((PopUpBaseVM)_multiSelectionQueryPopUpVM).SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		((PopUpBaseVM)_textQueryPopUpVM).SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		((PopUpBaseVM)_textQueryPopUpVM).SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
	}

	protected override void OnEarlyTick(float dt)
	{
		((GlobalLayer)this).OnEarlyTick(dt);
		if (_activeDataSource != null)
		{
			if (ScreenManager.FocusedLayer != ((GlobalLayer)this).Layer && (ScreenManager.FocusedLayer == null || ScreenManager.FocusedLayer.InputRestrictions.Order <= ((GlobalLayer)this).Layer.InputRestrictions.Order))
			{
				SetLayerFocus(isFocused: true);
			}
			if (_activeDataSource.IsButtonOkShown && _activeDataSource.IsButtonOkEnabled && ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/panels/next");
				_activeDataSource.ExecuteAffirmativeAction();
			}
			else if (_activeDataSource.IsButtonCancelShown && ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/panels/next");
				_activeDataSource.ExecuteNegativeAction();
			}
		}
	}

	protected override void OnLateTick(float dt)
	{
		((GlobalLayer)this).OnLateTick(dt);
		PopUpBaseVM activeDataSource = _activeDataSource;
		if (activeDataSource != null)
		{
			activeDataSource.OnTick(dt);
		}
	}

	private void CreateQuery(InquiryData data, bool pauseGameActiveState, bool prioritize)
	{
		if (_activeDataSource != null)
		{
			if (data == null)
			{
				Debug.FailedAssert("Trying to create query with null data!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletQueryManager.cs", "CreateQuery", 127);
			}
			else if (CheckIfQueryDataIsEqual(_activeQueryData, data) || _inquiryQueue.Any((Tuple<Type, object, bool, bool> x) => CheckIfQueryDataIsEqual(x.Item2, data)))
			{
				Debug.FailedAssert("Trying to create query but it is already present! Title: " + data.TitleText, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletQueryManager.cs", "CreateQuery", 132);
			}
			else if (prioritize)
			{
				QueueAndShowNewData(data, pauseGameActiveState, prioritize);
			}
			else
			{
				_inquiryQueue.Enqueue(new Tuple<Type, object, bool, bool>(typeof(InquiryData), data, pauseGameActiveState, prioritize));
			}
		}
		else
		{
			_singleQueryPopupVM.SetData(data);
			_movie = _gauntletLayer.LoadMovie("SingleQueryPopup", (ViewModel)(object)_singleQueryPopupVM);
			_activeDataSource = (PopUpBaseVM)(object)_singleQueryPopupVM;
			_activeQueryData = data;
			HandleQueryCreated(data.SoundEventPath, pauseGameActiveState);
		}
	}

	private void CreateTextQuery(TextInquiryData data, bool pauseGameActiveState, bool prioritize)
	{
		if (_activeDataSource != null)
		{
			if (data == null)
			{
				Debug.FailedAssert("Trying to create textQuery with null data!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletQueryManager.cs", "CreateTextQuery", 162);
			}
			else if (CheckIfQueryDataIsEqual(_activeQueryData, data) || _inquiryQueue.Any((Tuple<Type, object, bool, bool> x) => CheckIfQueryDataIsEqual(x.Item2, data)))
			{
				Debug.FailedAssert("Trying to create textQuery but it is already present! Title: " + data.TitleText, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletQueryManager.cs", "CreateTextQuery", 167);
			}
			else if (prioritize)
			{
				QueueAndShowNewData(data, pauseGameActiveState, prioritize);
			}
			else
			{
				_inquiryQueue.Enqueue(new Tuple<Type, object, bool, bool>(typeof(TextInquiryData), data, pauseGameActiveState, prioritize));
			}
		}
		else
		{
			_textQueryPopUpVM.SetData(data);
			_movie = _gauntletLayer.LoadMovie("TextQueryPopup", (ViewModel)(object)_textQueryPopUpVM);
			_activeDataSource = (PopUpBaseVM)(object)_textQueryPopUpVM;
			_activeQueryData = data;
			HandleQueryCreated(data.SoundEventPath, pauseGameActiveState);
		}
	}

	private void CreateMultiSelectionQuery(MultiSelectionInquiryData data, bool pauseGameActiveState, bool prioritize)
	{
		if (_activeDataSource != null)
		{
			if (data == null)
			{
				Debug.FailedAssert("Trying to create multiSelectionQuery with null data!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletQueryManager.cs", "CreateMultiSelectionQuery", 197);
			}
			else if (CheckIfQueryDataIsEqual(_activeQueryData, data) || _inquiryQueue.Any((Tuple<Type, object, bool, bool> x) => CheckIfQueryDataIsEqual(x.Item2, data)))
			{
				Debug.FailedAssert("Trying to create multiSelectionQuery but it is already present! Title: " + data.TitleText, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletQueryManager.cs", "CreateMultiSelectionQuery", 202);
			}
			else if (prioritize)
			{
				QueueAndShowNewData(data, pauseGameActiveState, prioritize);
			}
			else
			{
				_inquiryQueue.Enqueue(new Tuple<Type, object, bool, bool>(typeof(MultiSelectionInquiryData), data, pauseGameActiveState, prioritize));
			}
		}
		else
		{
			_multiSelectionQueryPopUpVM.SetData(data);
			_movie = _gauntletLayer.LoadMovie("MultiSelectionQueryPopup", (ViewModel)(object)_multiSelectionQueryPopUpVM);
			_activeDataSource = (PopUpBaseVM)(object)_multiSelectionQueryPopUpVM;
			_activeQueryData = data;
			HandleQueryCreated(data.SoundEventPath, pauseGameActiveState);
		}
	}

	private void QueueAndShowNewData(object newInquiryData, bool pauseGameActiveState, bool prioritize)
	{
		Queue<Tuple<Type, object, bool, bool>> queue = new Queue<Tuple<Type, object, bool, bool>>();
		queue.Enqueue(new Tuple<Type, object, bool, bool>(newInquiryData.GetType(), newInquiryData, pauseGameActiveState, prioritize));
		queue.Enqueue(new Tuple<Type, object, bool, bool>(_activeQueryData.GetType(), _activeQueryData, _isLastActiveGameStatePaused, item4: false));
		_inquiryQueue = CombineQueues(queue, _inquiryQueue);
		CloseQuery();
	}

	private void HandleQueryCreated(string soundEventPath, bool pauseGameActiveState)
	{
		InformationManager.HideTooltip();
		_activeDataSource.ForceRefreshKeyVisuals();
		SetLayerFocus(isFocused: true);
		_isLastActiveGameStatePaused = pauseGameActiveState;
		if (_isLastActiveGameStatePaused)
		{
			GameStateManager.Current.RegisterActiveStateDisableRequest((object)this);
			MBCommon.PauseGameEngine();
		}
		if (!string.IsNullOrEmpty(soundEventPath))
		{
			SoundEvent.PlaySound2D(soundEventPath);
		}
	}

	private void CloseQuery()
	{
		if (_activeDataSource == null)
		{
			return;
		}
		SetLayerFocus(isFocused: false);
		if (_isLastActiveGameStatePaused)
		{
			GameStateManager.Current.UnregisterActiveStateDisableRequest((object)this);
			MBCommon.UnPauseGameEngine();
		}
		if (_movie != null)
		{
			_gauntletLayer.ReleaseMovie(_movie);
			_movie = null;
		}
		_activeDataSource.OnClearData();
		_activeDataSource = null;
		_activeQueryData = null;
		if (_inquiryQueue.Count > 0)
		{
			Tuple<Type, object, bool, bool> tuple = _inquiryQueue.Dequeue();
			if (_createQueryActions.TryGetValue(tuple.Item1, out var value))
			{
				value(tuple.Item2, tuple.Item3, tuple.Item4);
			}
			else
			{
				Debug.FailedAssert("Invalid data type for query", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletQueryManager.cs", "CloseQuery", 294);
			}
		}
	}

	private void SetLayerFocus(bool isFocused)
	{
		if (isFocused)
		{
			ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, false);
			((GlobalLayer)this).Layer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(((GlobalLayer)this).Layer);
			((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		}
		else
		{
			((GlobalLayer)this).Layer.InputRestrictions.ResetInputRestrictions();
			ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, true);
			((GlobalLayer)this).Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(((GlobalLayer)this).Layer);
		}
	}

	private static Queue<T> CombineQueues<T>(Queue<T> t1, Queue<T> t2)
	{
		Queue<T> queue = new Queue<T>();
		int count = t1.Count;
		for (int i = 0; i < count; i++)
		{
			queue.Enqueue(t1.Dequeue());
		}
		count = t2.Count;
		for (int j = 0; j < count; j++)
		{
			queue.Enqueue(t2.Dequeue());
		}
		return queue;
	}

	private static bool CheckIfQueryDataIsEqual(object queryData1, object queryData2)
	{
		InquiryData val;
		if ((val = (InquiryData)((queryData1 is InquiryData) ? queryData1 : null)) != null)
		{
			return val.HasSameContentWith(queryData2);
		}
		TextInquiryData val2;
		if ((val2 = (TextInquiryData)((queryData1 is TextInquiryData) ? queryData1 : null)) != null)
		{
			return val2.HasSameContentWith(queryData2);
		}
		MultiSelectionInquiryData val3;
		if ((val3 = (MultiSelectionInquiryData)((queryData1 is MultiSelectionInquiryData) ? queryData1 : null)) != null)
		{
			return val3.HasSameContentWith(queryData2);
		}
		return false;
	}
}
