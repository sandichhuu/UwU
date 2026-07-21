using TMPro;
using UnityEngine;
using UwU.MVVM;

namespace UwU.Examples.MVVM
{
    public class CardView : ViewBase<CardViewModel>
    {
        [SerializeField] private TMP_Text textValue;

        protected override void RegisterBindings()
        {
            BindProperty(nameof(CardViewModel.Value), UpdateValue);
        }

        protected override void Refresh()
        {
            this.textValue.text = this.ViewModel.Value.ToString();
        }

        private void UpdateValue()
        {
            this.textValue.text = this.ViewModel.Value.ToString();
        }
    }
}