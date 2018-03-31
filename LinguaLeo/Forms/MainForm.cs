﻿using System;
using LinguaLeo.Logs;
using LinguaLeo.Api;
using LinguaLeo.Serialization;
using System.Windows.Forms;
using LinguaLeo.Reader;
using System.Collections.Generic;
using System.Threading;

namespace LinguaLeo.Forms
{
    public partial class MainForm : Form
    {
        API api;
        Logger logger;
        List<Word> words;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Log_Click(null, null);
            logger = new Logger(this.LoggerBox);
            logger.Clear();
        }

        private void Auth_Click(object sender, EventArgs e)
        {
            api = new API(Login.Text, Password.Text);
            logger.Add(Serrialize.Auth(api.Auth()));
        }

        private void Log_Click(object sender, EventArgs e)
        {
            this.Width -= this.Width == 619 ? this.LoggerBox.Width : -this.LoggerBox.Width;
            this.LoggerBox.Visible = this.LoggerBox.Visible ? false : true;
            this.Login.Focus();
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "All Files (*.*)|*.*|" +
                        "Text Files (.txt)|*.txt|" +
                        "Excel Book 97-2003  (.xls)|*.xls|" +
                        "Excel Book (.xlsx)|*.xlsx";
            fd.Multiselect = false;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                switch (fd.FileName.Substring(fd.FileName.LastIndexOf('.')))
                {
                    case ".txt":
                        words = new NotepadReader(fd.InitialDirectory + fd.FileName, this.progressBar1).Read();
                        break;
                    case ".xls":
                    case ".xlsx":
                        words = new ExcelReader(fd.InitialDirectory + fd.FileName, this.progressBar1).Read();
                        break;
                    default:
                        words = new List<Word>();
                        break;
                }

                this.WordsCounter.Text = words.Count.ToString();
                this.progressBar1.Maximum = Convert.ToInt32(this.WordsCounter.Text);
                this.progressBar1.Value = 0;
            }
        }

        private void ImportWords_Click(object sender, EventArgs e)
        {
            if (!Serrialize.isAuthed)
                Auth_Click(null, null);
            if (!this.LoggerBox.Visible)
                Log_Click(null, null);

            Thread th;
            if (Convert.ToInt32(WordsCounter.Text) == 1)
            {
                th = new Thread(thProcess1);
                th.Start();
            }
            else if (Convert.ToInt32(WordsCounter.Text) >= 2)
            {
                th = new Thread(thProcess2);
                th.Start();
            }
            else if (Convert.ToInt32(this.WordsCounter.Text) == 0)
            {
                OpenFile_Click(null, null);
                ImportWords_Click(null, null);
            }
        }
        public void thProcess2()
        {
            this.Enabled = false;
            for (int i = 0; i < words.Count; i++)
            {
                logger.Add(words[i].word + Serrialize.AddWord(api.AddWord(words[i].word, words[i].tword)));
                this.progressBar1.Value += 1;
                this.LoggerBox.ScrollToCaret();
            }

            this.Enabled = true;
        }
        public void thProcess1()
        {
            this.Enabled = false;
            logger.Add(words[0].word + Serrialize.AddWord(api.AddWord(words[0].word, words[0].tword)));
            this.progressBar1.Value += 1;

            this.Enabled = true;
        }
    }
}