using Caliburn.Light;

namespace Demo.SimpleMDI
{
    public class TabViewModel : Screen, IChild
    {
        private object _parent;

        /// <summary>
        /// Gets or Sets the Parent <see cref = "IConductor" />
        /// </summary>
        public object Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
