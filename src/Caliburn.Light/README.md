# Caliburn.Light

[![NuGet](https://img.shields.io/nuget/v/Caliburn.Light.svg)](https://www.nuget.org/packages/Caliburn.Light/)

> ⚠️ **Deprecated:** This is a compatibility meta package that forwards to [Caliburn.Light.WPF](https://www.nuget.org/packages/Caliburn.Light.WPF/) and [Caliburn.Light.Coroutines](https://www.nuget.org/packages/Caliburn.Light.Coroutines/).
> Please migrate to the **Caliburn.Light.WPF** package directly.

## Migration

This package forwards to `Caliburn.Light.WPF` and `Caliburn.Light.Coroutines`, so existing applications using the legacy package can migrate without code changes.
Replace the package reference in your project file:

```diff
- <PackageReference Include="Caliburn.Light" Version="..." />
+ <PackageReference Include="Caliburn.Light.WPF" Version="..." />
```

## Documentation

For complete documentation, visit the [Caliburn.Light documentation](https://github.com/tibel/Caliburn.Light/tree/main/docs).

## License

Caliburn.Light is licensed under the [MIT license](https://github.com/tibel/Caliburn.Light/blob/main/LICENSE).
