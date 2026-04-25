using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace AnimusForge
{
    public sealed class LordSelectionScreen
    {
        private static LordSelectionScreen _activePopup;

        private readonly ScreenBase _screen;
        private readonly GauntletLayer _layer;
        private readonly LordSelectionVM _dataSource;
        private readonly Action<List<Hero>> _onConfirm;
        private readonly Action _onCancel;
        private bool _isClosed;

        private LordSelectionScreen(ScreenBase screen, List<Hero> availableHeroes, Action<List<Hero>> onConfirm, Action onCancel)
        {
            _screen = screen;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _dataSource = new LordSelectionVM(availableHeroes, HandleConfirmRequested, HandleCancelRequested);
            _layer = new GauntletLayer("LordSelectionPopup", 4000, false);
        }

        public static bool Show(List<Hero> availableHeroes, Action<List<Hero>> onConfirm, Action onCancel)
        {
            ScreenBase topScreen = ScreenManager.TopScreen;
            if (topScreen == null)
            {
                Logger.Log("LordSelection", "Cannot show lord selection popup: TopScreen is null.");
                return false;
            }

            try
            {
                _activePopup?.Close(silent: true);
                LordSelectionScreen popup = new LordSelectionScreen(topScreen, availableHeroes, onConfirm, onCancel);
                popup.Open();
                _activePopup = popup;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("LordSelection", "Failed to show lord selection popup: " + ex);
                _activePopup?.Close(silent: true);
                _activePopup = null;
                return false;
            }
        }

        private void Open()
        {
            _layer.LoadMovie("LordSelectionPopup", _dataSource);
            _layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            try
            {
                _layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            }
            catch
            {
            }
            _screen.AddLayer(_layer);
            _layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_layer);
        }

        private void HandleConfirmRequested(List<Hero> selectedHeroes)
        {
            Close(silent: true);
            _onConfirm?.Invoke(selectedHeroes ?? new List<Hero>());
        }

        private void HandleCancelRequested()
        {
            Close(silent: true);
            _onCancel?.Invoke();
        }

        private void Close(bool silent)
        {
            if (_isClosed)
            {
                return;
            }
            _isClosed = true;

            try
            {
                _layer.IsFocusLayer = false;
                ScreenManager.TryLoseFocus(_layer);
            }
            catch
            {
            }

            try
            {
                _screen.RemoveLayer(_layer);
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    Logger.Log("LordSelection", "Failed to remove lord selection layer: " + ex.Message);
                }
            }

            _dataSource?.OnFinalize();
            if (ReferenceEquals(_activePopup, this))
            {
                _activePopup = null;
            }
        }
    }
}
