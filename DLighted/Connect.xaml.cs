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
using System.Windows;
using System.Windows.Input;

namespace LbF.DLighted
{
	public partial class Connect : Window
	{
		private DLightedViewModel viewModel;

		public Connect(DependencyObject viewModel, string windowTitle)
		{
			InitializeComponent();
			this.viewModel = (DLightedViewModel) viewModel;
			this.DataContext = this.viewModel;
			this.Title = windowTitle;
		}

		#region Handle <Enter> Key Presses

		private void TextBoxKeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			CloseDialog();
		}

		private void ConnectClicked(object sender, RoutedEventArgs e)
		{
			CloseDialog();
		}

		private void CloseDialog()
		{
			this.DialogResult = true;
			this.Close();
		}

		#endregion

		#region Update Password in the ViewModel

		private void PasswordChanged(object sender, RoutedEventArgs e)
		{
			dynamic passwordBox = sender;
			this.viewModel.Password = passwordBox.Password;
		}

		#endregion

		#region When entering a TextBox, select all text

		private void TextBoxGotFocus(object sender, RoutedEventArgs e)
		{
			dynamic textBox = sender;
			textBox.SelectAll();
		}

		#endregion
	}
}
