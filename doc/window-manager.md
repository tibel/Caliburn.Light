---
layout: page
title: The Window Manager
---

It is quite hard to write documentation for the Window Manager, as there is not one Window Manager but there is one per platform (Silverlight, Phone, WPF and WinRT). To make it even more complex: these Windows Managers have different interfaces and features. 

### Silverlight

A Silverlight application consists of one root visual only. The Window Manager is used to show dialogs, popups and toast notifications only.

### WPF

The Window Manager is used for every Window that is shown (modal and non-modal).

### Windows Phone

Windows Phone uses the View-First approach to show the applications pages. The Window Manager is used to show dialogs and popups on top of a page only.

### WinRT

A Windows Store application consists of multiple pages (similar to Windows Phone). The Window Manager is used to show the Settings Flyout only.