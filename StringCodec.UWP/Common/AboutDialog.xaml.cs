﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace StringCodec.UWP.Common
{
    public sealed partial class AboutDialog : ContentDialog
    {
        private CanvasBitmap logoImage = null;

        public AboutDialog()
        {
            this.InitializeComponent();

            try
            {
                this.RequestedTheme = Settings.GetTheme();

                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                AboutTitle.Text = "AppName".T();
                AboutAuthorValue.Text = package.PublisherDisplayName;
                AboutVersionValue.Text = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                AboutDescriptionValue.Text = packageId.Name;
            }
            catch(Exception ex)
            {
                ex.Message.T().ShowMessage("ERROR".T());
            }
        }

        private void Dialog_Unloaded(object sender, RoutedEventArgs e)
        {
            if (AboutLogo is CanvasControl)
            {
                //AboutLogo.RemoveFromVisualTree();
                //AboutLogo = null;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void Logo_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(Task.Run(async () =>
            {
                try
                {
                    // Load the background image and create an image brush from it
                    //var asset = new Uri("ms-appx:///Assets/Square71x71Logo.scale-200.png");
                    Uri asset = new Uri("ms-appx:///Assets/AppLogo.png");
                    if (asset != null && asset is Uri)
                        logoImage = await CanvasBitmap.LoadAsync(sender, asset);
                    else logoImage = null;
                }
                catch (Exception ex)
                {
                    ex.Message.T().ShowMessage("ERROR".T());
                }
            }).AsAsyncAction());
        }

        private void Logo_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;

            if (logoImage is CanvasBitmap && session is CanvasDrawingSession)
            {
                try
                {
                    if (RequestedTheme == ElementTheme.Dark)
                    {
                        session.DrawImage(logoImage);
                    }
                    else
                    {
                        var invert = new InvertEffect() { Source = logoImage };
                        if (invert is InvertEffect)
                            session.DrawImage(invert);
                    }
                }
                catch (Exception ex)
                {
                    ex.Message.T().ShowMessage("ERROR".T());
                }
            }
        }

        private async void Contact_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = null;
            if (sender == AboutContactValue)
                uri = new Uri($@"mailto:{(sender as HyperlinkButton).Content}");
            else if(sender == AboutTwitterValue)
                uri = new Uri(@"https://twitter.com/netcharm");

            if(uri is Uri)
            {
                var options = new Windows.System.LauncherOptions();
                options.TreatAsUntrusted = true;
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(uri, options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                }
            }
        }

        private async void Site_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(@"https://github.com/netcharm");
            if(uri is Uri)
            {
                // Set the desired remaining view.
                var options = new Windows.System.LauncherOptions();
                options.TreatAsUntrusted = true;
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(uri, options);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                }
            }
        }
    }
}
