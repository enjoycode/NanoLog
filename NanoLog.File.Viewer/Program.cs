using NanoLog.File.Viewer;
using Terminal.Gui;

Application.Init();
ConfigurationManager.Themes!.Theme = "Light";
ConfigurationManager.Apply();
Application.Run<MainView>().Dispose();
Application.Shutdown();