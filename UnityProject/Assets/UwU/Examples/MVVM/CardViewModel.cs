using UwU.MVVM;

namespace UwU.Examples.MVVM
{
    [System.Serializable]
    public class CardViewModel : ViewModelBase
    {
        private int value;

        public int Value
        {
            get => this.value;
            set => SetProperty(ref this.value, value);
        }

        public CardViewModel(int value)
        {
            this.value = value;
        }
    }
}