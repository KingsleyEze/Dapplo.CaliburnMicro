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

using Application.Demo.Languages;
using Application.Demo.Shared;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Extensions;

#endregion

namespace Application.Demo.UseCases.Configuration.ViewModels
{
    /// <summary>
    /// This represents a node in the config
    /// </summary>
    public sealed class UiConfigNodeViewModel : ConfigNode
    {
        public IDemoConfigTranslations ConfigTranslations { get; }

        public UiConfigNodeViewModel(IDemoConfigTranslations configTranslations)
        {
            ConfigTranslations = configTranslations;

            // automatically update the DisplayName
            ConfigTranslations.CreateDisplayNameBinding(this, nameof(IDemoConfigTranslations.Ui));

            // automatically update the DisplayName
            CanActivate = false;
            Id = nameof(ConfigIds.Ui);
        }
    }
}