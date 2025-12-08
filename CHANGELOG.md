# Changelog

All notable changes to Caliburn.Light will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

## [6.1.0] - 2025-12-08

### Added
- Added **Avalonia** support
- WinUI: Added `ContentDialogLifecycle`
- WinUI: Added `IWindowManager.ShowContentDialog(viewModel, ownerViewModel, context)`

### Changed
- `IWindowManager` methods throw when owner window cannot be determined
- bind to `IHaveDisplayName` in `Ensure` methods only
- optimize `IViewModelLocator` lookup

### Removed
- WPF: Removed `IWindowManager.ShowPopup(viewModel, context)`

## [6.0.0] - 2025-11-23

### Added
- Added `ViewAware.OnViewDetached(view, context)` and `IViewAware.GetViews()` 
- Added `ViewModelLocatorConfiguration` 
- WPF: Added `PageLifecycle`
- WPF: Added `IWindowManager.ShowOpenFolderDialog(settings, ownerViewModel)`
- WinUI: Added `WindowLifecycle`, `PopupLifecycle` and `PageLifecycle`
- WinUI: Added `WindowManager`

### Changed
- .NET 8.0 only
- Enabled nullable

### Removed
- Removed `SimpleContainer` and `ViewModelTypeResolver`
- Removed `IViewAware.ViewAttached` and `ViewAware.Views`
- WinUI: Removed `SuspensionManager`, `NavigationService` and `FrameAdapter`

### Fixed
- Fixed context parameter to be of type string everywhere

## [5.3.0] - 2025-02-22

### Added
- Added .NET 8.0 support

### Changed
- Optimized fix for IViewModelLocator not found
- WinUI: Updated AppSDK to 1.6.0

## [5.2.1] - 2023-08-20

### Fixed
- Fixed `IViewModelLocator` not found

## [5.2.0] - 2022-11-18

### Changed
- WinUI: Updated AppSDK to 1.2.1
- WinUI: Use DispatcherQueue

### Changed
- Removed .NET Core 3.1
- WinUI: Removed sharing service

## [5.1.0] - 2021-11-26

### Changed
- WinUI: Updated to Microsoft.WindowsAppSDK 1.0.0

## [5.0.0] - 2021-08-20

### Added
- Added ownerViewModel to IWindowManager functions

### Changed
- Reduced generic types for rule validation
- WinUI: Updated to WinUI 3.0
- Replaced IUIContext with IDispatcher
- Replaced ThreadOption in IEventAggregator with IDispatcher
- Changed IClose to be async

### Removed
- Removed IParent, IConductActiveItem and IScreen
- Removed dialogResult from TryClose()
- Removed coroutines
- Removed logging

## [4.0.0] - 2020-11-11

### Added
- Support for view model life cycles where async operations are happening
- Introduced WPF and WinUI namespaces
- Added IReadOnlyBindableCollection
- Added INotifyPropertyChanging

### Changed
- Merged IActivate and IDeactivate into IActivatable
- Replaced IServiceLocator by IServiceProvider

### Removed
- Removed BootstrapperBase and CaliburnApplication
- Removed conventions (NameBasedViewModelTypeResolver)
- Removed UIContext
- Removed ScreenHelper
- Removed PropertySupport

## [3.3.0] - 2019-10-21

### Changed
- Migrated from PCL to .NET Standard 2.0
- Changed minimum supported frameworks to NET 4.6.1 and UAP 10.0.16299
- Added .NET Core 3.0 WPF support

## [3.2.0] - 2018-10-26

### Added
- Added new FrameAdapter that attaches the framework to a Frame instance
- Added IPreserveState and INavigationAware interfaces for ViewModels
- Added SuspensionManager

### Changed
- Streamlined SimpleContainer API
- Changed SimpleContainer.GetInstance() to return null instead of throwing exception if not found
- Changed DelegateCommandBuilder to return concrete class instead of interface
- Changed NavigationService to be used by ViewModels only (framework uses FrameAdapter)

### Removed
- Removed Weakly
- Removed NavigationHelper
- Removed CallMethodAction and InvokeCommandAction

## [3.1.0] - 2018-01-14

### Changed
- Changed Validation to be reflection free
- Don't create new strings in validators (pre-format error messages)
- Removed usage of `Expression<Func<TProperty>>` to extract the property name
- Initial documentation update
- Optimized `EventAggregator` for async subscribers

### Added
- Added `BindableCommand` as base class of `DelegateCommand`
- UAP: Support `cal:Bind.CommandParameter` with `x:Bind`
- UAP: Support special value in `DelegateCommand.OnEvent`
- UAP: Enable `nameof()` usage with `NavigateHelper`

### Removed
- Removed support for static subscribers in `EventAggregator`

## [3.0.4] - 2017-11-19

### Fixed
- Fix null check (#74)

## [3.0.3] - 2017-11-19

### Changed
- Updated Weakly 2.7.0
- Reduced reflection
- Limited design time support
- Allow to override DelegateCommand's CoerceParameter function
- Support multiple properties in DelegateCommand
- Use string for View.Context
- Changed SimpleContainer.GetAllInstances() to return named instances too

### Added
- Added IViewModelTypeResolver with NameBasedViewModelTypeResolver (old behavior) and ViewModelTypeResolver (registration based)
- UAP: Added Caliburn.Xaml.UAP.rd.xml
- UAP: Added OnEvent() to IDelegateCommand for x:Bind support

### Removed
- IoC: Removed property injection
- UAP: Removed Behavior SDK

## [2.0.1] - 2017-04-29

### Fixed
- Fixed DelegateBuilder where no canExecute function was set

## [2.0.0] - 2017-04-28

### Changed
- Refactored DelegateCommand
- No Expression compilation
- Optimized StringLengthValidationRule
- Renamed EventAggregator SubsribeAsync() to Subscribe()

### Added
- Added DelegateCommandBuilder

## [1.8.1] - 2017-04-23

### Fixed
- Fixed EventAggregator

## [1.8.0] - 2017-04-13

### Changed
- Updated Weakly 2.5.0

### Removed
- Removed AsyncSubsystem

## [1.7.0] - 2017-04-11

### Changed
- View-Model first: Bind before setting content
- Changed EventAggregator to work without Reflection
- Removed WP8 and use NET451
- Hide UI scheduler
- WPF: Better fallback for active window
- WPF: Don't create Dispatcher upfront

### Added
- Added AsyncSubsystem
- Added DataTrigger

## [1.6.5] - 2016-12-12

### Added
- Added `Coroutine.OverrideCancel<TResult>()`

## [1.6.4] - 2016-10-20

### Added
- Added UIContext.VerifyAccess()
- Added ExecutionContextResolver
- Added an independent flag to ShowSettingsFlyout (win81)

### Changed
- Deal with unicode in names
- Changed handling of InitialDirectory in FileOpen and FileSave CoTasks (net45)

## [1.6.3] - 2016-09-09

### Changed
- Use ObserveException()

### Added
- Added UIContext.Run() overloads similar to Task.Run()

## [1.6.2] - 2016-08-26

### Fixed
- Bugfixes

## [1.6.1] - 2016-08-26

### Fixed
- Bugfixes

## [1.6.0] - 2016-08-23

### Added
- Async CanClose handling (`ICloseGuard` and `ICloseStrategy<T>`)
- Propagate exceptions from async methods

## [1.5.0] - 2016-08-09

### Changed
- Changed initialization of `UIContext`
- Changed initialization of `IoC`

### WPF
- Added common dialogs

## [1.4.0] - 2016-08-01

### Changed
- UIContext: Added `Initialize` overload with more parameters
- DelegateCommand: Replaced `Create` by `FromAction` and `FromFunc` methods
- ParameterBinder: Removed context from `CustomConverters`
- Replaced `AttachedCollection<T>` with `ParameterCollection`

### Removed
- Screen: Removed `IHaveDisplayName` (DisplayName property)
- Screen: Removed `IChild` (Parent property)
- Removed `IChild<TParent>` and `IParent<out T>`

## [1.3.0] - 2015-06-13

### Changed
- Optimized EventAggregator
- Removed obsolete methods

## [1.2.4] - 2015-06-11

### Fixed
- Bugfixes

## [1.2.3] - 2015-06-05

### Fixed
- Bugfixes

## [1.2.2] - 2015-05-27

### Fixed
- Bugfixes

## [1.2.1] - 2015-05-16

### Changed
- Removed Universal App PCL
- Updated Weakly

## [1.2.0] - 2015-05-14

### Added
- Added parameter support to `DelegateCommand`

## [1.1.0] - 2015-05-10

### Added
- Added support for special values with the `ISpecialValue` interface
- The value of objects that implement this interface will be resolved at runtime when used inside `CallMethodAction.Parameters` or `InvokeCommandAction.CommandParameter`

## [1.0.0] - 2015-05-08

### Added
- Initial release of Caliburn.Light where at least WPF can be considered stable

[Unreleased]: https://github.com/tibel/Caliburn.Light/compare/v6.1.0...HEAD
[6.1.0]: https://github.com/tibel/Caliburn.Light/compare/v6.0.0...v6.1.0
[6.0.0]: https://github.com/tibel/Caliburn.Light/compare/v5.3.0...v6.0.0
[5.3.0]: https://github.com/tibel/Caliburn.Light/compare/v5.2.1...v5.3.0
[5.2.1]: https://github.com/tibel/Caliburn.Light/compare/v5.2.0...v5.2.1
[5.2.0]: https://github.com/tibel/Caliburn.Light/compare/v5.1.0...v5.2.0
[5.1.0]: https://github.com/tibel/Caliburn.Light/compare/v5.0.0...v5.1.0
[5.0.0]: https://github.com/tibel/Caliburn.Light/compare/v4.0.0...v5.0.0
[4.0.0]: https://github.com/tibel/Caliburn.Light/compare/v3.3.0...v4.0.0
[3.3.0]: https://github.com/tibel/Caliburn.Light/compare/v3.2.0...v3.3.0
[3.2.0]: https://github.com/tibel/Caliburn.Light/compare/v3.1.0...v3.2.0
[3.1.0]: https://github.com/tibel/Caliburn.Light/compare/v3.0.4...v3.1.0
[3.0.4]: https://github.com/tibel/Caliburn.Light/compare/v3.0.3...v3.0.4
[3.0.3]: https://github.com/tibel/Caliburn.Light/compare/v2.0.1...v3.0.3
[2.0.1]: https://github.com/tibel/Caliburn.Light/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/tibel/Caliburn.Light/compare/v1.8.1...v2.0.0
[1.8.1]: https://github.com/tibel/Caliburn.Light/compare/v1.8.0...v1.8.1
[1.8.0]: https://github.com/tibel/Caliburn.Light/compare/v1.7.0...v1.8.0
[1.7.0]: https://github.com/tibel/Caliburn.Light/compare/v1.6.5...v1.7.0
[1.6.5]: https://github.com/tibel/Caliburn.Light/compare/v1.6.4...v1.6.5
[1.6.4]: https://github.com/tibel/Caliburn.Light/compare/v1.6.3...v1.6.4
[1.6.3]: https://github.com/tibel/Caliburn.Light/compare/v1.6.2...v1.6.3
[1.6.2]: https://github.com/tibel/Caliburn.Light/compare/v1.6.1...v1.6.2
[1.6.1]: https://github.com/tibel/Caliburn.Light/compare/v1.6.0...v1.6.1
[1.6.0]: https://github.com/tibel/Caliburn.Light/compare/v1.5.0...v1.6.0
[1.5.0]: https://github.com/tibel/Caliburn.Light/compare/v1.4.0...v1.5.0
[1.4.0]: https://github.com/tibel/Caliburn.Light/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/tibel/Caliburn.Light/compare/v1.2.4...v1.3.0
[1.2.4]: https://github.com/tibel/Caliburn.Light/compare/v1.2.3...v1.2.4
[1.2.3]: https://github.com/tibel/Caliburn.Light/compare/v1.2.2...v1.2.3
[1.2.2]: https://github.com/tibel/Caliburn.Light/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/tibel/Caliburn.Light/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/tibel/Caliburn.Light/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/tibel/Caliburn.Light/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/tibel/Caliburn.Light/releases/tag/v1.0.0
