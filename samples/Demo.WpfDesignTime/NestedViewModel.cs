using Caliburn.Light;

namespace Demo.WpfDesignTime
{
	public class NestedViewModel : BindableObject
	{
		private string _name;

        public NestedViewModel()
        {
            Name = "Nested";
        }

        public string Name
		{
			get { return _name; }
            set { SetProperty(ref _name, value); }
        }
	}
}
