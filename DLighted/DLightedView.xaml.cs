/*
  Copyright © Iain McDonald 2011
  
  This file is part of D-Lighted.

	D-Lighted is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	D-Lighted is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with D-Lighted.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using LbF.Database;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System.Threading.Tasks;


namespace LbF.DLighted
{
	public partial class DLightedView : Window
	{
		public DLightedView()
		{
			InitializeComponent();
		}

		#region ViewModel Dependency Injection

		private DLightedViewModel viewModel;

		[Dependency]
		public DLightedViewModel ViewModel
		{
			set
			{
				viewModel = value;
				viewModel.UIDispatcher = this.Dispatcher;
				viewModel.View = this;
				this.DataContext = viewModel;
			}
		}

		#endregion

		#region Handle ListView double-clicks and <Enter> key presses

		private void ListElementItemEnter(object sender, MouseButtonEventArgs e)
		{
			UpdateTable(sender);
		}

		private void ListElementKeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			UpdateTable(sender);
		}

		private async void UpdateTable(object sender)
		{
			dynamic listItem = sender;
			string listItemString = listItem.Content;
			await Task.Factory.StartNew(() => this.viewModel.UpdateTable(listItemString));
		}

		#endregion

		#region Full Screen UI Code Behind

		private void GridKeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Escape)
				return;

			this.isFullScreen = true;
			AlternateFullScreen(sender, null);
		}

		private bool isFullScreen = false;
		private void AlternateFullScreen(object sender, RoutedEventArgs e)
		{
			this.WindowStyle = this.isFullScreen ? System.Windows.WindowStyle.SingleBorderWindow : System.Windows.WindowStyle.None;
			this.WindowState = this.isFullScreen ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;

			this.isFullScreen = !this.isFullScreen;
		}

		#endregion

		#region Colour Heading Text of Primary Key Columns

		internal static Style columnHeaderStyle;

		static DLightedView()
		{
			DLightedView.columnHeaderStyle = new Style(typeof(Control));
			DLightedView.columnHeaderStyle.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Color.FromRgb(255, 255, 128))));
			DLightedView.columnHeaderStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
			DLightedView.columnHeaderStyle.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Black));
			DLightedView.columnHeaderStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 1, 0)));
			DLightedView.columnHeaderStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4)));
		}

		private void AutoGeneratingColumns(object sender, EventArgs e)
		{
			DataGrid dataGrid = (DataGrid) sender;

			foreach (var primaryKeyColumn in this.viewModel.PrimaryKey)
				dataGrid.Columns[primaryKeyColumn.Ordinal].HeaderStyle = DLightedView.columnHeaderStyle;
		}

		#endregion

		#region Gratuitous Link to Blog

		private void HyperlinkRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		#endregion
	}
}
