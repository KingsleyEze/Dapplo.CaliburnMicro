﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.CaliburnMicro
// 
//  Dapplo.CaliburnMicro is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.CaliburnMicro is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.CaliburnMicro. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Dapplo.Addons;
using Dapplo.Log;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Metro.Configuration;
using MahApps.Metro;

#endregion

namespace Dapplo.CaliburnMicro.Metro
{
    /// <summary>
    ///     This (slightly modified) comes from
    ///     <a href="https://github.com/ziyasal/Caliburn.Metro/blob/master/Caliburn.Metro.Core/MetroWindowManager.cs">here</a>
    ///     and providers a Caliburn.Micro IWindowManager implementation. The Dapplo.CaliburnMicro.CaliburnMicroBootstrapper
    ///     will
    ///     take care of taking this (if available) and the MetroWindowManager will take care of instanciating a MetroWindow.
    ///     Note: Currently there is no support for the DialogCoordinator yet..
    ///     For more information see <a href="https://gist.github.com/ButchersBoy/4a7272f3ac104c5b1a54">here</a> and
    ///     <a href="https://dragablz.net/2015/05/29/using-mahapps-dialog-boxes-in-a-mvvm-setup/">here</a>
    /// </summary>
    public sealed class MetroWindowManager : DapploWindowManager
    {
        private readonly IResourceProvider _resourceProvider;
        private static readonly LogSource Log = new LogSource();

        private static readonly string[] Styles =
        {
            "Controls", "Fonts", "Controls.AnimatedSingleRowTabControl"
        };

        private readonly IMetroUiConfiguration _metroUiConfiguration;

        /// <summary>
        /// Default constructor taking care of initialization
        /// </summary>
        public MetroWindowManager(
            IEnumerable<IConfigureWindowViews> configureWindows,
            IEnumerable<IConfigureDialogViews> configureDialogs,
            IResourceProvider resourceProvider,
            IMetroUiConfiguration metroUiConfiguration,
            IUiConfiguration uiConfiguration = null
        ) : base(configureWindows, configureDialogs, uiConfiguration)
        {
            _metroUiConfiguration = metroUiConfiguration;
            _resourceProvider = resourceProvider;
            foreach (var style in Styles)
            {
                AddMahappsStyle(style);
            }
        }

        /// <summary>
        ///     Add a ResourceDictionary for the specified MahApps style
        ///     The Uri to the source is created by CreateMahappStyleUri
        /// </summary>
        /// <param name="style">
        ///     Style name, this is actually what is added behind
        ///     pack://application:,,,/MahApps.Metro;component/Styles/ (and .xaml is added)
        /// </param>
        public void AddMahappsStyle(string style)
        {
            var packUri = CreateMahappStyleUri(style);
            if (!_resourceProvider.EmbeddedResourceExists(packUri))
            {
                Log.Warn().WriteLine("Style {0} might not be available as {1}.", style, packUri);
            }
            AddResourceDictionary(packUri);
        }

        /// <summary>
        ///     Add a single ResourceDictionary for the supplied source
        ///     An example would be /Resources/Icons.xaml (which is in MahApps.Metro.Resources)
        /// </summary>
        /// <param name="source">Uri, e.g. /Resources/Icons.xaml or </param>
        public void AddResourceDictionary(Uri source)
        {
            if (Application.Current.Resources.MergedDictionaries.Any(x => x.Source == source))
            {
                return;
            }

            var resourceDictionary = new ResourceDictionary
            {
                Source = source
            };
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        /// <summary>
        ///     Remove all ResourceDictionaries for the specified MahApps style
        ///     The Uri to the source is created by CreateMahappStyleUri
        /// </summary>
        /// <param name="style">string</param>
        public void RemoveMahappsStyle(string style)
        {
            var mahappsStyleUri = CreateMahappStyleUri(style);
            foreach (var resourceDirectory in Application.Current.Resources.MergedDictionaries.ToList())
            {
                if (resourceDirectory.Source == mahappsStyleUri)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(resourceDirectory);
                }
            }
        }

        /// <summary>
        ///     Change the current theme
        /// </summary>
        /// <param name="theme">Themes</param>
        /// <param name="themeAccent">ThemeAccents</param>
        public void ChangeTheme(Themes theme, ThemeAccents themeAccent)
        {
            // Remove current
            RemoveMahappsStyle($"Themes/{_metroUiConfiguration.Theme}.{_metroUiConfiguration.ThemeAccent}");

            // Change the settings
            _metroUiConfiguration.Theme = theme;
            _metroUiConfiguration.ThemeAccent = themeAccent;

            // Apply the new
            var themeName = $"{_metroUiConfiguration.Theme}.{_metroUiConfiguration.ThemeAccent}";
            var themeString = $"Themes/{themeName}";
            AddMahappsStyle(themeString);

            ThemeManager.ChangeTheme(Application.Current, themeName);
        }

        /// <summary>
        ///     Create a MapApps Uri for the supplied style
        /// </summary>
        /// <param name="style">e.g. Fonts or Controls</param>
        /// <returns></returns>
        public static Uri CreateMahappStyleUri(string style)
        {
            return new Uri($@"pack://application:,,,/MahApps.Metro;component/Styles/{style}.xaml", UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        ///     Create a MetroWindow
        /// </summary>
        /// <param name="model">the model as object</param>
        /// <param name="view">the view as object</param>
        /// <param name="isDialog">Is this for a dialog?</param>
        /// <returns></returns>
        protected override Window CreateCustomWindow(object model, object view, bool isDialog)
        {
            var result = view as Window ?? new MetroWindow
            {
                Content = view,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            bool isMetroWindow = result is MetroWindow;
            bool isWindow = view is Window;
            if (isMetroWindow || !isWindow)
            {
                result.SetResourceReference(Control.BorderBrushProperty, "AccentColorBrush");
                result.SetValue(Control.BorderThicknessProperty, new Thickness(1));
            }
            // Allow dialogs
            if (isDialog)
            {
                result.SetValue(DialogParticipation.RegisterProperty, model);
            }
            result.SetValue(View.IsGeneratedProperty, true);

            return result;
        }
    }
}