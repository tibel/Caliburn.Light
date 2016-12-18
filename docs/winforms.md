# Office and WinForms Applications

Caliburn.Light can be used from non-XAML hosts. In order to accomplish this, you must follow a slightly different procedure than for a normal WPF app.

Since your application does not initiate via the App.xaml you have to adapt the bootstrapper a bit (not much). This allows the bootstrapper to properly configure Caliburn.Light without the presence of a XAML application instance. All you need to do to start the framework is create an instance of your Bootstrapper and call the Initialize() method. Once the class is instantiated, you can use Caliburn.Light like normal, probably by invoke the IWindowManager to display new UI.

For more details see [WinFormsInterop](../samples/Demo.WinFormsInterop/) demo.
