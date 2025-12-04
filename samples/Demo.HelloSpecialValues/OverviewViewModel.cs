using Caliburn.Light;
using Caliburn.Light.WinUI;
using System.Threading.Tasks;

namespace Demo.HelloSpecialValues;

public sealed class OverviewViewModel : Screen, IHaveDisplayName
{
    private readonly IWindowManager _windowManager;

    public OverviewViewModel(IWindowManager windowManager)
    {
        _windowManager = windowManager;

        Characters = new BindableCollection<CharacterViewModel>
        {
            new CharacterViewModel("Arya Stark", "ms-appx:///resources/images/arya.jpg"),
            new CharacterViewModel("Catelyn Stark", "ms-appx:///resources/images/catelyn.jpg"),
            new CharacterViewModel("Cercei Lannister", "ms-appx:///resources/images/cercei.jpg"),
            new CharacterViewModel("Jamie Lannister", "ms-appx:///resources/images/jamie.jpg"),
            new CharacterViewModel("Jon Snow", "ms-appx:///resources/images/jon.jpg"),
            new CharacterViewModel("Rob Stark", "ms-appx:///resources/images/rob.jpg"),
            new CharacterViewModel("Sandor Clegane", "ms-appx:///resources/images/sandor.jpg"),
            new CharacterViewModel("Sansa Stark", "ms-appx:///resources/images/sansa.jpg"),
            new CharacterViewModel("Tyrion Lannister", "ms-appx:///resources/images/tyrion.jpg")
        };

        CharacterSelectedCommand = DelegateCommandBuilder.WithParameter<CharacterViewModel>()
            .OnExecute(CharacterSelected)
            .Build();
    }

    private async Task CharacterSelected(CharacterViewModel? character)
    {
        if (character is null)
            return;

        await _windowManager.ShowContentDialog(character, this, "dialog");
    }

    public string? DisplayName => "Demo.HelloSpecialValues";

    public IReadOnlyBindableCollection<CharacterViewModel> Characters { get; }

    public AsyncCommand CharacterSelectedCommand { get; }
}
