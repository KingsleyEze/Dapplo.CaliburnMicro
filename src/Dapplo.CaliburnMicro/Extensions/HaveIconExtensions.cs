﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
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
using System.Windows;
using System.Windows.Media;

#endregion

namespace Dapplo.CaliburnMicro.Extensions
{
    /// <summary>
    ///     Extensions to simplify the usage of IHaveIcon
    /// </summary>
    public static class HaveIconExtensions
    {
        /// <summary>
        ///     Apply the specified Brush as Foreground for the Icons in the IEnumerable with IHaveIcon
        /// </summary>
        /// <param name="hasIcons">IEnumerable with IHaveIcon</param>
        /// <param name="foregroundBrush">Brush for the Foreground</param>
        public static void ApplyIconForegroundColor(this IEnumerable<IHaveIcon> hasIcons, Brush foregroundBrush)
        {
            foreach (var haveIcon in hasIcons)
            {
                haveIcon.ApplyIconForegroundColor(foregroundBrush);
            }
        }

        /// <summary>
        ///     Apply the specified Brush as Foreground for the icon of the IHaveIcon
        /// </summary>
        /// <param name="haveIcon">IHaveIcon</param>
        /// <param name="foregroundBrush">Brush for the Foreground</param>
        public static void ApplyIconForegroundColor(this IHaveIcon haveIcon, Brush foregroundBrush)
        {
            if (haveIcon == null)
            {
                throw new ArgumentNullException(nameof(haveIcon));
            }
            if (haveIcon.Icon != null)
            {
                haveIcon.Icon.Foreground = foregroundBrush;
            }
        }

        /// <summary>
        ///     Apply the specified Thickness as margin to the Icons in the IEnumerable with IHaveIcon
        /// </summary>
        /// <param name="hasIcons">IEnumerable with IHaveIcon</param>
        /// <param name="thickness">Thickness for the marging</param>
        public static void ApplyIconMargin(this IEnumerable<IHaveIcon> hasIcons, Thickness thickness)
        {
            foreach (var menuItem in hasIcons)
            {
                menuItem.ApplyIconMargin(thickness);
            }
        }

        /// <summary>
        ///     Apply the specified Thickness as margin to the Icon in the IHaveIcon
        /// </summary>
        /// <param name="haveIcon">IHaveIcon</param>
        /// <param name="thickness">Thickness for the marging</param>
        public static void ApplyIconMargin(this IHaveIcon haveIcon, Thickness thickness)
        {
            if (haveIcon == null)
            {
                throw new ArgumentNullException(nameof(haveIcon));
            }
            if (haveIcon.Icon != null)
            {
                haveIcon.Icon.Margin = thickness;
            }
        }
    }
}