using UnityEngine;

namespace UwU.Examples.MVVM
{
    public class UIScript : MonoBehaviour
    {
        [SerializeField] private CardView cardView;

        private CardViewModel cardViewModel;

        private void Start()
        {
            this.cardViewModel = new CardViewModel(0);
            this.cardView.Bind(this.cardViewModel);
        }

        public void SetCardValue(int value)
        {
            this.cardViewModel.Value = value;
        }
    }
}