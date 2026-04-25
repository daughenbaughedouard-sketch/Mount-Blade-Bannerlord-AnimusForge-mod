using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace AnimusForge
{
    public class LordSelectionItemVM : ViewModel
    {
        private bool _isSelected;
        private string _name;
        private Hero _hero;
        private string _selectionMark;

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChangedWithValue(value, "IsSelected");
                    SelectionMark = value ? "[x]" : "[ ]";
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        public Hero Hero => _hero;

        [DataSourceProperty]
        public string SelectionMark
        {
            get => _selectionMark;
            set
            {
                if (_selectionMark != value)
                {
                    _selectionMark = value;
                    OnPropertyChangedWithValue(value, "SelectionMark");
                }
            }
        }

        public LordSelectionItemVM(Hero hero, bool isSelected = true)
        {
            _hero = hero;
            _isSelected = isSelected;
            _name = hero?.Name?.ToString() ?? "Unknown";
            _selectionMark = isSelected ? "[x]" : "[ ]";
        }

        public void ExecuteToggleSelection()
        {
            IsSelected = !IsSelected;
        }
    }

    public class LordSelectionVM : ViewModel
    {
        private MBBindingList<LordSelectionItemVM> _availableLords;
        private string _titleText;
        private string _descriptionText;
        private string _confirmText;
        private string _cancelText;
        private Action<List<Hero>> _onConfirm;
        private Action _onCancel;

        [DataSourceProperty]
        public MBBindingList<LordSelectionItemVM> AvailableLords
        {
            get => _availableLords;
            set
            {
                if (_availableLords != value)
                {
                    _availableLords = value;
                    OnPropertyChangedWithValue(value, "AvailableLords");
                }
            }
        }

        [DataSourceProperty]
        public string TitleText
        {
            get => _titleText;
            set
            {
                if (_titleText != value)
                {
                    _titleText = value;
                    OnPropertyChangedWithValue(value, "TitleText");
                }
            }
        }

        [DataSourceProperty]
        public string DescriptionText
        {
            get => _descriptionText;
            set
            {
                if (_descriptionText != value)
                {
                    _descriptionText = value;
                    OnPropertyChangedWithValue(value, "DescriptionText");
                }
            }
        }

        [DataSourceProperty]
        public string ConfirmText
        {
            get => _confirmText;
            set
            {
                if (_confirmText != value)
                {
                    _confirmText = value;
                    OnPropertyChangedWithValue(value, "ConfirmText");
                }
            }
        }

        [DataSourceProperty]
        public string CancelText
        {
            get => _cancelText;
            set
            {
                if (_cancelText != value)
                {
                    _cancelText = value;
                    OnPropertyChangedWithValue(value, "CancelText");
                }
            }
        }

        public LordSelectionVM(List<Hero> availableHeroes, Action<List<Hero>> onConfirm, Action onCancel)
        {
            _availableLords = new MBBindingList<LordSelectionItemVM>();
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            TitleText = "选择对话的领主";
            DescriptionText = "选择你想要对话的领主（可多选）";
            ConfirmText = "确认";
            CancelText = "取消";

            if (availableHeroes != null)
            {
                foreach (var hero in availableHeroes)
                {
                    if (hero != null && hero != Hero.MainHero)
                    {
                        _availableLords.Add(new LordSelectionItemVM(hero, true));
                    }
                }
            }
        }

        public void ExecuteConfirm()
        {
            var selectedHeroes = _availableLords
                .Where(x => x.IsSelected)
                .Select(x => x.Hero)
                .ToList();

            _onConfirm?.Invoke(selectedHeroes);
        }

        public void ExecuteCancel()
        {
            _onCancel?.Invoke();
        }

        public void ExecuteSelectAll()
        {
            foreach (var lord in _availableLords)
            {
                lord.IsSelected = true;
            }
        }

        public void ExecuteDeselectAll()
        {
            foreach (var lord in _availableLords)
            {
                lord.IsSelected = false;
            }
        }
    }
}
