using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace LAS
{
    class LasTools
    {
        public DataTable LoadFromFile(string path)
        {
            DataTable dataTable = new DataTable();
            DataColumn column = new DataColumn();
            DataRow row;
            StreamReader streamReader = new StreamReader(path);

            List<string> columnName = GetInfoAboutCurves(path);
            string stringFromFile;
            string[] arrayFromString;

            for (int i = 0; i < columnName.Count; i++)
            {
                string tmp;
                if (columnName[i].IndexOf(" ") >= 0)
                {
                    tmp = columnName[i].Remove(columnName[i].IndexOf(" "));
                }
                else
                {
                    tmp = columnName[i];
                }
                column.DataType = Type.GetType("System.String");
                dataTable.Columns.Add(tmp, typeof(String));
            }

            while (!streamReader.EndOfStream)
            {
                stringFromFile = streamReader.ReadLine();

                if (stringFromFile.StartsWith("~A"))
                {
                    while (!streamReader.EndOfStream)
                    {
                        stringFromFile = streamReader.ReadLine();
                        arrayFromString = stringFromFile.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        row = dataTable.NewRow();

                        for (int i = 0; i < arrayFromString.Length; i++)
                        {
                            row[i] = arrayFromString[i];
                        }

                        dataTable.Rows.Add(row);
                    }
                }
            }

            //dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["DEPTH"] };
            streamReader.Close();
            return dataTable;
        }

        public List<string> GetInfoAboutCurves(string path)
        {
            List<string> listCurves = new List<string> { };
            StreamReader streamReader = new StreamReader(path);

            while (!streamReader.EndOfStream)
            {
                string stringFromFile = streamReader.ReadLine();

                if (stringFromFile.StartsWith("~C"))
                {
                    stringFromFile = streamReader.ReadLine();

                    while (!stringFromFile.StartsWith("~"))
                    {
                        if (!stringFromFile.StartsWith("#"))
                        {
                            listCurves.Add(stringFromFile);
                            stringFromFile = streamReader.ReadLine();
                        }
                        else
                            stringFromFile = streamReader.ReadLine();
                    }
                }
            }

            streamReader.Close();
            return listCurves;
        }

        public List<string> GetHeader(string path)
        {
            List<string> listHeader = new List<string> { };
            StreamReader streamReader = new StreamReader(path);

            while (!streamReader.EndOfStream)
            {
                string stringFromFile = streamReader.ReadLine();
                listHeader.Add(stringFromFile);

                if (stringFromFile.StartsWith("~A"))
                {
                    streamReader.Close();
                    return listHeader;
                }
            }

            streamReader.Close();
            return listHeader;
        }

        public bool SaveFile(string path, DataTable dataTable)
        {
            StreamWriter streamWriter = new StreamWriter(path);
            List<string> listHeader = new List<string> { };

            listHeader.Add("~VERSION INFORMATION SECTION");
            listHeader.Add("VERS.           2.0     :   CWLS log ASCII Standard - VERSION 2.0");
            listHeader.Add("WRAP.NO      :   One line per depth step");
            listHeader.Add("#--------------------------------------------------");
            listHeader.Add("#-----------------------------------------------------------------------------");
            listHeader.Add("~CURVE INFORMATION SECTION");
            listHeader.Add("#MNEM.UNIT         API CODE      : CURVE  DESCRIPTION");
            listHeader.Add("#---- -----        -----------   --------------------------------");

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                listHeader.Add(dataTable.Columns[i].ColumnName);
            }

            listHeader.Add("#-----------------------------------------------------------------------------");
            listHeader.Add("~PARAMETER INFORMATION SECTION");
            listHeader.Add("#MNEM.UNIT                  VALUE                DESCRIPTION");
            listHeader.Add("#-----------------------------------------------------------------------------");
            listHeader.Add("~A");

            for (int i = 0; i < listHeader.Count; i++)
            {
                streamWriter.WriteLine(listHeader[i]);
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string stringForWrite = "";

                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    stringForWrite += "   " + dataTable.Rows[i][j].ToString() + "   ";
                }
                streamWriter.WriteLine(stringForWrite, true, Encoding.UTF8);
            }

            streamWriter.Close();
            return true;
        }
    }
}
