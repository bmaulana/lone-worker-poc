﻿

#pragma checksum "C:\Users\Jackys\Source\Repos\LoneWorkerPoC\lone-worker-poc2\LoneWorkerPoC\LoneWorkerPoC\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "09EA7FE1A20CA0A90AAE2B9958E05255"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LoneWorkerPoC
{
    partial class MainPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 108 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.NavigateToProfile;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 109 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.NavigateToNotifications;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 97 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.PanicClick;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 90 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.CheckInClick;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 91 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.RefreshClick;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 33 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ToggleClick;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


