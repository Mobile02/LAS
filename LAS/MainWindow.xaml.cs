using Microsoft.Win32;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LAS
{
    public partial class LASWindow : Window
    {
        private DataTable dataTable, dataTable2;
        private string path = "";
        private bool edit = false;

        public LASWindow()
        {
            InitializeComponent();
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
            edit = true;
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
            edit = true;
        }

        private void MenuItem_Click_DeleteRow(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            try
            {
                string[] nameColumn = new string[dataGrid.SelectedCells.Count];

                for (int i = 0; i < dataGrid.SelectedCells.Count; i++)
                {
                    nameColumn[i] = dataGrid.SelectedCells[i].Column.Header.ToString();
                }

                int countColumnInSelect = nameColumn.Distinct().Count();
                int countRowInSelect = dataGrid.SelectedCells.Count / countColumnInSelect;
                int indexFirstRowInSelect = dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item);

                for (int i = 0; i < countRowInSelect; i++)
                {
                    int indexCurrentRow = dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item);
                    dataTable.Rows[indexCurrentRow].Delete();
                }
                
                edit = true;
            }
            catch
            {
                MessageBox.Show("Не выделена ни одна строка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_Click_PasteRow(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            int indexCurrentRow = dataGrid.Items.IndexOf(dataGrid.SelectedCells[0].Item);
            var row = dataTable.NewRow();
            dataTable.Rows.InsertAt(row, indexCurrentRow + 1);
            edit = true;
        }

        private void MenuItem_Click_DeleteColumn(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            if (dataGrid.CurrentColumn == null)
                return;
            dataTable.Columns.RemoveAt(dataGrid.CurrentColumn.DisplayIndex);
            dataGrid.ItemsSource = dataTable.AsDataView();
            edit = true;
        }

        private void MenuItem_Click_ClearSelected(object sender, RoutedEventArgs e)
        {
            if (dataTable != null)
                ClearSelected();
        }

        private void MenuOpen(object sender, RoutedEventArgs e)
        {
            OpenOrMergeFile(false);
        }

        private void MenuMerge(object sender, RoutedEventArgs e)
        {
            OpenOrMergeFile(true);
        }

        private void MenuSave(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            SaveFile();
            edit = false;
        }

        private void MenuClose(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
                return;
            if (edit)
                if (MessageBox.Show("Вы не сохранили изменения, продолжить без сохранения?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            dataTable.Clear();
            dataTable = null;
            edit = false;
        }

        private void MenuQuit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void WindowLAS_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (edit)
            {
                if (MessageBox.Show("Вы не сохранили изменения, продолжить без сохранения?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void Paste()
        {
            string textFromClipboard = Clipboard.GetText();

            if (dataTable == null || textFromClipboard == null || textFromClipboard == "")
                return;
            
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
            edit = true;
        }

        private void ClearSelected()
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
            edit = true;
        }

        private void OpenOrMergeFile(bool merge)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "LAS файлы (*.las)|*.las|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                LasTools lasTools = new LasTools(path);
                lasTools.LoadFromFile();

                if (!merge)
                {
                    if (dataTable != null)
                        dataTable.Clear();

                    dataTable = lasTools.DataTable;
                }
                else
                {
                    if (dataTable == null)
                    {
                        dataTable = lasTools.DataTable;
                    }
                    else
                    {
                        dataTable2 = lasTools.DataTable;
                        dataTable.Merge(dataTable2);
                        edit = true;
                    }
                }

                dataGrid.ItemsSource = dataTable.AsDataView();
                WindowLAS.Width = 70 * dataGrid.Columns.Count + 54;
            }
        }

        private void SaveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "LAS файлы (*.las)|*.las|Все файлы (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
                if (dataTable != null)
                {
                    LasTools lasTools = new LasTools(path);
                    lasTools.DataTable = dataTable;
                    lasTools.SaveFile();
                    MessageBox.Show("Сохранено!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
