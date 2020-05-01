using Microsoft.Win32;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LAS
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class LASWindow : Window
    {
        private DataTable dataTable, dataTable2;
        private string path = "";

        public LASWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //label.Content = dataTable.Rows[2].ItemArray.ToString();
        }

        private void dataGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers == ModifierKeys.Control) && (e.Key == Key.A))
                dataGrid.SelectAllCells();
            if ((e.KeyboardDevice.Modifiers == ModifierKeys.Control) && (e.Key == Key.V))
                Paste();
            if ((e.Key == Key.Enter) && (dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item) == dataTable.Rows.Count - 1))
                MenuItem_Click_PasteRow(sender, e);
            if (e.Key == Key.Delete)
                ClearSelected();
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers == ModifierKeys.Control) && (e.Key == Key.C))
            {
                if (Clipboard.ContainsText())
                    Clipboard.Clear();
            }

            if (((e.Key >= Key.D0) && (e.Key <= Key.D9)) || e.Key == Key.OemPeriod || e.Key == Key.Enter || e.Key == Key.Delete || ((e.Key >= Key.Prior) && (e.Key <= Key.Down))
            || e.Key == Key.Escape || e.Key == Key.Tab || ((e.KeyboardDevice.Modifiers == ModifierKeys.Control) && (e.Key == Key.V)) || e.Key == Key.Back
            || ((e.KeyboardDevice.Modifiers == ModifierKeys.Control) && (e.Key == Key.C)) || ((e.KeyboardDevice.Modifiers == ModifierKeys.Control) && (e.Key == Key.A)))
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void MenuItem_Click_Copy(object sender, RoutedEventArgs e)
        {
            ApplicationCommands.Copy.Execute(null, dataGrid);
        }

        private void MenuItem_Click_Paste(object sender, RoutedEventArgs e)
        {
            Paste();
        }

        private void MenuItem_Click_DeleteRow(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            int maxRows = dataTable.Rows.Count;
            int indexCurrentRow = dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item);

            if (indexCurrentRow < maxRows)
                dataTable.Rows[indexCurrentRow].Delete();
        }

        private void MenuItem_Click_PasteRow(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            int indexCurrentRow = dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item);
            var row = dataTable.NewRow();
            dataTable.Rows.InsertAt(row, indexCurrentRow + 1);
        }

        public void Paste()
        {
            if (dataTable == null)
                return;
            string textFromClipboard = Clipboard.GetText();
            int indexCurrentRow = dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item);
            int indexCurrentColumn = dataGrid.CurrentColumn.DisplayIndex;
            int maxColumns = dataTable.Columns.Count;
            int maxRows = dataTable.Rows.Count;

            string[] rowsFromClipboard = textFromClipboard.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] columnFromCurrRow = rowsFromClipboard[0].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if ((indexCurrentColumn + columnFromCurrRow.Length > maxColumns) || (indexCurrentRow + rowsFromClipboard.Length > maxRows))
            {
                MessageBox.Show("Данные выходят за пределы текущей таблицы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            for (int i = indexCurrentRow; i < rowsFromClipboard.Length + indexCurrentRow; i++)
            {
                columnFromCurrRow = rowsFromClipboard[i - indexCurrentRow].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int q = indexCurrentColumn; q < columnFromCurrRow.Length + indexCurrentColumn; q++)
                {
                    dataTable.Rows[i][q] = columnFromCurrRow[q - indexCurrentColumn];
                }
            }
        }

        private void MenuOpen(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                if (dataTable == null)
                {
                    LasTools lasTools = new LasTools();
                    dataTable = lasTools.LoadFromFile(path);
                    dataGrid.ItemsSource = dataTable.AsDataView();
                }
                else
                {
                    dataTable.Clear();
                    LasTools lasTools = new LasTools();
                    dataTable = lasTools.LoadFromFile(path);
                    dataGrid.ItemsSource = dataTable.AsDataView();
                }
            }
        }

        private void MenuMerge(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                if (dataTable == null)
                {
                    LasTools lasTools = new LasTools();
                    dataTable = lasTools.LoadFromFile(path);
                    dataGrid.ItemsSource = dataTable.AsDataView();
                }
                else
                {
                    LasTools lasTools = new LasTools();
                    dataTable2 = lasTools.LoadFromFile(path);
                    dataTable.Merge(dataTable2);
                    dataGrid.ItemsSource = dataTable.AsDataView();
                }
            }
        }

        private void MenuSave(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
                if (dataTable != null)
                {
                    LasTools lasTools = new LasTools();
                    lasTools.SaveFile(path, dataTable);
                }
            }
        }

        private void MenuQuit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void ClearSelected()
        {
            string[] nameColumn = new string[dataGrid.SelectedCells.Count];

            for (int i = 0; i < dataGrid.SelectedCells.Count; i++)
            {
                nameColumn[i] = dataGrid.SelectedCells[i].Column.Header.ToString();
            }

            int countColumnInSelect = nameColumn.Distinct().Count();
            int countRowInSelect = dataGrid.SelectedCells.Count / countColumnInSelect;
            int indexFirstRowInSelect = dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item);
            int indexFirstColumnInSelect = dataGrid.Columns.IndexOf(dataGrid.SelectedCells[0].Column);

            for (int i = 0; i < countRowInSelect; i++)
            {
                for (int j = 0; j < countColumnInSelect; j++)
                {
                    dataTable.Rows[i + indexFirstRowInSelect][j + indexFirstColumnInSelect] = "-999.25";
                }
            }
        }
    }
}
