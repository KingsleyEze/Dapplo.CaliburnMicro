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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Dapplo.Addons;
using Dapplo.Log.Facade;
using Dapplo.Utils.Resolving;

#endregion

namespace Dapplo.CaliburnMicro
{
	/// <summary>
	///     An implementation of the Caliburn Micro Bootstrapper which is started from the Dapplo ApplicationBootstrapper (MEF)
	///     and uses this.
	/// </summary>
	[ShutdownAction(ShutdownOrder = (int) CaliburnStartOrder.Bootstrapper)]
	[Export]
	public class CaliburnMicroBootstrapper : BootstrapperBase, IShutdownAction
	{
		private static readonly LogSource Log = new LogSource();

		[Import]
		private IServiceExporter ServiceExporter { get; set; }

		[Import]
		private IServiceLocator ServiceLocator { get; set; }

		[Import]
		private IServiceRepository ServiceRepository { get; set; }

		/// <summary>
		///     Shutdown Caliburn
		/// </summary>
		/// <param name="token">CancellationToken</param>
		/// <returns>Task</returns>
		public async Task ShutdownAsync(CancellationToken token = default(CancellationToken))
		{
			Log.Debug().WriteLine("Starting shutdown");
			await Execute.OnUIThreadAsync(() =>
			{
				OnExit(this, new EventArgs());
			}).ConfigureAwait(false);
			Log.Debug().WriteLine("finished shutdown");
		}

		/// <summary>
		///     Fill imports of the supplied instance
		/// </summary>
		/// <param name="instance">some object to fill</param>
		protected override void BuildUp(object instance)
		{
			ServiceLocator.FillImports(instance);
		}

		/// <summary>
		/// Add logic to find the base viewtype if the default locator can't find a view.
		/// </summary>
		private void ConfigureViewLocator()
		{
			var defaultLocator = ViewLocator.LocateTypeForModelType;
			ViewLocator.LocateTypeForModelType = (modelType, displayLocation, context) =>
			{
				var viewType = defaultLocator(modelType, displayLocation, context);
				bool initialViewFound = viewType != null;

				if (!initialViewFound)
				{
					Log.Verbose().WriteLine("No view for {0}, looking into base types.", modelType);
					var currentModelType = modelType;
					while (viewType == null && currentModelType != null && currentModelType != typeof(object))
					{
						currentModelType = currentModelType.BaseType;
						viewType = defaultLocator(currentModelType, displayLocation, context);
					}
					if (viewType != null)
					{
						Log.Verbose().WriteLine("Found view for {0} in base type {1}, the view is {2}", modelType, currentModelType, viewType);
					}
				}

				return viewType;
			};
		}

		/// <summary>
		///     Configure the Dapplo.Addon.Bootstrapper with the AssemblySource.Instance values
		/// </summary>
		protected override void Configure()
		{
			LogManager.GetLog = type => new CaliburnLogger(type);

			foreach (var assembly in AssemblySource.Instance)
			{
				ServiceRepository.Add(assembly);
			}

			ConfigureViewLocator();

			// Test if there is a IWindowManager available, if not use the default
			var windowManagers = ServiceLocator.GetExports<IWindowManager>();
			if (!windowManagers.Any())
			{
				ServiceExporter.Export<IWindowManager>(new WindowManager());
			}

			// Test if there is a IEventAggregator available, if not use the default
			var eventAggregators = ServiceLocator.GetExports<IEventAggregator>();
			if (!eventAggregators.Any())
			{
				ServiceExporter.Export<IEventAggregator>(new EventAggregator());
			}

			// TODO: Documentation
			// This make it possible to pass the data-context of the originally clicked object in the Message.Attach event bubbling.
			// E.G. the parent Menu-Item Click will get the Child MenuItem that was actually clicked.
			MessageBinder.SpecialValues.Add("$originalDataContext", context => {
				var routedEventArgs = context.EventArgs as RoutedEventArgs;
				var frameworkElement = routedEventArgs?.OriginalSource as FrameworkElement;
				return frameworkElement?.DataContext;
			});
		}

		/// <summary>
		///     Return all instances of a certain service type
		/// </summary>
		/// <param name="serviceType"></param>
		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return ServiceLocator.GetExports(serviceType).Select(x => x.Value);
		}

		/// <summary>
		///     Locate an instance of a service, used in Caliburn.
		/// </summary>
		/// <param name="serviceType"></param>
		/// <param name="key"></param>
		/// <returns>instance</returns>
		protected override object GetInstance(Type serviceType, string key)
		{
			var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
			return ServiceLocator.GetExport(serviceType, contract);
		}

		/// <summary>
		///     This is the startup of the Caliburn bootstrapper, here the only implementation of IShell is displayed as root view
		/// </summary>
		/// <param name="sender">object, as it's called internally this is actually null</param>
		/// <param name="e">StartupEventArgs, as it's called internally this is actually null</param>
		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			// Call the base, this actually currently does nothing but who knows what is added later.
			base.OnStartup(sender, e);

			// Throw exception when no IShell export is found
			var shells = ServiceLocator.GetExports<IShell>();
			if (shells.Any())
			{
				// Display the IShell viewmodel
				DisplayRootViewFor<IShell>();
			}
			else
			{
				Log.Info().WriteLine("No IShell export found, if you want to have an initial window make sure you exported your ViewModel with [Export(typeof(IShell))]");
			}
		}

		/// <summary>
		///     Return all assemblies that the Dapplo Bootstrapper knows of
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Assembly> SelectAssemblies()
		{
			return AssemblyResolver.AssemblyCache;
		}
	}
}