﻿#region Dapplo 2016 - GNU Lesser General Public License

// Dapplo - building blocks for .NET applications
// Copyright (C) 2016 Dapplo
// 
// For more information see: http://dapplo.net/
// Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
// This file is part of Dapplo.CaliburnMicro
// 
// Dapplo.CaliburnMicro is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dapplo.CaliburnMicro is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have a copy of the GNU Lesser General Public License
// along with Dapplo.CaliburnMicro. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#endregion

#region Usings

using System.ComponentModel.Composition;
using Caliburn.Micro;
using Dapplo.CaliburnMicro.Demo.Languages;
using Dapplo.CaliburnMicro.Demo.UseCases.Configuration.ViewModels;
using Dapplo.CaliburnMicro.Extensions;
using Dapplo.CaliburnMicro.Menu;
using Dapplo.Log.Facade;
using Dapplo.Utils;
using MahApps.Metro.IconPacks;

#endregion

namespace Dapplo.CaliburnMicro.Demo.UseCases.ContextMenu
{
	/// <summary>
	/// This will add an extry which shows children to the context menu
	/// </summary>
	[Export("contextmenu", typeof(IMenuItem))]
	public sealed class WithChildrenMenuItem : MenuItem, IPartImportsSatisfiedNotification
	{
		private static readonly LogSource Log = new LogSource();

		[Import]
		public IWindowManager WindowManager { get; set; }

		[Import]
		public ConfigViewModel ConfigViewModel { get; set; }

		[Import]
		private IContextMenuTranslations ContextMenuTranslations { get; set; }

		public void OnImportsSatisfied()
		{
			UiContext.RunOn(() =>
			{
				Icon = new PackIconMaterial
				{
					Kind = PackIconMaterialKind.HumanChild
				};
				// automatically update the DisplayName
				this.BindDisplayName(ContextMenuTranslations, nameof(IContextMenuTranslations.WithChildren));
			});

			var menuItem = new MenuItem
			{
				Id = "1"
			};
			var observable = menuItem.BindDisplayName(ContextMenuTranslations, nameof(IContextMenuTranslations.One));
			ChildNodes.Add(menuItem);

			ChildNodes.Add(new MenuItem { IsSeparator = true });

			menuItem = new MenuItem
			{
				Id = "2"
			};
			menuItem.BindDisplayName(observable, nameof(IContextMenuTranslations.Two));
			ChildNodes.Add(menuItem);

			menuItem = new MenuItem
			{
				Id = "3"
			};
			menuItem.BindDisplayName(observable, nameof(IContextMenuTranslations.Three));
			ChildNodes.Add(menuItem);
		}

		public override void Click(IMenuItem clickedItem)
		{
			Log.Debug().WriteLine("child {0} clicked", clickedItem.Id);
		}
	}
}