using Microsoft.Windows.Storage.Pickers;

namespace Caliburn.Light.WinUI.Tests;

public class PickerOptionsTests
{
    // --- FileOpenPickerOptions ---

    [Test]
    public async Task FileOpenPickerOptions_DefaultCommitButtonText_IsEmpty()
    {
        var options = new FileOpenPickerOptions();
        await Assert.That(options.CommitButtonText).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task FileOpenPickerOptions_SetCommitButtonText_RetainsValue()
    {
        var options = new FileOpenPickerOptions { CommitButtonText = "Open" };
        await Assert.That(options.CommitButtonText).IsEqualTo("Open");
    }

    [Test]
    public async Task FileOpenPickerOptions_DefaultFileTypeFilter_IsNull()
    {
        var options = new FileOpenPickerOptions();
        await Assert.That(options.FileTypeFilter).IsNull();
    }

    [Test]
    public async Task FileOpenPickerOptions_SetFileTypeFilter_RetainsValue()
    {
        var filters = new List<string> { ".txt", ".pdf" };
        var options = new FileOpenPickerOptions { FileTypeFilter = filters };
        await Assert.That(options.FileTypeFilter).IsEquivalentTo(filters);
    }

    [Test]
    public async Task FileOpenPickerOptions_DefaultSuggestedStartLocation_IsUnspecified()
    {
        var options = new FileOpenPickerOptions();
        await Assert.That(options.SuggestedStartLocation).IsEqualTo(PickerLocationId.Unspecified);
    }

    [Test]
    public async Task FileOpenPickerOptions_SetSuggestedStartLocation_RetainsValue()
    {
        var options = new FileOpenPickerOptions { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        await Assert.That(options.SuggestedStartLocation).IsEqualTo(PickerLocationId.DocumentsLibrary);
    }

    [Test]
    public async Task FileOpenPickerOptions_DefaultViewMode_IsDefault()
    {
        var options = new FileOpenPickerOptions();
        await Assert.That(options.ViewMode).IsEqualTo(default(PickerViewMode));
    }

    [Test]
    public async Task FileOpenPickerOptions_SetViewMode_RetainsValue()
    {
        var options = new FileOpenPickerOptions { ViewMode = PickerViewMode.Thumbnail };
        await Assert.That(options.ViewMode).IsEqualTo(PickerViewMode.Thumbnail);
    }

    [Test]
    public async Task FileOpenPickerOptions_DefaultAllowMultiple_IsFalse()
    {
        var options = new FileOpenPickerOptions();
        await Assert.That(options.AllowMultiple).IsFalse();
    }

    [Test]
    public async Task FileOpenPickerOptions_SetAllowMultiple_RetainsValue()
    {
        var options = new FileOpenPickerOptions { AllowMultiple = true };
        await Assert.That(options.AllowMultiple).IsTrue();
    }

    // --- FileSavePickerOptions ---

    [Test]
    public async Task FileSavePickerOptions_DefaultValues_AreCorrect()
    {
        var options = new FileSavePickerOptions();
        await Assert.That(options.CommitButtonText).IsEqualTo(string.Empty);
        await Assert.That(options.DefaultFileExtension).IsEqualTo(string.Empty);
        await Assert.That(options.FileTypeChoices).IsNull();
        await Assert.That(options.SuggestedFileName).IsEqualTo(string.Empty);
        await Assert.That(options.SuggestedFolder).IsEqualTo(string.Empty);
        await Assert.That(options.SuggestedStartLocation).IsEqualTo(PickerLocationId.Unspecified);
    }

    [Test]
    public async Task FileSavePickerOptions_SetCommitButtonText_RetainsValue()
    {
        var options = new FileSavePickerOptions { CommitButtonText = "Save" };
        await Assert.That(options.CommitButtonText).IsEqualTo("Save");
    }

    [Test]
    public async Task FileSavePickerOptions_SetDefaultFileExtension_RetainsValue()
    {
        var options = new FileSavePickerOptions { DefaultFileExtension = ".docx" };
        await Assert.That(options.DefaultFileExtension).IsEqualTo(".docx");
    }

    [Test]
    public async Task FileSavePickerOptions_SetFileTypeChoices_RetainsValue()
    {
        var choices = new Dictionary<string, IList<string>>
        {
            { "Text Files", new List<string> { ".txt" } },
            { "All Files", new List<string> { ".*" } }
        };
        var options = new FileSavePickerOptions { FileTypeChoices = choices };
        await Assert.That(options.FileTypeChoices).IsNotNull();
        await Assert.That(options.FileTypeChoices!.Count).IsEqualTo(2);
    }

    [Test]
    public async Task FileSavePickerOptions_SetSuggestedFileName_RetainsValue()
    {
        var options = new FileSavePickerOptions { SuggestedFileName = "report" };
        await Assert.That(options.SuggestedFileName).IsEqualTo("report");
    }

    [Test]
    public async Task FileSavePickerOptions_SetSuggestedFolder_RetainsValue()
    {
        var options = new FileSavePickerOptions { SuggestedFolder = @"C:\Documents" };
        await Assert.That(options.SuggestedFolder).IsEqualTo(@"C:\Documents");
    }

    [Test]
    public async Task FileSavePickerOptions_SetSuggestedStartLocation_RetainsValue()
    {
        var options = new FileSavePickerOptions { SuggestedStartLocation = PickerLocationId.Desktop };
        await Assert.That(options.SuggestedStartLocation).IsEqualTo(PickerLocationId.Desktop);
    }

    // --- FolderPickerOptions ---

    [Test]
    public async Task FolderPickerOptions_DefaultValues_AreCorrect()
    {
        var options = new FolderPickerOptions();
        await Assert.That(options.CommitButtonText).IsEqualTo(string.Empty);
        await Assert.That(options.SuggestedStartLocation).IsEqualTo(PickerLocationId.Unspecified);
        await Assert.That(options.ViewMode).IsEqualTo(default(PickerViewMode));
    }

    [Test]
    public async Task FolderPickerOptions_SetCommitButtonText_RetainsValue()
    {
        var options = new FolderPickerOptions { CommitButtonText = "Select Folder" };
        await Assert.That(options.CommitButtonText).IsEqualTo("Select Folder");
    }

    [Test]
    public async Task FolderPickerOptions_SetSuggestedStartLocation_RetainsValue()
    {
        var options = new FolderPickerOptions { SuggestedStartLocation = PickerLocationId.Desktop };
        await Assert.That(options.SuggestedStartLocation).IsEqualTo(PickerLocationId.Desktop);
    }

    [Test]
    public async Task FolderPickerOptions_SetViewMode_RetainsValue()
    {
        var options = new FolderPickerOptions { ViewMode = PickerViewMode.Thumbnail };
        await Assert.That(options.ViewMode).IsEqualTo(PickerViewMode.Thumbnail);
    }
}
