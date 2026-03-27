using System.Windows;
using Microsoft.Win32;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
public class DialogOptionsTests
{
    [Test]
    public async Task MessageBoxDialogOptions_DefaultValues_AreCorrect()
    {
        var options = new MessageBoxDialogOptions();
        await Assert.That(options.Text).IsEqualTo(string.Empty);
        await Assert.That(options.Caption).IsEqualTo(string.Empty);
        await Assert.That(options.Button).IsEqualTo(MessageBoxButton.OK);
        await Assert.That(options.Image).IsEqualTo(MessageBoxImage.None);
        await Assert.That(options.DefaultResult).IsEqualTo(MessageBoxResult.None);
        await Assert.That(options.Options).IsEqualTo(MessageBoxOptions.None);
    }

    [Test]
    public async Task MessageBoxDialogOptions_PropertyAssignment_RoundTrips()
    {
        var options = new MessageBoxDialogOptions
        {
            Text = "Are you sure?",
            Caption = "Confirm",
            Button = MessageBoxButton.YesNo,
            Image = MessageBoxImage.Warning,
            DefaultResult = MessageBoxResult.No,
            Options = MessageBoxOptions.RightAlign
        };

        await Assert.That(options.Text).IsEqualTo("Are you sure?");
        await Assert.That(options.Caption).IsEqualTo("Confirm");
        await Assert.That(options.Button).IsEqualTo(MessageBoxButton.YesNo);
        await Assert.That(options.Image).IsEqualTo(MessageBoxImage.Warning);
        await Assert.That(options.DefaultResult).IsEqualTo(MessageBoxResult.No);
        await Assert.That(options.Options).IsEqualTo(MessageBoxOptions.RightAlign);
    }

    [Test]
    public async Task OpenFileDialogOptions_DefaultValues_AreCorrect()
    {
        var options = new OpenFileDialogOptions();
        await Assert.That(options.Title).IsEqualTo(string.Empty);
        await Assert.That(options.Multiselect).IsFalse();
        await Assert.That(options.FileTypeFilter).IsEqualTo(string.Empty);
        await Assert.That(options.InitialDirectory).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task OpenFileDialogOptions_PropertyAssignment_RoundTrips()
    {
        var options = new OpenFileDialogOptions
        {
            Title = "Open File",
            Multiselect = true,
            FileTypeFilter = "Text files|*.txt",
            InitialDirectory = @"C:\Temp"
        };

        await Assert.That(options.Title).IsEqualTo("Open File");
        await Assert.That(options.Multiselect).IsTrue();
        await Assert.That(options.FileTypeFilter).IsEqualTo("Text files|*.txt");
        await Assert.That(options.InitialDirectory).IsEqualTo(@"C:\Temp");
    }

    [Test]
    public async Task OpenFileDialogOptions_ApplyTo_SetsDialogProperties()
    {
        var options = new OpenFileDialogOptions
        {
            Title = "Select File",
            Multiselect = true,
            FileTypeFilter = "All files|*.*",
            InitialDirectory = @"C:\Temp"
        };

        var dialog = new OpenFileDialog();
        options.ApplyTo(dialog);

        await Assert.That(dialog.Title).IsEqualTo("Select File");
        await Assert.That(dialog.Multiselect).IsTrue();
        await Assert.That(dialog.Filter).IsEqualTo("All files|*.*");
        await Assert.That(dialog.InitialDirectory).IsEqualTo(@"C:\Temp");
        await Assert.That(dialog.RestoreDirectory).IsTrue();
        await Assert.That(dialog.CheckFileExists).IsTrue();
        await Assert.That(dialog.CheckPathExists).IsTrue();
    }

    [Test]
    public async Task SaveFileDialogOptions_DefaultValues_AreCorrect()
    {
        var options = new SaveFileDialogOptions();
        await Assert.That(options.Title).IsEqualTo(string.Empty);
        await Assert.That(options.InitialFileName).IsEqualTo(string.Empty);
        await Assert.That(options.FileTypeFilter).IsEqualTo(string.Empty);
        await Assert.That(options.InitialDirectory).IsEqualTo(string.Empty);
        await Assert.That(options.DefaultFileExtension).IsEqualTo(string.Empty);
        await Assert.That(options.PromptForOverwrite).IsFalse();
        await Assert.That(options.PromptForCreate).IsFalse();
    }

    [Test]
    public async Task SaveFileDialogOptions_PropertyAssignment_RoundTrips()
    {
        var options = new SaveFileDialogOptions
        {
            Title = "Save As",
            InitialFileName = "document.txt",
            FileTypeFilter = "Text files|*.txt",
            InitialDirectory = @"C:\Temp",
            DefaultFileExtension = ".txt",
            PromptForOverwrite = true,
            PromptForCreate = true
        };

        await Assert.That(options.Title).IsEqualTo("Save As");
        await Assert.That(options.InitialFileName).IsEqualTo("document.txt");
        await Assert.That(options.FileTypeFilter).IsEqualTo("Text files|*.txt");
        await Assert.That(options.InitialDirectory).IsEqualTo(@"C:\Temp");
        await Assert.That(options.DefaultFileExtension).IsEqualTo(".txt");
        await Assert.That(options.PromptForOverwrite).IsTrue();
        await Assert.That(options.PromptForCreate).IsTrue();
    }

    [Test]
    public async Task SaveFileDialogOptions_ApplyTo_SetsDialogProperties()
    {
        var options = new SaveFileDialogOptions
        {
            Title = "Save As",
            InitialFileName = "doc.txt",
            FileTypeFilter = "Text files|*.txt",
            InitialDirectory = @"C:\Temp",
            DefaultFileExtension = ".txt",
            PromptForOverwrite = true,
            PromptForCreate = false
        };

        var dialog = new SaveFileDialog();
        options.ApplyTo(dialog);

        await Assert.That(dialog.Title).IsEqualTo("Save As");
        await Assert.That(dialog.FileName).IsEqualTo("doc.txt");
        await Assert.That(dialog.Filter).IsEqualTo("Text files|*.txt");
        await Assert.That(dialog.InitialDirectory).IsEqualTo(@"C:\Temp");
        // SaveFileDialog strips the leading dot from DefaultExt
        await Assert.That(dialog.DefaultExt).IsEqualTo("txt");
        await Assert.That(dialog.OverwritePrompt).IsTrue();
        await Assert.That(dialog.CreatePrompt).IsFalse();
        await Assert.That(dialog.RestoreDirectory).IsTrue();
        await Assert.That(dialog.AddExtension).IsTrue();
        await Assert.That(dialog.CheckPathExists).IsTrue();
    }

    [Test]
    public async Task OpenFolderDialogOptions_DefaultValues_AreCorrect()
    {
        var options = new OpenFolderDialogOptions();
        await Assert.That(options.Title).IsEqualTo(string.Empty);
        await Assert.That(options.Multiselect).IsFalse();
        await Assert.That(options.InitialDirectory).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task OpenFolderDialogOptions_PropertyAssignment_RoundTrips()
    {
        var options = new OpenFolderDialogOptions
        {
            Title = "Select Folder",
            Multiselect = true,
            InitialDirectory = @"C:\Temp"
        };

        await Assert.That(options.Title).IsEqualTo("Select Folder");
        await Assert.That(options.Multiselect).IsTrue();
        await Assert.That(options.InitialDirectory).IsEqualTo(@"C:\Temp");
    }

    [Test]
    public async Task OpenFolderDialogOptions_ApplyTo_SetsDialogProperties()
    {
        var options = new OpenFolderDialogOptions
        {
            Title = "Browse Folder",
            Multiselect = true,
            InitialDirectory = @"C:\Temp"
        };

        var dialog = new OpenFolderDialog();
        options.ApplyTo(dialog);

        await Assert.That(dialog.Title).IsEqualTo("Browse Folder");
        await Assert.That(dialog.Multiselect).IsTrue();
        await Assert.That(dialog.InitialDirectory).IsEqualTo(@"C:\Temp");
    }
}
