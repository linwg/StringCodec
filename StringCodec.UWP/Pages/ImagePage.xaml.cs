﻿using StringCodec.UWP.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace StringCodec.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ImagePage : Page
    {
        private bool CURRENT_PREFIX = true;
        private bool CURRENT_LINEBREAK = false;
        private string CURRENT_FORMAT = ".png";

        static private string text_src = string.Empty;
        static public string Text
        {
            get { return text_src; }
            set { text_src = value; }
        }

        public ImagePage()
        {
            this.InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Enabled;

            optPrefix.IsChecked = CURRENT_PREFIX;
            optLinbeBreak.IsChecked = CURRENT_LINEBREAK;

            optFmtPng.IsChecked = true;

            //if (!string.IsNullOrEmpty(text_src)) edBase64.Text = text_src;

            #region Add small image to Image control for dragdrop target
            if (imgBase64.Source == null)
            {
                var wb = new WriteableBitmap(1, 1);
                imgBase64.Stretch = Stretch.Uniform;
                imgBase64.Source = wb;
            }
            #endregion

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var data = e.Parameter;
                if (data is string)
                {
                    imgBase64.Stretch = Stretch.Uniform;
                    edBase64.Text = data.ToString();
                    imgBase64.Source = await edBase64.Text.Decoder();
                }
                else if (data is WriteableBitmap)
                {
                    imgBase64.Source = data as WriteableBitmap;
                    if (CURRENT_LINEBREAK) edBase64.TextWrapping = TextWrapping.NoWrap;
                    else edBase64.TextWrapping = TextWrapping.Wrap;
                    var wb = await imgBase64.ToWriteableBitmmap();
                    edBase64.Text = await wb.ToBase64(CURRENT_FORMAT, CURRENT_PREFIX, CURRENT_LINEBREAK);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void edBase64_TextChanged(object sender, TextChangedEventArgs e)
        {
            //text_src = edBase64.Text;
        }

        private void OptFmt_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem[] opts = new ToggleMenuFlyoutItem[] { optFmtBmp, optFmtGif, optFmtJpg, optFmtPng, optFmtTif };

            var btn = sender as ToggleMenuFlyoutItem;
            var FMT_NAME = btn.Name.Substring(btn.Name.Length - 3).ToLower();

            foreach (ToggleMenuFlyoutItem opt in opts)
            {
                if (string.Equals(opt.Name, btn.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    opt.IsChecked = true;
                }
                else opt.IsChecked = false;
            }
            CURRENT_FORMAT = $".{FMT_NAME}";
        }

        private void Opt_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleMenuFlyoutItem;
            var C_NAME = btn.Name;

            switch (C_NAME)
            {
                case "optPrefix":
                    CURRENT_PREFIX = btn.IsChecked;
                    break;
                case "optLinbeBreak":
                    CURRENT_LINEBREAK = btn.IsChecked;
                    if (CURRENT_LINEBREAK) edBase64.TextWrapping = TextWrapping.NoWrap;
                    else edBase64.TextWrapping = TextWrapping.Wrap;
                    break;
                default:
                    break;
            }
        }

        private void QRCode_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnImageQRCode)
            {
                if (imgBase64.Source == null) return;
                Frame.Navigate(typeof(QRCodePage), imgBase64.Source as WriteableBitmap);
            }
            else if (sender == btnImageOneD)
            {
                if (imgBase64.Source == null) return;
                Frame.Navigate(typeof(CommonOneDPage), imgBase64.Source as WriteableBitmap);
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            switch (btn.Name)
            {
                case "btnEncode":
                    edBase64.TextWrapping = TextWrapping.NoWrap;
                    var wb = await imgBase64.ToWriteableBitmmap();
                    //edBase64.Text = await wb.ToBase64(CURRENT_FORMAT, CURRENT_PREFIX, CURRENT_LINEBREAK);
                    string b64 = await wb.ToBase64(CURRENT_FORMAT, CURRENT_PREFIX, CURRENT_LINEBREAK);
                    edBase64.Text = b64;
                    if (CURRENT_LINEBREAK) edBase64.TextWrapping = TextWrapping.NoWrap;
                    else edBase64.TextWrapping = TextWrapping.Wrap;

                    //
                    // Maybe TextBox bug: If lines > 3500, the text maybe displayed as white but infact
                    // the content is right, you can select & copy. it's ok, but only display white
                    // 
                    break;
                case "btnDecode":
                    //imgBase64.Source = await TextCodecs.Decode(edBase64.Text);
                    //var bmp = await TextCodecs.Decode(edBase64.Text);
                    //if (bmp.PixelWidth >= imgBase64.RenderSize.Width || bmp.PixelHeight >= imgBase64.RenderSize.Height) imgBase64.Stretch = Stretch.Uniform;
                    //else imgBase64.Stretch = Stretch.None;
                    imgBase64.Source = await edBase64.Text.Decoder();
                    break;
                case "btnCopy":
                    //Utils.SetClipboard(edBase64.Text);
                    Utils.SetClipboard(imgBase64, -1);
                    break;
                case "btnPaste":
                    edBase64.Text = await Utils.GetClipboard(edBase64.Text, imgBase64);
                    break;
                case "btnSave":
                    await Utils.ShowSaveDialog(imgBase64);
                    break;
                case "btnShare":
                    await Utils.Share(imgBase64.Source as WriteableBitmap);
                    break;
                default:
                    break;
            }
        }

        #region Drag/Drop routines
        private bool canDrop = true;
        private async void OnDragEnter(object sender, DragEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("drag enter.." + DateTime.Now);
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var deferral = e.GetDeferral(); // since the next line has 'await' we need to defer event processing while we wait
                try
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        canDrop = false;
                        var item = items[0] as StorageFile;
                        string filename = item.Name;
                        string extension = item.FileType.ToLower();
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"Drag count:{items.Count}, {filename}");
#endif
                        if (sender == imgBase64)
                        {
                            if (Utils.image_ext.Contains(extension))
                            {
                                canDrop = true;
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("Drag image to Image Control");
#endif
                            }
                        }
                        else if(sender == edBase64)
                        {
                            if (Utils.text_ext.Contains(extension))
                            {
                                canDrop = true;
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("Drag text to TextBox Control");
#endif
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                deferral.Complete();
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            //if (canDrop && !e.Handled)
            //{
            //    { e.AcceptedOperation = DataPackageOperation.Copy; }
            //    System.Diagnostics.Debug.WriteLine("drag ok");
            //}
            //return;

            if (sender == imgBase64)
            {
                if (e.DataView.Contains(StandardDataFormats.WebLink))
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                }
                else if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    if (canDrop) e.AcceptedOperation = DataPackageOperation.Copy;
                }
            }
            else if (sender == edBase64)
            {
                if (e.DataView.Contains(StandardDataFormats.Text))
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                }
                else if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    if (canDrop) e.AcceptedOperation = DataPackageOperation.Copy;
                }
            }
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            // 需要异步拖放时记得获取Deferral对象
            //var def = e.GetDeferral();
            if (sender == imgBase64)
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        var storageFile = items[0] as StorageFile;
                        if (Utils.image_ext.Contains(storageFile.FileType.ToLower()))
                        {
                            var bitmapImage = new WriteableBitmap(1, 1);

                            if (e.DataView.Contains(StandardDataFormats.WebLink))
                            {
                                var url = await e.DataView.GetWebLinkAsync();
                                await bitmapImage.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(url).OpenReadAsync());
                            }
                            else if (e.DataView.Contains(StandardDataFormats.Text))
                            {
                                //var content = await e.DataView.GetHtmlFormatAsync();
                                var content = await e.DataView.GetTextAsync();
                                if (content.Length > 0)
                                {
                                    await bitmapImage.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(new Uri(content)).OpenReadAsync());
                                }
                            }
                            else
                            {
                                await bitmapImage.SetSourceAsync(await storageFile.OpenReadAsync());
                            }

                            byte[] arr = WindowsRuntimeBufferExtensions.ToArray(bitmapImage.PixelBuffer, 0, (int)bitmapImage.PixelBuffer.Length);
                            imgBase64.Source = bitmapImage;
                        }
                    }
                }
                else if (e.DataView.Contains(StandardDataFormats.WebLink))
                {
                    var uri = await e.DataView.GetWebLinkAsync();

                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                    //dp.SetBitmap(RandomAccessStreamReference.CreateFromUri(uri));

                    var bitmapImage = new WriteableBitmap(1, 1);
                    await bitmapImage.SetSourceAsync(await file.OpenAsync(FileAccessMode.Read));
                    // Set the image on the main page to the dropped image
                    //if (bitmapImage.PixelWidth >= imgBase64.RenderSize.Width || bitmapImage.PixelHeight >= imgBase64.RenderSize.Height)
                    //    imgBase64.Stretch = Stretch.Uniform;
                    //else imgBase64.Stretch = Stretch.None;
                    byte[] arr = WindowsRuntimeBufferExtensions.ToArray(bitmapImage.PixelBuffer, 0, (int)bitmapImage.PixelBuffer.Length);
                    imgBase64.Source = bitmapImage;
                }
            }
            else if (sender == edBase64)
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        var storageFile = items[0] as StorageFile;
                        if (Utils.url_ext.Contains(storageFile.FileType.ToLower()))
                        {
                            if (e.DataView.Contains(StandardDataFormats.WebLink))
                            {
                                var url = await e.DataView.GetWebLinkAsync();
                                if (url.IsAbsoluteUri)
                                {
                                    edBase64.Text = url.ToString();
                                }
                                else if (url.IsFile || url.IsUnc)
                                {
                                    edBase64.Text = await FileIO.ReadTextAsync(storageFile);
                                }
                            }
                            else if (e.DataView.Contains(StandardDataFormats.Text))
                            {
                                //var content = await e.DataView.GetHtmlFormatAsync();
                                var content = await e.DataView.GetTextAsync();
                                if (content.Length > 0)
                                {
                                    edBase64.Text = content;
                                }
                            }
                        }
                        else if (Utils.text_ext.Contains(storageFile.FileType.ToLower()))
                            edBase64.Text = await FileIO.ReadTextAsync(storageFile);
                    }
                }
                else if (e.DataView.Contains(StandardDataFormats.WebLink))
                {
                    var uri = await e.DataView.GetWebLinkAsync();

                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                    if (!file.FileType.ToLower().Equals(".txt")) return;
                    edBase64.Text = await FileIO.ReadTextAsync(file);
                }
                else if (e.DataView.Contains(StandardDataFormats.Text))
                {
                    var content = await e.DataView.GetTextAsync();
                    if (content.Length > 0)
                    {
                        edBase64.Text = content;
                    }
                }
            }
            //def.Complete();
        }
        #endregion

    }
}
